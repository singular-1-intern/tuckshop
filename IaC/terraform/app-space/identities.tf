resource "azurerm_user_assigned_identity" "app_identities" {
  for_each = local.apps_that_require_identities

  resource_group_name = local.resource_group.name
  location            = var.azure.location
  name                = join("-", [local.resource_prefix, "ids", each.value.name])
}

resource "azurerm_federated_identity_credential" "app_identity_federated_credentials" {
  for_each = local.k8s_provisioned && local.k8s_namespace != null ? local.apps_that_require_identities : {}

  name                = join("-", [local.resource_prefix, "fedcr", each.value.name])
  resource_group_name = local.resource_group_name
  audience            = ["api://AzureADTokenExchange"]
  issuer              = local.aks_cluster_state.aks.oidc_issuer_url
  parent_id           = azurerm_user_assigned_identity.app_identities[each.key].id
  subject             = join(":", ["system", "serviceaccount", local.k8s_namespace.namespace.name, join("-", [local.k8s_resource_prefix, each.value.name, "neo-web-app-service-account"])])
}
