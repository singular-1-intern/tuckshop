
locals {
  can_not_delete_resource_ids = compact(flatten([
    # For resources that could be empty (non-existing), either the resource ID (if enabled), otherwise we use null.
    (var.app_services.enabled ? azurerm_service_plan.main[0].id : null),
    (var.service_bus.enabled ? azurerm_servicebus_namespace.service_bus[0].id : null),

    # For groups of resources, we conditionally check for and aggregate their IDs.
    length(azurerm_linux_web_app.app_services) > 0 ?
    [for app in azurerm_linux_web_app.app_services : app.id] : [],

    length(azurerm_storage_account.storage_accounts) > 0 ?
    [for storage in azurerm_storage_account.storage_accounts : storage.id] : [],

    length(azurerm_user_assigned_identity.app_identities) > 0 ?
    [for identity in azurerm_user_assigned_identity.app_identities : identity.id] : [],
  ]))

  read_only_resource_ids = compact(flatten([]))
}

module "resource_locks" {
  source = "./../../../submodules/neo-iac-terraform/modules/azure/neo_az_resource_locks"
  # source = "./../../../../neo-iac-terraform/modules/azure/neo_az_resource_locks" # Use this for local module development

  count = var.prevent_destroy.resource_locks ? 1 : 0

  can_not_delete_resource_ids = local.can_not_delete_resource_ids
  read_only_resource_ids      = local.read_only_resource_ids
}
