resource "azurerm_servicebus_namespace" "service_bus" {
  count = var.service_bus.enabled ? 1 : 0

  name                = local.service_bus_name
  location            = local.resource_group.location
  resource_group_name = local.resource_group.name
  sku                 = var.service_bus.sku
}

resource "azurerm_role_assignment" "identities_service_bus_owner" {
  for_each = var.service_bus.enabled ? local.apps_that_require_identities : {}

  scope                = azurerm_servicebus_namespace.service_bus[0].id
  role_definition_name = "Azure Service Bus Data Owner"
  principal_id         = azurerm_user_assigned_identity.app_identities[each.key].principal_id
}
