# ┌─────────────────────────────────────────────────────────────────────┐
# │ Base host-space tfvars file                                         │
# │ (Used for default configuration common across all environments)     │
# └─────────────────────────────────────────────────────────────────────┘

project_label = "Tuckshop"

ips_whitelist = [
  "102.33.188.98/32", "102.33.191.226/32",    # Singular Joburg Office
  "196.192.167.194/32", "196.192.167.210/32", # Singular Cape Town Office (MetroFibre)
  "196.39.133.74/32", "196.39.167.100/32"     # cbridgman@singular.co.za, gpaddey@singular.co.za
]

prefixes = {
  core           = "sc"
  shared_hosts   = "stg"
  client_project = "ts"
  host_space     = "stg"
}

azure = {
  tenant_id                = "c9871c9d-7fe6-4b1d-a97a-22b172b872e7"
  subscription_id          = "2e538ecd-e2f2-40d9-bc8a-272df8f5e2f7" # sc-sub-devtest
  location                 = "westeurope"
  location_prefix          = "we"
  purge_protection_enabled = false
}

backup_policies = {
  sql_server_policies = {
    StagingSqlBackupPolicy = {
      full_backup = {
        weekly_retention_weeks   = 2
        monthly_retention_months = 0
        yearly_retention_years   = 0
      }
      differential_backup = {
        retention_days = 7
      }

      log_backup = {
        frequency_in_minutes = 120
        retention_days       = 7
      }
    }

    ProductionSqlBackupPolicy = {
      full_backup = {
        weekly_retention_weeks   = 4
        monthly_retention_months = 9
        yearly_retention_years   = 0
      }
      differential_backup = {
        retention_days = 14
      }

      log_backup = {
        frequency_in_minutes = 30
        retention_days       = 14
      }
    }
  }
}

prevent_destroy = false

