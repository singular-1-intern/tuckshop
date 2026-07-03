# Retrieve the shared host state
data "azurerm_automation_variable_string" "shared_host_state" {
  name                    = "host-state"
  resource_group_name     = local.host_space_base_state.shared_hosts_automation_account.resource_group
  automation_account_name = local.host_space_base_state.shared_hosts_automation_account.name
}

# Retrieve the host space state
data "azurerm_automation_variable_string" "host_space_base_state" {
  name                    = "host-space-base-state"
  resource_group_name     = local.resource_group_name
  automation_account_name = local.automation_account_name
}

# Get the details for the AKS cluster (if provisioned)
data "azurerm_kubernetes_cluster" "main" {
  count = local.k8s_provisioned ? 1 : 0

  name                = local.aks_cluster_resource.name
  resource_group_name = local.aks_cluster_resource.resource_group
}
