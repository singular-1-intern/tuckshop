# Export environment information for deployment scripting to use
locals {
  config_export_file         = "${path.root}/${var.config_export.file_folder}/${var.azure.location_prefix}/${var.config_export.file_prefix}.${local.environment_prefix}.${var.config_export.file_format}"
  config_export_data         = local.output
  config_export_data_encoded = var.config_export.file_format == "yaml" ? yamlencode(local.config_export_data) : jsonencode(local.config_export_data)
}

resource "local_file" "config_export" {
  count = var.config_export.enabled && var.config_export.file_export_enabled ? 1 : 0

  content  = local.config_export_data_encoded
  filename = local.config_export_file
}

resource "azurerm_automation_variable_string" "host_space_state" {
  count = var.config_export.enabled && var.config_export.automation_account_export_enabled ? 1 : 0

  name                    = "host-space-state"
  resource_group_name     = local.resource_group_name
  automation_account_name = local.automation_account_name
  value                   = jsonencode(local.config_export_data)
}
