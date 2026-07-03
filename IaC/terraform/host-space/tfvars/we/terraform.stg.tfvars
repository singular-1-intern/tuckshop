# ┌─────────────────────────────────────────────────────────────────────┐
# │ Staging host-space tfvars file                                      │
# └─────────────────────────────────────────────────────────────────────┘

prefixes = {
  core           = "sc"
  shared_hosts   = "stg"
  client_project = "ts"
  host_space     = "stg"
}


azure = {
  tenant_id                = "c9871c9d-7fe6-4b1d-a97a-22b172b872e7"
  subscription_id          = "2e538ecd-e2f2-40d9-bc8a-272df8f5e2f7" 
  location                 = "westeurope"
  location_prefix          = "we"
  purge_protection_enabled = false
}

sql_servers = {
  sql01 = {
    enable_secure_enclaves = true
    enable_alerts          = true

    vm_size        = "Standard_B2ms"
    os_disk_type   = "Standard_LRS"
    os_disk_size   = 64
    data_disk_type = "StandardSSD_LRS"
    data_disk_size = 128
    logs_disk_type = "StandardSSD_LRS"
    logs_disk_size = 128

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
        sql_server = "StagingSqlBackupPolicy"
      }
    }
  }
}
