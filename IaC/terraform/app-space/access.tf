# Get the Principal IDs for users to add to the App Space groups
module "app_space_principal_lookups" {
  source = "./../../../submodules/neo-iac-terraform/modules/azure/neo_az_principals_lookup"
  # source = "./../../../../neo-iac-terraform/modules/azure/neo_az_principals_lookup" # Use this for local module development

  principal_sets = {
    provisioner_service_principals = { service_principals = local.service_principal_names.provisioners }
    deployer_service_principals    = { service_principals = local.service_principal_names.deployers }
    operations_service_principals  = { service_principals = local.service_principal_names.operations }

    admins = {
      users  = var.access.admins.enabled ? var.access.admins.principals : []
      groups = var.access.admins.enabled ? var.access.admins.groups : []
    },
    managers = {
      users  = var.access.managers.enabled ? var.access.managers.principals : []
      groups = var.access.managers.enabled ? var.access.managers.groups : []
    },
    readers = {
      users  = var.access.readers.enabled ? var.access.readers.principals : []
      groups = var.access.readers.enabled ? var.access.readers.groups : []
    },
    data_writers = {
      users  = var.access.data_writers.enabled ? var.access.data_writers.principals : []
      groups = var.access.data_writers.enabled ? var.access.data_writers.groups : []
    },
    data_readers = {
      users  = var.access.data_readers.enabled ? var.access.data_readers.principals : []
      groups = var.access.data_readers.enabled ? var.access.data_readers.groups : []
    }
    apps = {
      users  = var.access.apps.enabled ? var.access.apps.principals : []
      groups = var.access.apps.enabled ? var.access.apps.groups : []
    }
  }
}

# Add additional roles to Environment groups
module "aad_groups" {
  source = "./../../../submodules/neo-iac-terraform/modules/azure/neo_az_aad_groups"
  # source = "./../../../../neo-iac-terraform/modules/azure/neo_az_aad_groups" # Use this for local module development

  resource_prefix = local.resource_prefix

  aad_groups = concat(
    var.access.admins.enabled ? [{
      name       = "admins"
      existing   = var.access.existing
      principals = module.app_space_principal_lookups.principal_maps.admins.all
      role_assignments = concat(
        # Owner on app space's Resource Group if it is a standalone app space
        local.is_standalone ? [{ resource_name = local.resource_group.name, scope = local.resource_group.id, role_definition_name = "Owner" }] : [],
        # Assign Data Owner on all storage accounts
        [for storage_account in var.storage_accounts : { resource_name = azurerm_storage_account.storage_accounts[storage_account.name].name, scope = azurerm_storage_account.storage_accounts[storage_account.name].id, role_definition_name = "Storage Blob Data Owner" }],
      )
    }] : [],
    var.access.managers.enabled ? [{
      name       = "managers"
      existing   = var.access.existing
      principals = module.app_space_principal_lookups.principal_maps.managers.all
      # Reader on app space's Resource Group if it is a standalone app space
      role_assignments = local.is_standalone ? [{ resource_name = local.resource_group.name, scope = local.resource_group.id, role_definition_name = "Reader" }] : [],
    }] : [],
    var.access.readers.enabled ? [{
      name       = "readers"
      existing   = var.access.existing
      principals = module.app_space_principal_lookups.principal_maps.readers.all
      # Reader on app space's Resource Group if it is a standalone app space
      role_assignments = local.is_standalone ? [{ resource_name = local.resource_group.name, scope = local.resource_group.id, role_definition_name = "Reader" }] : [],
    }] : [],
    var.access.data_writers.enabled ? [{
      name       = "data_writers"
      existing   = var.access.existing
      principals = module.app_space_principal_lookups.principal_maps.data_writers.all
      role_assignments = concat(
        # Assign Data Contributor on all storage accounts
        [for storage_account in var.storage_accounts : { resource_name = azurerm_storage_account.storage_accounts[storage_account.name].name, scope = azurerm_storage_account.storage_accounts[storage_account.name].id, role_definition_name = "Reader and Data Access" }],
        [for storage_account in var.storage_accounts : { resource_name = azurerm_storage_account.storage_accounts[storage_account.name].name, scope = azurerm_storage_account.storage_accounts[storage_account.name].id, role_definition_name = "Storage Blob Data Contributor" }]
      )
    }] : [],
    var.access.data_readers.enabled ? [{
      name       = "data_readers"
      existing   = var.access.existing
      principals = module.app_space_principal_lookups.principal_maps.data_readers.all
      role_assignments = concat(
        # Assign Reader and Data Access on all storage accounts
        [for storage_account in var.storage_accounts : { resource_name = azurerm_storage_account.storage_accounts[storage_account.name].name, scope = azurerm_storage_account.storage_accounts[storage_account.name].id, role_definition_name = "Reader and Data Access" }]
      )
    }] : [],
    var.access.apps.enabled ? [{
      name       = "apps"
      existing   = var.access.existing
      principals = module.app_space_principal_lookups.principal_maps.apps.all
      # Reader on app space's Resource Group if it is a standalone app space
      role_assignments = local.is_standalone ? [{ resource_name = local.resource_group.name, scope = local.resource_group.id, role_definition_name = "Reader" }] : [],
    }] : [],
    var.access.deployer_service_principals.enabled ? [{
      name       = "deployers"
      existing   = var.access.existing
      principals = module.app_space_principal_lookups.principal_maps.deployer_service_principals.all
      role_assignments = concat(
        # Assign Contributor on all app services
        var.app_services.enabled ? [for app in var.apps : { resource_name = azurerm_linux_web_app.app_services[app.name].name, scope = azurerm_linux_web_app.app_services[app.name].id, role_definition_name = "Contributor" }] : [],
        # Assign Reader on the Resource Group
        [{ resource_name = local.resource_group.name, scope = local.resource_group.id, role_definition_name = "Reader" }]
      )
    }] : []
  )
}
