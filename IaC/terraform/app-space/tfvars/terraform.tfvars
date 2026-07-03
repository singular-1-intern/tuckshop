# ┌─────────────────────────────────────────────────────────────────────┐
# │ Base app-space tfvars file                                          │
# │ (Used for default configuration common across all environments)     │
# └─────────────────────────────────────────────────────────────────────┘

project_label = "Tuckshop"

prefixes = {
  project    = "ts"
  host_space = "stg"
}

azure = {
  tenant_id                = "c9871c9d-7fe6-4b1d-a97a-22b172b872e7"
  subscription_id          = "2e538ecd-e2f2-40d9-bc8a-272df8f5e2f7"
  location                 = "westeurope"
  location_prefix          = "we"
  purge_protection_enabled = false
}

service_bus = {
  enabled = true
}

key_vault = {
  enabled = true
}

backup_vault = {
  enabled = true
}

