locals {
  output = {
    # Generate output objects which are used in both module output and as part of the config export
    project = local.host_space_base_state.project

    prefixes = local.host_space_base_state.prefixes

    azure = {
      tenant_id       = var.azure.tenant_id
      subscription_id = var.azure.subscription_id
      location        = var.azure.location
      location_prefix = var.azure.location_prefix
    }

    aad_groups = { for key, group in module.aad_groups.groups : key =>
      {
        id   = group.object_id
        name = group.display_name
      }
    }

    resource_group = {
      id   = data.azurerm_resource_group.main.id
      name = data.azurerm_resource_group.main.name
    }

    network = length(keys(var.sql_servers)) > 0 ? tomap({
      subnet = {
        id             = azurerm_subnet.servers[0].id
        name           = azurerm_subnet.servers[0].name
        resource_group = azurerm_subnet.servers[0].resource_group_name
      }
      nsg = {
        id             = azurerm_network_security_group.servers[0].id
        name           = azurerm_network_security_group.servers[0].name
        resource_group = azurerm_network_security_group.servers[0].resource_group_name
      }
    }) : tomap({})

    sql_servers = {
      for name, sql_server in module.sql_server_base_virtual_machines : name => merge(sql_server, module.mssql_iaas_extension[name])
    }

    ops_vms = {
      for name, ops_vm in module.ops_virtual_machines : name => merge(
        ops_vm,
        var.ops_vms[name].features.warp_cli != null ? {
          warp_cli = {
            identity_certificate = {
              name       = local.ops_vm_certificates[local.ops_vm_certificate_keys[name]].name
              id         = module.key_vault.certificates[local.ops_vm_certificate_keys[name]].name
              thumbprint = module.key_vault.certificates[local.ops_vm_certificate_keys[name]].thumbprint
              sha256     = module.key_vault.certificates[local.ops_vm_certificate_keys[name]].sha
              location   = "~/.config/device_posture/${module.ops_virtual_machines[name].virtual_machine.computer_name}.cer"
            }
          }
        } : {}
      )
    }

    key_vault = can(module.key_vault.key_vault.id) ? {
      vault             = module.key_vault.key_vault
      certificates      = module.key_vault.certificates
      rsa_keys          = module.key_vault.rsa_keys
      secrets           = module.key_vault.secrets
      generated_secrets = module.key_vault.generated_secrets
      user_secrets      = module.key_vault.user_secrets
    } : null

    recovery_services_vault = var.backups.recovery_services_vault.enabled && can(module.recovery_services_vault[0].recovery_services_vault) ? module.recovery_services_vault[0] : null

    container_registry = var.container_registry.enabled ? tomap({
      id           = azurerm_container_registry.container_registry[0].id
      name         = azurerm_container_registry.container_registry[0].name
      login_server = azurerm_container_registry.container_registry[0].login_server
    }) : null
  }
}

output "project" {
  value = local.output.project
}

output "prefixes" {
  value = local.output.prefixes
}

output "azure" {
  value = local.output.azure
}

output "aad_groups" {
  value = local.output.aad_groups
}

output "network" {
  value = local.output.network
}

output "sql_servers" {
  value = local.output.sql_servers
}

output "ops_vms" {
  value = local.output.ops_vms
}

output "key_vault" {
  value = local.output.key_vault
}

output "recovery_services_vault" {
  value = local.output.recovery_services_vault
}

output "container_registry" {
  value     = local.output.container_registry
  sensitive = true
}

output "config_export" {
  value = {
    file_path   = local.config_export_file
    file_format = var.config_export.file_format
  }
}
