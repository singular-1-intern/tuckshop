# ┌─────────────────────────────────────────────────────────────────────┐
# │ UAT app-space tfvars file                            │
# └─────────────────────────────────────────────────────────────────────┘

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

access = {
  # ZTN access to AKS, Owner RBAC
  admins = {
    principals = []
  }
  # Cluster and RG access
  managers = {
    principals = []
  }
  readers = {
    principals = []
  }
  # ZTN access to VM instances
  data_readers = {
    principals = []
  }
  data_writers = {
    principals = []
  }
}

storage_accounts = [
  {
    name             = "documents"
    clear_operations = { allowed = false }
    copy_operations = {
      outbound_enabled        = true
      allow_inbound_copy      = true
      allow_inbound_overwrite = true
    }
  }
]

dns_zone = {
  enabled             = true
  domain              = "uat-singular-cloud.com"
  sub_domains         = ["ts"]
  primary_sub_domain  = "ts"
  proxying_enabled    = true # Should proxying be create on automatically generated DNS Records (E.g. AKS or App Service A records)
}

apps = [
  {
    name                       = "web-app"
    requires_identity          = false
    requires_api_client_secret = false
  },
  {
    name = "identity-server"
  },
  {
    name                 = "domain"
    storage_write_access = ["documents"]
  }
]

sql_servers = {
  sql01 = {
    type            = "shared"
    database_prefix = "TS.UAT."
    sql_host_type   = "private-ip"
    restore_settings = {
      excluded_databases = ["IdentityServer", "AuthorisationServer", "Notifications"]
      overwrite_existing = true
    }
  }
}
