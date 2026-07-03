locals {
  can_not_delete_resource_ids = compact(flatten([
    # For resources that could be empty (non-existing), either the resource ID (if enabled), otherwise we use null.
    (var.container_registry.enabled ? azurerm_container_registry.container_registry[0].id : null),
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
