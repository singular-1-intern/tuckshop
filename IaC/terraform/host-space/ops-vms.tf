locals {
  # Ops VM - Zero Trust Network Connectivity
  ops_vm_certificate_keys = {
    for name, ops_vm in var.ops_vms : name => join("-", ["ops", "vm", name, "warp-identity"]) if ops_vm.features.warp_cli != null
  }

  ops_vm_certificates = {
    for name, ops_vm in var.ops_vms : local.ops_vm_certificate_keys[name] => {
      name               = module.ops_virtual_machines[name].virtual_machine.computer_name
      subject            = "CN=${module.ops_virtual_machines[name].virtual_machine.computer_name}.singular.co.za"
      dns_names          = ["${module.ops_virtual_machines[name].virtual_machine.computer_name}.singular.co.za"]
      validity_in_months = 1200 # No expiry (100 years)
    } if ops_vm.features.warp_cli != null
  }

  ops_vm_secret_names_service_token_client_id = {
    for name, ops_vm in var.ops_vms : name => join("-", compact(["vm", "ops", name, "zero-trust-service-token-client-id"]))
    if ops_vm.features.warp_cli != null
  }

  ops_vm_secret_names_service_token_client_secret = {
    for name, ops_vm in var.ops_vms : name => join("-", compact(["vm", "ops", name, "zero-trust-service-token-client-secret"]))
    if ops_vm.features.warp_cli != null
  }

  # Ops VM - Nginx Proxy
  ops_vm_nginx_proxy_certificate_keys = {
    for name, ops_vm in var.ops_vms : name => join("-", [name, "nginx-proxy"])
  }

  ops_vm_nginx_proxy_certificates = {
    for name, ops_vm in var.ops_vms : local.ops_vm_nginx_proxy_certificate_keys[name] => {
      name               = join("-", [module.ops_virtual_machines[name].virtual_machine.computer_name, "nginx-proxy"])
      subject            = "CN=${module.ops_virtual_machines[name].virtual_machine.computer_name}"
      dns_names          = ["${module.ops_virtual_machines[name].virtual_machine.computer_name}"]
      validity_in_months = 1200 # No expiry (100 years)
    } if ops_vm.features.nginx_proxy != null
  }

  ops_vm_nginx_proxy_client_certificates = flatten([
    for name, ops_vm in var.ops_vms : [
      for cert_name in ops_vm.features.nginx_proxy.client_certificates : {
        ops_vm    = name
        cert_name = cert_name
        name      = join("-", ["vm", "ops", name, "client-certificate", cert_name])
        key       = join("-", [name, cert_name])
      }
    ] if ops_vm.features.nginx_proxy != null
  ])

  # Create an API Token per App Space for each Ops VM with an Nginx Proxy
  ops_vm_secret_names_nginx_proxy_app_space_api_token = {
    for name, item in local.ops_vm_app_spaces : name =>
    join("-", compact(["vm", "ops", item.ops_vm_name, item.app_space_prefix, "nginx-proxy-api-token"]))
    if item.ops_vm.features.nginx_proxy != null
  }

  # Get all details needed to generate Nginx Proxy location configurations
  ops_vms_nginx_proxy_locations = merge([
    for name, item in local.ops_vm_app_spaces : {
      for location_key, location in item.ops_vm.features.nginx_proxy.locations : join("-", [name, location_key]) => {
        ops_vm_app_space_key = name
        app_space_prefix     = item.app_space_prefix
        ops_vm_name          = item.ops_vm_name
        location             = location
        probe_url = location.probe != null && !contains(location.probe.excluded_app_spaces, item.app_space_prefix) ? join("/", [
          "https://${module.ops_virtual_machines[item.ops_vm_name].virtual_machine.computer_name}",
          item.app_space_prefix,
          trimprefix(location.path, "/"),
          trimprefix(location.probe.path, "/")
        ]) : null
      }
    } if item.ops_vm.features.nginx_proxy != null
  ]...)

  # Ops VM - Network Rules
  ops_vm_network_rules = length(keys(var.ops_vms)) > 0 ? concat(
    [
      # Allow communication between the VMs on the host-space subnet
      {
        name                         = "AllowInBoundWebInsideHostSpaceSubnet-OpsVm"
        priority                     = 300
        direction                    = "Inbound"
        access                       = "Allow"
        protocol                     = "Tcp"
        source_port_ranges           = ["*"]
        source_address_prefixes      = azurerm_subnet.servers[0].address_prefixes
        destination_port_ranges      = ["80", "443"] # Web
        destination_address_prefixes = azurerm_subnet.servers[0].address_prefixes
      }
    ],

    [
      for index, name in keys(var.ops_vms) : {
        name                         = "AllowInBoundSshFromInternet-OpsVm-${name}"
        priority                     = sum([310, index])
        direction                    = "Inbound"
        access                       = "Allow"
        protocol                     = "Tcp"
        source_port_ranges           = ["*"]
        source_address_prefixes      = distinct(concat(var.ips_whitelist, var.ops_vms[name].public_access.allowed_inbound_ips)) # Only allow connection attempts from known IPs
        destination_port_ranges      = ["22"]                                                                                   # SSH
        destination_address_prefixes = [local.ops_vms_ips[name]]
      } if var.ops_vms[name].public_ip.enabled && var.ops_vms[name].public_access.ssh
    ],

    [
      for index, name in keys(var.ops_vms) : {
        name                         = "AllowInBoundHttpsFromInternet-OpsVm-${name}"
        priority                     = sum([320, index])
        direction                    = "Inbound"
        access                       = "Allow"
        protocol                     = "Tcp"
        source_port_ranges           = ["*"]
        source_address_prefixes      = distinct(concat(var.ips_whitelist, var.ops_vms[name].public_access.allowed_inbound_ips)) # Only allow connection attempts from known IPs
        destination_port_ranges      = ["443"]                                                                                  # HTTPS
        destination_address_prefixes = [local.ops_vms_ips[name]]
      } if var.ops_vms[name].public_ip.enabled && var.ops_vms[name].public_access.https
    ],

    [
      for index, name in keys(var.ops_vms) : {
        name                         = "AllowOutBoundToInternet-OpsVm-${name}"
        priority                     = sum([340, index])
        direction                    = "Outbound"
        access                       = "Allow"
        protocol                     = "*"
        source_port_ranges           = ["*"]
        source_address_prefixes      = [local.ops_vms_ips[name]]
        destination_port_ranges      = ["*"]
        destination_address_prefixes = length(var.ops_vms[name].public_access.allowed_outbound_ips) > 0 ? var.ops_vms[name].public_access.allowed_outbound_ips : ["Internet"]
        # We only need to create an outbound allow rule if the custom deny rule is in place for internet traffic. Otherwise subnets allow outbound internet traffic by default.
      } if var.ops_vms[name].public_access.outbound && var.network.deny_outbound_internet_traffic
    ]
  ) : []

  ops_vm_nginx_proxy_network_rules = anytrue([for name, ops_vm in var.ops_vms : ops_vm.features.nginx_proxy != null]) ? [
    # Allow HTTP traffic from the AKS Cluster (Services subnet) to reach the Ops VM
    {
      name                         = "AllowInBoundTcpFromServicesSubnet"
      priority                     = 330
      direction                    = "Inbound"
      access                       = "Allow"
      protocol                     = "Tcp"
      source_port_ranges           = ["*"]
      source_address_prefixes      = local.shared_host_state.network.subnets["services"].address_prefixes
      destination_port_ranges      = ["80", "443", "9100"] # Web and Node Exporter
      destination_address_prefixes = ["*"]
    }
  ] : []

  ops_vms_with_node_exporter = { for name, ops_vm in var.ops_vms : name => ops_vm if ops_vm.features.node_exporter }
}

module "ops_virtual_machines" {
  source = "./../../../submodules/neo-iac-terraform/modules/azure/neo_az_virtual_machine"
  # source = "./../../../../neo-iac-terraform/modules/azure/neo_az_virtual_machine" # Use this for local module development

  for_each = { for name, ops_vm in var.ops_vms : name => ops_vm }

  prevent_destroy = var.prevent_destroy.ops_machines

  azure = {
    location        = var.azure.location
    resource_group  = local.resource_group_name
    resource_prefix = local.resource_prefix
    tags            = local.tags
  }

  virtual_machine = {
    name           = each.key
    size           = each.value.vm_size
    is_windows     = false
    public_ssh_key = file(replace(each.value.public_ssh_key, "{path.module}", path.module))

    enable_boot_diagnostics = false # If we want to enable this, a storage account is needed
    install_aad_integration = each.value.install_aad_integration
    install_azure_policy    = false # The VM terraform module doesn't current support installing this on linux
  }

  secrets = {
    # This password can be used to connect, but not sure if you have to have the SSH key on your machine too.
    admin_password = module.key_vault.generated_secret_values[local.ops_vm_admin_password_secret_names[each.key]]
  }

  image = {
    publisher = each.value.image.publisher
    offer     = each.value.image.offer
    sku       = each.value.image.sku
    version   = each.value.image.version
  }

  os_disk = {
    storage_type = each.value.storage.type
    size_gb      = each.value.storage.size
  }

  data_disks = []

  network = {
    subnet_id = azurerm_subnet.servers[0].id
    nsg_id    = azurerm_network_security_group.servers[0].id
  }

  public_ip = {
    enabled           = each.value.public_ip.enabled
    link_nic          = each.value.public_ip.link_nic
    allocation_method = each.value.public_ip.allocation_method
    sku               = each.value.public_ip.sku
  }

  private_ip = local.ops_vms_ips[each.key]

  principals = {
    admins = [module.aad_groups.groups.host_admins.object_id]
    users  = []
  }
}

# Public Kubernetes Secrets for Nginx Proxy on Ops VMs to App Space namespaces
# (SSL Certificate and API Key)
resource "kubernetes_secret_v1" "nginx_proxy_secrets" {
  for_each = { for name, item in local.ops_vm_app_spaces : name => item if item.ops_vm.features.nginx_proxy != null }

  metadata {
    name      = join("-", [each.value.app_space.k8s_resource_prefix, "secret", "nginx-proxy", module.ops_virtual_machines[each.value.ops_vm_name].virtual_machine.computer_name])
    namespace = each.value.app_space.k8s_namespace
  }

  type = "Opaque"

  data = {
    "nginx-proxy.crt" = data.azurerm_key_vault_certificate_data.ops_vm_nginx_proxy_certificates_data[each.value.ops_vm_name].pem
    "hostname.txt"    = module.ops_virtual_machines[each.value.ops_vm_name].virtual_machine.computer_name
    "private-ip.txt"  = module.ops_virtual_machines[each.value.ops_vm_name].network.private_ip
    "port.txt"        = "443"
    "base-path.txt"   = "/${each.value.app_space_prefix}"
    "base-url.txt"    = "https://${module.ops_virtual_machines[each.value.ops_vm_name].virtual_machine.computer_name}/${each.value.app_space_prefix}"
    "api-key.txt"     = base64encode(data.azurerm_key_vault_secret.ops_vm_nginx_proxy_app_space_api_token_secrets[each.key].value)
  }

  depends_on = [module.key_vault]
}
