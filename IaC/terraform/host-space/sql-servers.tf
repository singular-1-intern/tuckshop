locals {
  sql_server_security_rules = merge(
    # SQL Server Inbound Rules
    {
      for index, name in keys(var.sql_servers) : name =>
      {
        name                         = "AllowInBoundFromInternet-Sql-${module.sql_server_base_virtual_machines[name].virtual_machine.computer_name}"
        priority                     = sum([1000, index])
        direction                    = "Inbound"
        access                       = "Allow"
        protocol                     = "Tcp"
        source_port_ranges           = ["*"]
        source_address_prefixes      = distinct(concat(var.ips_whitelist, var.sql_servers[name].public_access.allowed_inbound_ips))
        destination_port_ranges      = compact([var.sql_servers[name].public_access.sql ? "1433" : "", var.sql_servers[name].public_access.rdp ? "3389" : ""])
        destination_address_prefixes = [local.sql_server_ips[name]]
      } if var.sql_servers[name].public_ip.enabled && (var.sql_servers[name].public_access.sql || var.sql_servers[name].public_access.rdp)
    },
    # SQL Server Outbound Rules
    {
      for index, name in keys(var.sql_servers) : "${name}-outbound" =>
      {
        name                         = "AllowOutBoundToInternet-Sql-${module.sql_server_base_virtual_machines[name].virtual_machine.computer_name}"
        priority                     = sum([1010, index])
        direction                    = "Outbound"
        access                       = "Allow"
        protocol                     = "*"
        source_port_ranges           = ["*"]
        source_address_prefixes      = [local.sql_server_ips[name]]
        destination_port_ranges      = ["*"]
        destination_address_prefixes = length(var.sql_servers[name].public_access.allowed_outbound_ips) > 0 ? var.sql_servers[name].public_access.allowed_outbound_ips : ["Internet"]
        # We only need to create an outbound allow rule if the custom deny rule is in place for internet traffic. Otherwise subnets allow outbound internet traffic by default.
      } if var.sql_servers[name].public_access.outbound && var.network.deny_outbound_internet_traffic
    }
  )
}

module "sql_server_base_virtual_machines" {
  source = "./../../../submodules/neo-iac-terraform/modules/azure/neo_az_virtual_machine"
  # source = "./../../../../neo-iac-terraform/modules/azure/neo_az_virtual_machine" # Use this for local module development

  for_each = { for name, sql_server in var.sql_servers : name => sql_server }

  prevent_destroy = var.prevent_destroy.sql_machines

  azure = {
    location        = var.azure.location
    resource_group  = data.azurerm_resource_group.main.name
    resource_prefix = local.resource_prefix
    tags            = local.tags
  }

  virtual_machine = {
    name                    = each.key
    size                    = each.value.vm_size
    is_windows              = true
    enable_boot_diagnostics = true
    install_aad_integration = each.value.virtual_machine.install_aad_integration
    install_azure_policy    = each.value.virtual_machine.install_azure_policy
    vm_patch_mode           = each.value.virtual_machine.vm_patch_mode

    computer_name = {
      custom_resource_prefix = each.value.short_computer_name ? local.short_resource_prefix : ""
      exclude_resource_type  = true
    }
    admin_username = {
      custom_resource_prefix = local.short_resource_prefix
      suffix                 = each.value.short_admin_username ? "adm" : "admin"
    }
  }

  secrets = {
    admin_password = module.key_vault.generated_secret_values[local.sql_server_admin_password_secret_names[each.key]]
  }

  image = {
    offer     = each.value.image.offer
    publisher = each.value.image.publisher
    sku       = each.value.image.sku
    version   = each.value.image.version
  }

  os_disk = {
    storage_type = each.value.os_disk_type
    size_gb      = each.value.os_disk_size
  }

  data_disks = [
    {
      name         = "data"
      storage_type = each.value.data_disk_type
      size_gb      = each.value.data_disk_size
      lun          = "10"
      caching      = "ReadOnly"
    },
    {
      name         = "logs"
      storage_type = each.value.logs_disk_type
      size_gb      = each.value.logs_disk_size
      lun          = "20"
      caching      = "None"
    }
  ]

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

  private_ip = local.sql_server_ips[each.key]

  shutdown_schedule = {
    enabled               = each.value.shutdown_schedule.enabled
    daily_recurrence_time = each.value.shutdown_schedule.daily_recurrence_time
    timezone              = each.value.shutdown_schedule.timezone
  }

  principals = {
    admins = [module.aad_groups.groups.host_admins.object_id]
    users  = [module.aad_groups.groups.host_managers.object_id]
  }

  recovery_services_vault = each.value.recovery_services_vault
}

module "mssql_iaas_extension" {
  source = "./../../../submodules/neo-iac-terraform/modules/azure/neo_az_mssql_iaas_agent"
  # source = "./../../../../neo-iac-terraform/modules/azure/neo_az_mssql_iaas_agent" # Use this for local module development

  for_each = { for name, sql_server in var.sql_servers : name => sql_server if sql_server.sql_iaas_enabled }

  azure = {
    location        = var.azure.location
    resource_group  = data.azurerm_resource_group.main.name
    resource_prefix = local.resource_prefix
    tags            = local.tags
  }

  virtual_machine = {
    id            = module.sql_server_base_virtual_machines[each.key].virtual_machine.id
    computer_name = module.sql_server_base_virtual_machines[each.key].virtual_machine.computer_name
  }

  iaas_agent = {
    data_disk_luns = [10]
    logs_disk_luns = [20]
  }

  credentials = {
    admin = {
      username          = module.sql_server_base_virtual_machines[each.key].virtual_machine.admin_username
      password          = module.key_vault.generated_secret_values[local.sql_server_admin_password_secret_names[each.key]]
      admin_secret_name = local.sql_server_admin_password_secret_names[each.key]
    }
    application = {
      username                = local.sql_server_app_usernames[each.key]
      password                = module.key_vault.generated_secret_values[local.sql_server_app_password_secret_names[each.key]]
      application_secret_name = local.sql_server_app_password_secret_names[each.key]
    }
  }

  config_script_settings = {
    restart_delay_seconds = 45
  }

  sql_enable_flags = {
    enable_secure_enclaves = each.value.enable_secure_enclaves
  }
}

resource "azurerm_network_security_rule" "sql_server_rules" {
  for_each = local.sql_server_security_rules

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
