terraform {
  required_providers {
    azuread = {
      source  = "hashicorp/azuread"
      version = "=3.8.0"
    }
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "=4.71.0"
    }
    kubernetes = {
      source  = "hashicorp/kubernetes"
      version = "=3.1.0"
    }
    null = {
      source  = "hashicorp/null"
      version = "=3.2.4"
    }
    local = {
      source  = "hashicorp/local"
      version = "=2.8.0"
    }
    random = {
      source  = "hashicorp/random"
      version = "=3.8.1"
    }
    template = {
      source  = "hashicorp/template"
      version = "=2.2.0"
    }
    time = {
      source  = "hashicorp/time"
      version = "=0.13.1"
    }
  }

  backend "azurerm" {
    container_name = "tfstate"
    key            = "host-spaces"
  }

  required_version = "=1.3.6"
}

# Configure the Microsoft Azure Resource Manager Provider.
provider "azurerm" {
  features {}

  tenant_id       = var.azure.tenant_id
  subscription_id = var.azure.subscription_id
}

# Configure the Microsoft Azure Active Directory Provider.
provider "azuread" {
  tenant_id = var.azure.tenant_id
}

# Get the environment resource group
data "azurerm_resource_group" "main" {
  name = local.resource_group_name
}

# This provider acquires a token using your Azure CLI credentials
provider "kubernetes" {
  host                   = local.k8s_provisioned ? (local.aks_cluster.private_cluster_enabled ? join("", ["https://", local.aks_cluster.fqdn]) : local.aks_cluster.kube_admin_config.0.host) : ""
  cluster_ca_certificate = local.aks_cluster_ca_certificate

  # Use kubelogin to get an AAD token for the cluster
  exec {
    api_version = "client.authentication.k8s.io/v1beta1"
    command     = "kubelogin"

    args = var.service_principal != null ? [
      # Service Principal Login
      "get-token",
      "--environment",
      "AzurePublicCloud",
      "--server-id",
      "6dae42f8-4368-4678-94ff-3960e28e3630", # Note: The AAD server app ID of AKS Managed AAD is always 6dae42f8-4368-4678-94ff-3960e28e3630 in any environment.
      "--client-id",
      var.service_principal.client_id,
      "--client-secret",
      var.service_principal.client_secret,
      "--tenant-id",
      var.azure.tenant_id,
      "--login",
      "spn"
      ] : [
      # User Login
      "get-token",
      "--login",
      "azurecli",
      "--server-id",
      "6dae42f8-4368-4678-94ff-3960e28e3630" # Note: The AAD server app ID of AKS Managed AAD is always 6dae42f8-4368-4678-94ff-3960e28e3630 in any environment.
    ]

    # This article details different authentication methods:
    # https://spacelift.io/blog/terraform-kubernetes-provider
  }
}

# Alternative: This provider authenticates using an existing context in your kube config
# provider "kubernetes" {
#   config_path    = "~/.kube/config"
#   config_context = "sc-we-dev-aks"
# }
