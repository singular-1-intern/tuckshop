
module "key_vault" {
  source = "./../../../submodules/neo-iac-terraform/modules/azure/neo_az_key_vault"
  # source = "./../../../../neo-iac-terraform/modules/azure/neo_az_key_vault" # Use this for local module development

  count = var.key_vault.enabled ? 1 : 0

  prevent_destroy = var.prevent_destroy.key_vault

  azure = {
    tenant_id       = var.azure.tenant_id
    subscription_id = var.azure.subscription_id
    location        = var.azure.location
    resource_group  = local.resource_group.name
    resource_prefix = local.resource_prefix
    tags            = local.tags
  }

  name_suffix              = "apps"
  purge_protection_enabled = var.azure.purge_protection_enabled

  principals = {
    # Also add the admin principals directly
    # (When using a group, KV often doesn't recognise the assigned rights on the access policies for some time, causing failures in the provisioning process)
    add_caller_as_admin = false
    admins = merge(
      module.app_space_principal_lookups.principal_maps.provisioner_service_principals.all,
      var.access.admins.enabled ? module.aad_groups.groups.admins.principal_map : {}
    )

    managers = merge(
      module.app_space_principal_lookups.principal_maps.operations_service_principals.all,
      var.access.managers.enabled ? module.aad_groups.groups.managers.principal_map : {}
    )

    apps = merge(
      {
        for identity in azurerm_user_assigned_identity.app_identities : join("_", ["identities", lower(identity.name)]) => {
          name      = identity.name
          object_id = identity.principal_id
        }
      },
      var.access.apps.enabled ? module.aad_groups.groups.apps.principal_map : {}
    )

    key_users = merge(
      var.access.data_readers.enabled ? module.aad_groups.groups.data_readers.principal_map : {},
      var.access.data_writers.enabled ? module.aad_groups.groups.data_writers.principal_map : {},
      # If a DB Script Runner service principal exists, it needs access to the DB Encryption Keys in the Key Vault for Always Encrypted to work.
      local.db_script_runner_service_principal != null ? {
        db_script_runner = {
          name      = local.db_script_runner_service_principal.service_principal.name
          object_id = local.db_script_runner_service_principal.service_principal.object_id
        }
      } : {}
    )
  }

  certificates = {
    token_encryption_credential = {
      name               = "KeyVault--AuthenticationServer--TokenEncryptionCredential"
      subject            = "CN=internal.singular.co.za"
      dns_names          = ["internal.singular.co.za"]
      validity_in_months = 12
    }
  }

  generate_identity_server_cert = true

  rsa_keys = [
    {
      name = "identity-server-data-protection-keys-master-key"
      size = 4096
    }
  ]

  secret_names = compact([
    var.service_bus.enabled ? join("--", ["KeyVault", "ServiceBus", "ConnectionString"]) : ""
  ])

  secret_values = compact([
    var.service_bus.enabled ? azurerm_servicebus_namespace.service_bus[0].default_primary_connection_string : ""
  ])

  generated_secrets = flatten([
    local.api_client_secret_names,
    local.additional_api_client_secret_names,
    local.generated_secret_names,
  ])

  user_secrets = local.user_secret_names

  secret_properties = local.secret_properties
}
