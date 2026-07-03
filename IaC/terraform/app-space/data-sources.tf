# Get the environment resource group
data "azurerm_resource_group" "main" {
  count = var.resource_group.use_existing ? 1 : 0

  name = local.resource_group_name
}

# Get the state storage account
data "azurerm_storage_account" "tfstate" {
  count = var.tfstate.use_existing ? 1 : 0

  name                = var.tfstate.use_custom_naming ? var.tfstate.name : local.tfstate_storage_account
  resource_group_name = local.resource_group.name

  depends_on = [module.tfstate_storage_account]
}

# Retrieve the app space base state
data "azurerm_automation_variable_string" "app_space_base_state" {
  name                    = "app-space-base-state"
  resource_group_name     = local.resource_group_name
  automation_account_name = local.automation_account_name
}

# Retrieve the shared hosts state
data "azurerm_automation_variable_string" "shared_hosts_state" {
  count = local.is_standalone ? 0 : 1

  name                    = "host-state"
  resource_group_name     = local.app_space_base_state.shared_hosts_automation_account.resource_group
  automation_account_name = local.app_space_base_state.shared_hosts_automation_account.name
}

# If a Kubernetes Cluster is provisioned, get its details
data "azurerm_kubernetes_cluster" "main" {
  count = local.k8s_provisioned ? 1 : 0

  name                = local.aks_cluster_state.aks.name
  resource_group_name = local.aks_cluster_state.aks.resource_group
}

# If a Cloudflare API Token secret is available, retrieve it
data "kubernetes_secret_v1" "cloudflare_api_token" {
  count = local.k8s_provisioned && local.k8s_namespace != null ? contains(keys(local.k8s_namespace.secrets), "cloudflare-api-token") ? 1 : 0 : 0

  metadata {
    name      = local.k8s_namespace.secrets["cloudflare-api-token"].name
    namespace = local.k8s_namespace.namespace.name
  }
}

locals {
  # If no Cloudflare token is available, we must set a fake value to prevent a provider validation error.
  # (We must also ensure that the module is not configured to create any Cloudflare resources if no token is available)
  cloudflare_api_token = length(data.kubernetes_secret_v1.cloudflare_api_token) > 0 ? data.kubernetes_secret_v1.cloudflare_api_token[0].data["api-token"] : "1234567890123456789012345678901234567890"
}
