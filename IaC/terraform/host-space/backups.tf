module "recovery_services_vault" {
  source = "./../../../submodules/neo-iac-terraform/modules/azure/neo_az_recovery_services_vault"
  # source = "./../../../../neo-iac-terraform/modules/azure/neo_az_recovery_services_vault" # Use this for local module development

  count = var.backups.recovery_services_vault.enabled ? 1 : 0

  prevent_destroy = var.prevent_destroy.recovery_services_vault

  azure = {
    location        = var.azure.location
    resource_group  = data.azurerm_resource_group.main.name
    resource_prefix = local.resource_prefix
    tags            = local.tags
  }

  vault = {
    soft_delete_enabled = var.backups.recovery_services_vault.soft_delete_enabled
  }

  alerts = {
    enabled               = var.backups.recovery_services_vault.alerts.enabled
    emails                = var.backups.recovery_services_vault.alerts.emails
    alert_context_filters = var.backups.recovery_services_vault.alerts.alert_context_filters
  }

  sql_server_policies = var.backup_policies.sql_server_policies
}
