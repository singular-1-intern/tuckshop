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
    cloudflare = {
      source  = "cloudflare/cloudflare"
      version = "=5.19.1"
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
    time = {
      source  = "hashicorp/time"
      version = "=0.13.1"
    }
  }

  # Note: This can only be enabled after the first terraform run of this module, as the tfstate storage account is created by this root module.
  #       On the next run, Terraform will ask you if you want to migrate the state to the storage account.
  backend "azurerm" {
    resource_group_name  = join("-", [var.prefixes.project, terraform.workspace, "rg"])
    storage_account_name = join("", [var.prefixes.project, terraform.workspace, "st", "tfstate"])
    container_name       = "tfstate"
    key                  = "app-spaces"
  }

  required_version = "=1.3.6"
}

# Configure the Microsoft Azure Resource Manager Provider.
provider "azurerm" {
  features {}

  # Enable this if you are in an access-restricted environment where you cannot register new providers
  # skip_provider_registration = true # This is required for the azurerm provider to work with certain access-restricted client cloud setups.
  tenant_id       = var.azure.tenant_id
  subscription_id = var.azure.subscription_id
  # Terraform uses Shared Key Authorisation to provision Storage Containers, Blobs and other items.
  # See: https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs#storage_use_azuread-1
  # NOTE: Document Restore DR processes make use of SAS Tokens.
  storage_use_azuread = local.use_storage_azuread # Should the AzureRM Provider use AzureAD to connect to the Storage Blob & Queue APIs, rather than the SharedKey from the Storage Account?
}

# This provider acquires a token using your Azure CLI credentials
provider "kubernetes" {
  host                   = local.k8s_provisioned ? (local.aks_cluster.private_cluster_enabled ? join("", ["https://", local.aks_cluster_state.api_server.fqdn]) : local.aks_cluster.kube_config.0.host) : ""
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

# Configure the Cloudflare Provider.
# (When provisioning App Service, this needs to be set as an environment variable on the command line: $Env:CLOUDFLARE_API_TOKEN = "{TOKEN_VALUE}"")
provider "cloudflare" {
  # If the API token is not available, set to null to allow loading via the CLOUDFLARE_API_TOKEN environment variable.
  api_token = coalesce(local.cloudflare_api_token, null)
}
