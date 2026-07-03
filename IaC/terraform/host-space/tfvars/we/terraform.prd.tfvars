# ┌─────────────────────────────────────────────────────────────────────┐
# │ Production host-space tfvars file                                      │
# └─────────────────────────────────────────────────────────────────────┘

prefixes = {
  core           = "sc"
  shared_hosts   = "prd"
  client_project = "ts"
  host_space     = "prd"
}


azure = {
  tenant_id                = "c9871c9d-7fe6-4b1d-a97a-22b172b872e7"
  subscription_id          = "aa7782d9-0b79-4740-9750-caf85a5aba67" 
  location                 = "westeurope"
  location_prefix          = "we"
  purge_protection_enabled = false
}

sql_servers = {
  sql01 = {
    enable_secure_enclaves = true
    enable_alerts          = true

    vm_size        = "Standard_E2ads_v5"
    os_disk_type   = "Standard_LRS"
    os_disk_size   = 128
    data_disk_type = "StandardSSD_LRS"
    data_disk_size = 256
    logs_disk_type = "StandardSSD_LRS"
    logs_disk_size = 256

    image = {
      offer     = "sql2022-ws2022"
      publisher = "MicrosoftSQLServer"
      sku       = "web-gen2"
      version   = "latest"
    }

    public_access = {
      # Entries in ips_whitelist are merged with this list. Add any case-specific IPs here.
      allowed_inbound_ips = []
    }

    recovery_services_vault = {
      policies = {
        sql_server = "ProductionSqlBackupPolicy"
      }
    }
  }
}
prevent_destroy = {
  resource_locks          = true # This is the base resource_locks module
  key_vault               = true
  sql_machines            = true
  ops_machines            = true
  recovery_services_vault = true
}
