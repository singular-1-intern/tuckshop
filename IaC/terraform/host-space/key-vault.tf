module "key_vault" {
  source = "./../../../submodules/neo-iac-terraform/modules/azure/neo_az_key_vault"
  # source = "./../../../../neo-iac-terraform/modules/azure/neo_az_key_vault" # Use this for local module development

  prevent_destroy = var.prevent_destroy.key_vault

  azure = {
    tenant_id       = var.azure.tenant_id
    subscription_id = var.azure.subscription_id
    location        = var.azure.location
    resource_group  = data.azurerm_resource_group.main.name
    resource_prefix = local.resource_prefix
    tags            = local.tags
  }

  name_suffix              = "hosts"
  purge_protection_enabled = var.azure.purge_protection_enabled

  principals = {
    admins = merge(
      module.aad_groups.groups.host_admins.principal_map,
      module.host_space_principal_lookups.principal_maps.provisioner_service_principals.all
    )

    managers = merge(
      module.aad_groups.groups.host_managers.principal_map,
      module.host_space_principal_lookups.principal_maps.operations_service_principals.all,
    )
  }

  certificates = merge(
    local.ops_vm_certificates,
    local.ops_vm_nginx_proxy_certificates
  )

  generated_secrets              = local.generated_secret_names
  output_generated_secret_values = true

  secret_names      = local.secret_names
  secret_values     = local.secret_values
  secret_properties = local.secret_properties

  user_secrets = local.user_secrets
}
