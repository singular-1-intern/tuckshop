# Either create the host resource group or use an existing one
resource "azurerm_resource_group" "main" {
  count = var.resource_group.use_existing ? 0 : 1

  name     = local.resource_group_name
  location = local.resource_group_location

  tags = local.tags
}
