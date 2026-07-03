# Create a Terraform State Storage Account
module "tfstate_storage_account" {
  source = "./../../../submodules/neo-iac-terraform/modules/azure/neo_az_storage_tfstate"
  # source = "./../../../../neo-iac-terraform/modules/azure/neo_az_storage_tfstate" # Use this for local module development

  prevent_destroy = var.prevent_destroy.storage_accounts

  azure = {
    tenant_id       = var.azure.tenant_id
    subscription_id = var.azure.subscription_id
    location        = var.azure.location
    resource_group  = local.resource_group.name
    resource_prefix = local.resource_prefix
    tags            = local.tags
  }

  tfstate = {
    enabled                 = true
    name                    = var.tfstate.use_custom_naming ? var.tfstate.name : local.tfstate_storage_account
    use_existing            = var.tfstate.use_existing
    use_custom_naming       = var.tfstate.use_custom_naming
    resource_group          = local.resource_group.name
    data_retention_settings = var.tfstate.data_retention_settings
  }
}
