resource "azurerm_storage_account" "storage_accounts" {
  for_each = { for storage_account in var.storage_accounts : storage_account.name => storage_account }

  name                            = lower(replace(join("", compact([var.prefixes.project, each.value.use_location_prefix ? var.azure.location_prefix : "", local.environment_prefix, "st", each.value.name])), "-", ""))
  location                        = local.resource_group.location
  resource_group_name             = local.resource_group.name
  account_kind                    = each.value.kind
  access_tier                     = each.value.access_tier
  account_tier                    = each.value.sku
  account_replication_type        = each.value.replication_type
  allow_nested_items_to_be_public = each.value.allow_public_access
  https_traffic_only_enabled      = true
  min_tls_version                 = "TLS1_2"
  shared_access_key_enabled       = var.azure.storage_account_shared_access_key_enabled

  dynamic "blob_properties" {
    for_each = each.value.data_retention_settings.enabled ? [each.value.data_retention_settings] : []
    content {
      # Both BV and DP features require these to be enabled.
      change_feed_enabled = true
      versioning_enabled  = true

      change_feed_retention_in_days = blob_properties.value.change_feed_retention_in_days

      # Policy that manages soft delete for Containers.
      container_delete_retention_policy {
        days = blob_properties.value.container_retention_days
      }
    }
  }

  # Set Lifecycle Ignore rules on Blob Properties that are managed by an external Backup Vault.
  # This rule prevents the above `!var.backup_vault.enabled` conditions to not be respected and are thus ignored.
  # We need terraform v1.6+ to be able to make use of dynamic lifecycle blocks.
  lifecycle {
    ignore_changes = [
      blob_properties[0].delete_retention_policy,
      blob_properties[0].restore_policy,
    ]
  }
}

# Applications with Data Reader on storage accounts
resource "azurerm_role_assignment" "app_storage_readers" {
  for_each = { for reader in local.storage_readers : "${reader.app_name}-${reader.storage_name}" => reader }

  scope                = azurerm_storage_account.storage_accounts[each.value.storage_name].id
  role_definition_name = "Storage Blob Data Reader"
  principal_id         = azurerm_user_assigned_identity.app_identities[each.value.app_name].principal_id
}

# Applications with Data Contributor on storage accounts
resource "azurerm_role_assignment" "app_storage_writers" {
  for_each = { for writer in local.storage_writers : "${writer.app_name}-${writer.storage_name}" => writer }

  scope                = azurerm_storage_account.storage_accounts[each.value.storage_name].id
  role_definition_name = "Storage Blob Data Contributor"
  principal_id         = azurerm_user_assigned_identity.app_identities[each.value.app_name].principal_id
}
