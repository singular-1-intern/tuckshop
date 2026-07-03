# Get the Principal IDs for users to add to the Host Space groups
module "host_space_principal_lookups" {
  source = "./../../../submodules/neo-iac-terraform/modules/azure/neo_az_principals_lookup"
  # source = "./../../../../neo-iac-terraform/modules/azure/neo_az_principals_lookup" # Use this for local module development

  principal_sets = {
    provisioner_service_principals = { service_principals = local.service_principal_names.provisioners }
    deployer_service_principals    = { service_principals = local.service_principal_names.deployers }
    operations_service_principals  = { service_principals = local.service_principal_names.operations }

    host_admins       = { users = var.access.host_admins },
    host_managers     = { users = var.access.host_managers },
    host_readers      = { users = var.access.host_readers },
    host_data_writers = { users = var.access.host_data_writers },
    host_data_readers = { users = var.access.host_data_readers }
  }
}

# Assign principals to Host Space groups
# Additional role assignments can also be added to these groups against resources created by this module.
module "aad_groups" {
  source = "./../../../submodules/neo-iac-terraform/modules/azure/neo_az_aad_groups"
  # source = "./../../../../neo-iac-terraform/modules/azure/neo_az_aad_groups" # Use this for local module development

  resource_prefix = local.resource_prefix

  aad_groups = [
    {
      name       = "host_admins"
      existing   = true
      principals = module.host_space_principal_lookups.principal_maps.host_admins.all
      role_assignments = [
        // Example role assignment:
        // { scope = data.azurerm_resource_group.main.id, role_definition_name = "Reader" }, # Reader on Host Space Resource Group
      ]
    },
    {
      name             = "host_managers"
      existing         = true
      principals       = module.host_space_principal_lookups.principal_maps.host_managers.all
      role_assignments = []
    },
    {
      name             = "host_readers"
      existing         = true
      principals       = module.host_space_principal_lookups.principal_maps.host_readers.all
      role_assignments = []
    },
    {
      name             = "host_data_writers"
      existing         = true
      principals       = module.host_space_principal_lookups.principal_maps.host_data_writers.all
      role_assignments = []
    },
    {
      name             = "host_data_readers"
      existing         = true
      principals       = module.host_space_principal_lookups.principal_maps.host_data_readers.all
      role_assignments = []
    }
  ]
}
