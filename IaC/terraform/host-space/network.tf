locals {
  network_security_rules = concat(
    var.network.enable_private_ips_deny_rule ? [
      # By default, on Network Security Groups, Azure adds a "Deny All" rule followed by an allow rule for all private network traffic.
      # This rule re-introduces the deny on all private traffic. If this is enabled, the consuming module must then specify additional
      # rules which allow traffic on a more granular basis.
      {
        name                         = "DenyPrivateNetworkIps"
        priority                     = 4096
        direction                    = "Inbound"
        access                       = "Deny"
        protocol                     = "*"
        source_port_ranges           = ["*"]
        source_address_prefixes      = local.private_ip_ranges
        destination_port_ranges      = ["*"]
        destination_address_prefixes = ["*"]
      }
    ] : [],

    var.network.deny_outbound_internet_traffic && var.network.default_outbound_access_enabled == false ? [
      # If deny outbound internet traffic is enabled, only add the rule if default outbound access is disabled.
      # (VMs must then be individually configured to allow outbound traffic as required)
      {
        name                         = "DenyAllOutboundTraffic"
        priority                     = 4095
        direction                    = "Outbound"
        access                       = "Deny"
        protocol                     = "*"
        source_port_ranges           = ["*"]
        source_address_prefixes      = ["*"]
        destination_port_ranges      = ["*"]
        destination_address_prefixes = ["Internet"]
      }
    ] : [],

    # Private ZTN Access: SQL, RDP, Windows Exporter
    contains(keys(local.shared_host_state.network.subnets), "public") ? [
      {
        name                         = "AllowInBoundSqlAndRdpFromPublicSubnet"
        priority                     = 100
        direction                    = "Inbound"
        access                       = "Allow"
        protocol                     = "Tcp"
        source_port_ranges           = ["*"]
        source_address_prefixes      = local.shared_host_state.network.subnets["public"].address_prefixes
        destination_port_ranges      = ["1433", "3389"]
        destination_address_prefixes = ["*"]
      },
      {
        name                         = "AllowInBoundPrometheusMetricsFromPublicSubnet"
        priority                     = 110
        direction                    = "Inbound"
        access                       = "Allow"
        protocol                     = "Tcp"
        source_port_ranges           = ["*"]
        source_address_prefixes      = local.shared_host_state.network.subnets["public"].address_prefixes
        destination_port_ranges      = ["9182"]
        destination_address_prefixes = ["*"]
      }
    ] : [],

    # Private ZTN Access: SSH
    contains(keys(local.shared_host_state.network.subnets), "public") &&
    anytrue([for ops_vm in var.ops_vms : ops_vm.private_access.ssh]) ? [
      {
        name                         = "AllowInBoundSshFromPublicSubnet"
        priority                     = 120
        direction                    = "Inbound"
        access                       = "Allow"
        protocol                     = "Tcp"
        source_port_ranges           = ["*"]
        source_address_prefixes      = local.shared_host_state.network.subnets["public"].address_prefixes
        destination_port_ranges      = ["22"]
        destination_address_prefixes = ["*"]
      }
    ] : [],

    # Private ZTN Access: HTTPS
    contains(keys(local.shared_host_state.network.subnets), "public") &&
    anytrue([for ops_vm in var.ops_vms : ops_vm.private_access.https]) ? [
      {
        name                         = "AllowInBoundHttpsFromPublicSubnet"
        priority                     = 130
        direction                    = "Inbound"
        access                       = "Allow"
        protocol                     = "Tcp"
        source_port_ranges           = ["*"]
        source_address_prefixes      = local.shared_host_state.network.subnets["public"].address_prefixes
        destination_port_ranges      = ["443"]
        destination_address_prefixes = ["*"]
      }
    ] : [],
    contains(keys(local.shared_host_state.network.subnets), "services") ? [
      {
        name                         = "AllowInBoundSqlFromServicesSubnet"
        priority                     = 200
        direction                    = "Inbound"
        access                       = "Allow"
        protocol                     = "Tcp"
        source_port_ranges           = ["*"]
        source_address_prefixes      = local.shared_host_state.network.subnets["services"].address_prefixes
        destination_port_ranges      = ["1433"]
        destination_address_prefixes = ["*"]
      },
      {
        name                         = "AllowInBoundPrometheusMetricsFromServicesSubnet"
        priority                     = 210
        direction                    = "Inbound"
        access                       = "Allow"
        protocol                     = "Tcp"
        source_port_ranges           = ["*"]
        source_address_prefixes      = local.shared_host_state.network.subnets["services"].address_prefixes
        destination_port_ranges      = ["9182"]
        destination_address_prefixes = ["*"]
      }
    ] : [],
    local.ops_vm_network_rules,
    local.ops_vm_nginx_proxy_network_rules,
    var.network.additional_subnet_security_rules
  )
}


# Create a Subnet under the shared virtual network using the host space CIDR
resource "azurerm_subnet" "servers" {
  count = local.networking_enabled ? 1 : 0

  name                = join("-", [local.resource_prefix, "snet", local.network_subnet_name])
  resource_group_name = local.shared_host_state.resource_group.name

  virtual_network_name = local.shared_host_state.network.vnet.name
  address_prefixes     = [local.host_space_base_state.cidr_range]
  service_endpoints    = []

  default_outbound_access_enabled = var.network.default_outbound_access_enabled
}

resource "azurerm_network_security_group" "servers" {
  count = local.networking_enabled ? 1 : 0

  name                = join("-", [local.resource_prefix, "nsg", local.network_subnet_name])
  tags                = local.tags
  location            = var.azure.location
  resource_group_name = data.azurerm_resource_group.main.name
}

resource "azurerm_subnet_network_security_group_association" "servers" {
  count = local.networking_enabled ? 1 : 0

  subnet_id                 = azurerm_subnet.servers[0].id
  network_security_group_id = azurerm_network_security_group.servers[0].id
}

# If a NAT Gateway is present and the option is enabled, link the servers subnet to the NAT Gateway.
resource "azurerm_subnet_nat_gateway_association" "servers" {
  count = var.network.link_subnets_to_nat_gateway_if_present && local.nat_gateway_available ? 1 : 0

  subnet_id      = azurerm_subnet.servers[0].id
  nat_gateway_id = local.nat_gateway_id
}

resource "azurerm_network_security_rule" "rules" {
  for_each = { for rule in local.network_security_rules : join("-", [local.network_subnet_name, rule.name]) => rule if local.networking_enabled }

  resource_group_name         = data.azurerm_resource_group.main.name
  network_security_group_name = azurerm_network_security_group.servers[0].name

  name      = each.value.name
  priority  = each.value.priority
  direction = each.value.direction
  access    = each.value.access
  protocol  = each.value.protocol

  source_port_range            = length(each.value.source_port_ranges) == 1 ? each.value.source_port_ranges[0] : null
  source_port_ranges           = length(each.value.source_port_ranges) != 1 ? each.value.source_port_ranges : null
  source_address_prefix        = length(each.value.source_address_prefixes) == 1 ? each.value.source_address_prefixes[0] : null
  source_address_prefixes      = length(each.value.source_address_prefixes) != 1 ? each.value.source_address_prefixes : null
  destination_port_range       = length(each.value.destination_port_ranges) == 1 ? each.value.destination_port_ranges[0] : null
  destination_port_ranges      = length(each.value.destination_port_ranges) != 1 ? each.value.destination_port_ranges : null
  destination_address_prefix   = length(each.value.destination_address_prefixes) == 1 ? each.value.destination_address_prefixes[0] : null
  destination_address_prefixes = length(each.value.destination_address_prefixes) != 1 ? each.value.destination_address_prefixes : null
}
