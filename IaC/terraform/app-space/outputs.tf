locals {
  output = {
    # Generate output objects which are used in both module output and as part of the config export
    project = local.is_standalone ? {
      label  = var.project_label
      prefix = var.prefixes.project
    } : local.app_space_base_state.project

    prefixes = local.is_standalone ? {
      app_space_environment    = local.environment_prefix
      app_space_project        = var.prefixes.project
      host_space_environment   = null
      host_space_project       = null
      shared_hosts_environment = null
      shared_hosts_project     = null
    } : local.app_space_base_state.prefixes

    azure = {
      tenant_id       = var.azure.tenant_id
      subscription_id = var.azure.subscription_id
      location        = var.azure.location
      location_prefix = var.azure.location_prefix
    }

    aad_groups = module.aad_groups != null ? {
      for key, group in module.aad_groups.groups : key =>
      {
        id   = group.object_id
        name = group.display_name
      }
    } : null

    resource_group = {
      id   = local.resource_group.id
      name = local.resource_group.name
    }

    apps = {
      for app in var.apps : app.name => {
        sub_domain = try(coalesce(app.sub_domain, local.default_app_sub_domain), "")
        identity = app.requires_identity ? {
          id           = azurerm_user_assigned_identity.app_identities[app.name].id
          principal_id = azurerm_user_assigned_identity.app_identities[app.name].principal_id
          client_id    = azurerm_user_assigned_identity.app_identities[app.name].client_id
        } : null
      }
    }

    key_vault = var.key_vault.enabled ? module.key_vault[0] : null

    service_bus = var.service_bus.enabled ? {
      id       = azurerm_servicebus_namespace.service_bus[0].id
      name     = local.service_bus_name
      endpoint = azurerm_servicebus_namespace.service_bus[0].endpoint
      fqdn     = regex("^[a-z]+://([\\w-_.]+)", azurerm_servicebus_namespace.service_bus[0].endpoint)[0]
    } : null

    storage_accounts = var.storage_accounts != null ? {
      for storage_account in var.storage_accounts : storage_account.name =>
      {
        id               = azurerm_storage_account.storage_accounts[storage_account.name].id
        name             = azurerm_storage_account.storage_accounts[storage_account.name].name
        clear_operations = storage_account.clear_operations
        copy_operations  = storage_account.copy_operations
      }
    } : null

    app_services = var.app_services.enabled ? {
      plan = {
        id   = azurerm_service_plan.main[0].id
        name = azurerm_service_plan.main[0].name
      }

      services = { for app in var.apps : app.name =>
        {
          id   = azurerm_linux_web_app.app_services[app.name].id
          name = azurerm_linux_web_app.app_services[app.name].name
        }
      }

      dns_records = var.dns_zone.enabled ? concat(
        [for app in var.apps : {
          app   = app.name
          type  = cloudflare_dns_record.app_services_cname_dns_records[app.name].type
          name  = cloudflare_dns_record.app_services_cname_dns_records[app.name].name
          value = cloudflare_dns_record.app_services_cname_dns_records[app.name].content
        }],
        [for app in var.apps : {
          app   = app.name
          type  = cloudflare_dns_record.app_services_txt_dns_records[app.name].type
          name  = cloudflare_dns_record.app_services_txt_dns_records[app.name].name
          value = cloudflare_dns_record.app_services_txt_dns_records[app.name].content
        }]
      ) : null
    } : null

    dns_zone = var.dns_zone.enabled ? {
      name               = var.dns_zone.domain
      primary_sub_domain = var.dns_zone.primary_sub_domain
      dns_records = [
        for dns_record in local.dns_records : {
          type  = dns_record.type
          name  = dns_record.name
          value = dns_record.value
        }
      ]
    } : null

    backup_vault = var.backup_vault.enabled && length(var.storage_accounts) > 0 ? {
      id   = azurerm_data_protection_backup_vault.main[0].id
      name = azurerm_data_protection_backup_vault.main[0].name
    } : null

    protected_resource_ids = {
      do_not_delete_locks = (var.prevent_destroy.resource_locks && local.can_not_delete_resource_ids != null) ? local.can_not_delete_resource_ids : null
      read_only_locks     = (var.prevent_destroy.resource_locks && local.read_only_resource_ids != null) ? local.read_only_resource_ids : null
    }

    sql_servers = var.sql_servers
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

output "resource_group" {
  value = local.output.resource_group
}

output "apps" {
  value = local.output.apps
}

output "key_vault" {
  value = local.output.key_vault
}

output "service_bus" {
  value = local.output.service_bus
}

output "storage" {
  value = local.output.storage_accounts
}

output "app_services" {
  value     = local.output.app_services
  sensitive = true
}

output "dns_zone" {
  value = local.output.dns_zone
}

output "backup_vault" {
  value = local.output.backup_vault
}

output "protected_resource_ids" {
  value = local.output.protected_resource_ids
}

output "config_export" {
  value = {
    file_path   = local.config_export_file
    file_format = var.config_export.file_format
  }
}

output "sql_servers" {
  value = local.output.sql_servers
}
