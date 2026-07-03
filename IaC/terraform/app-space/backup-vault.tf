resource "azurerm_data_protection_backup_vault" "main" {
  count = var.backup_vault.enabled ? 1 : 0

  name                = join("-", [local.resource_prefix, "bv"])
  resource_group_name = local.resource_group.name
  location            = local.resource_group.location
  datastore_type      = "VaultStore"
  redundancy          = var.backup_vault.georedundant ? "GeoRedundant" : "LocallyRedundant"
  soft_delete         = var.backup_vault.soft_delete

  identity {
    type = "SystemAssigned"
  }
}

# Grant the Backup Vault permissions to manage storage accounts in the Resource Group
resource "azurerm_role_assignment" "backup_vault_storage_account_backup_contributor" {
  count = var.backup_vault.enabled ? 1 : 0

  scope                = local.resource_group.id
  role_definition_name = "Storage Account Backup Contributor"
  principal_id         = azurerm_data_protection_backup_vault.main[0].identity[0].principal_id
}

# Create a backup policy to apply to the storage accounts
resource "azurerm_data_protection_backup_policy_blob_storage" "storage_policy" {
  count = var.backup_vault.enabled ? 1 : 0

  name                                   = join("-", [local.resource_prefix, "bvp", "storage"])
  vault_id                               = azurerm_data_protection_backup_vault.main[0].id
  operational_default_retention_duration = var.backup_vault.operational_default_retention_duration
}

# Link the tfstate storage to the backup vault and the storage policy
resource "azurerm_data_protection_backup_instance_blob_storage" "tfstate" {
  count = var.backup_vault.enabled ? 1 : 0

  name               = join("-", [local.resource_prefix, "bvi", "storage_tfstate"])
  vault_id           = azurerm_data_protection_backup_vault.main[0].id
  location           = var.azure.location
  storage_account_id = var.tfstate.use_existing ? data.azurerm_storage_account.tfstate[0].id : module.tfstate_storage_account.storage.id
  backup_policy_id   = azurerm_data_protection_backup_policy_blob_storage.storage_policy[0].id

  depends_on = [azurerm_role_assignment.backup_vault_storage_account_backup_contributor]
}

# When running a destroy operation, the role assignments cannot be removed while the data protection on the storage accounts is still being destroyed.
# So we need to add a delay to allow the destroy operation to complete without errors.
resource "time_sleep" "wait_for_backup_vault_on_destroy" {
  count = var.backup_vault.enabled ? 1 : 0

  depends_on = [azurerm_role_assignment.app_storage_readers, azurerm_role_assignment.app_storage_writers, module.aad_groups]

  destroy_duration = "30s"
}

# Link the storage accounts to the backup vault
# NOTE: This adds a lock to the storage account resource, which can sometimes cause problems (E.g. Associated role assignments cannot be removed).
#       If a change needs to be made which affects the storage accounts, this protection resource might need to be temporarily removed.
resource "azurerm_data_protection_backup_instance_blob_storage" "storage_accounts" {
  for_each = { for storage_account in(var.backup_vault.enabled ? var.storage_accounts : []) : storage_account.name => storage_account }

  name               = join("-", [local.resource_prefix, "bvi", "storage", each.key])
  vault_id           = azurerm_data_protection_backup_vault.main[0].id
  location           = local.resource_group.location
  storage_account_id = azurerm_storage_account.storage_accounts[each.key].id
  backup_policy_id   = azurerm_data_protection_backup_policy_blob_storage.storage_policy[0].id

  depends_on = [azurerm_role_assignment.backup_vault_storage_account_backup_contributor, time_sleep.wait_for_backup_vault_on_destroy]
}
