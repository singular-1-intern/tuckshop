variable "project_label" {
  description = "Friendly name of the client project"
  type        = string
}

variable "prefixes" {
  description = "Project and host environment prefixes"
  type = object({
    project    = string           # The client project that the environment belongs to
    host_space = optional(string) # The host space that the environment runs on. Optional if provisioning a standalone app space
  })
}

variable "azure" {
  description = "The Azure tenant & subscription, location and login details"
  type = object({
    tenant_id                                 = string               # The Azure Tenant (AAD) ID
    subscription_id                           = string               # The subscription to provision resources against
    location                                  = string               # The location to create project resources under
    location_prefix                           = optional(string, "") # The location prefix (Optional). Will be included in resource names if provided.
    resource_group                            = optional(string, "") # The name of the resource group (Optional, defaults to a generated name)
    purge_protection_enabled                  = bool                 # Enable purge protection on resources which support it?
    storage_account_shared_access_key_enabled = optional(bool, true) # Indicates whether the storage account permits requests to be authorized with the account access key via Shared Key. If false, then all requests, including shared access signatures, must be authorized with Azure Active Directory (Azure AD). Defaults to true.
  })
}

variable "service_principal" {
  description = "Service Principal for connecting to Azure. Only needs to be provided when this module is run via automation. (Use the 'TF_VAR_service_principal' environment variable to avoid needing to save the credentials into a tfvars file)"
  type = object({
    client_id     = string
    client_secret = string
  })
  default   = null
  sensitive = true
}

variable "access" {
  description = "Users to add to the app space access groups"
  type = object({
    existing = optional(bool, true)  # Use existing groups or create new ones?
    group_owners = optional(object({ # The group owners can modify group membership. (By default only the principal running the terraform is added)
      enabled    = optional(bool, true)
      principals = optional(list(string), [])
    }), {})
    admin_service_principals = optional(object({
      enabled    = optional(bool, true)
      principals = optional(list(string), [])
    }), {})
    admins = optional(object({
      enabled    = optional(bool, true)
      principals = optional(list(string), [])
      groups     = optional(list(string), [])
    }), {})
    managers = optional(object({
      enabled    = optional(bool, true)
      principals = optional(list(string), [])
      groups     = optional(list(string), [])
    }), {})
    readers = optional(object({
      enabled    = optional(bool, true)
      principals = optional(list(string), [])
      groups     = optional(list(string), [])
    }), {})
    data_readers = optional(object({
      enabled    = optional(bool, true)
      principals = optional(list(string), [])
      groups     = optional(list(string), [])
    }), {})
    data_writers = optional(object({
      enabled    = optional(bool, true)
      principals = optional(list(string), [])
      groups     = optional(list(string), [])
    }), {})
    apps = optional(object({
      enabled    = optional(bool, false)
      principals = optional(list(string), [])
      groups     = optional(list(string), [])
    }), {})
    provisioner_service_principals = optional(object({
      enabled    = optional(bool, false)
      principals = optional(list(string), [])
    }), {})
    deployer_service_principals = optional(object({
      enabled    = optional(bool, false)
      principals = optional(list(string), [])
    }), {})
    operations_service_principals = optional(object({
      enabled    = optional(bool, false)
      principals = optional(list(string), [])
    }), {})
  })
  default = {}
}

variable "service_bus" {
  type = object({
    enabled = optional(bool, false)
    name    = optional(string, "main")
    sku     = optional(string, "Standard")
  })
  default = {}
}

variable "key_vault" {
  type = object({
    enabled = optional(bool, false)
  })
  default = {}
}

variable "storage_accounts" {
  description = "Client Blob Storage Account configurations"

  type = list(object({
    name                = string
    kind                = optional(string, "StorageV2")
    access_tier         = optional(string, "Hot")
    sku                 = optional(string, "Standard")
    replication_type    = optional(string, "RAGRS")
    allow_public_access = optional(bool, false)
    use_location_prefix = optional(bool, false)

    # Use the below settings to control and modify the Blob Storage Data Protection settings.
    data_retention_settings = optional(object({
      enabled = optional(bool, true) # (Optional) Enables data retention settings.
      # General settings for data retention
      change_feed_retention_in_days = optional(number, (365 * 3)) # (Optional) The duration of change feed events retention in days. The possible values are between 1 and 146000 days (400 years).
      # Container Retention policy settings.
      container_retention_days = optional(number, 90) # (Optional) Specifies the number of days that the container should be retained, between 1 and 365 days
    }), {})

    # The below configuration can be overridden on an operational pipeline level.
    clear_operations = optional(object({
      allowed = optional(bool, true)
    }), {})

    # The below configuration can be overridden on an operational pipeline level.
    copy_operations = optional(object({
      outbound_enabled        = optional(bool, true) # Controls if copies from this storage account are allowed to a target storage account.
      allow_inbound_copy      = optional(bool, true) # Controls if copies from a source storage account are allowed to this storage account.
      allow_inbound_overwrite = optional(bool, true) # Controls if copies from a source storage account can overwrite existing data files in this storage account.
    }), {})

  }))
  default = []
}

variable "backup_vault" {
  description = "Backup vault configuration"
  type = object({
    enabled                                = optional(bool, true)
    georedundant                           = optional(bool, true)
    operational_default_retention_duration = optional(string, "P30D")
    soft_delete                            = optional(string, "On")
  })
  default = {}
}

variable "dns_zone" {
  type = object({
    enabled             = optional(bool, false)
    domain              = optional(string, "")
    primary_sub_domain  = optional(string, "")       # Optional: Use this to override the sub-domain used by all apps. Note that this will not cause a DNS entry to be created (You would still need to add an entry to the sub_domains list).
    sub_domains         = optional(list(string), []) # NOTE: If primary_sub_domain isn't specified, and the apps don't have sub_domain set, then the first entry in this list is used as the sub-domain under which the apps will run.
    proxying_enabled    = optional(bool, false)
    max_upload          = optional(number, 100) # Free plan only allows up to 100mb
    minimum_tls_version = optional(string, "1.2")
    ssl                 = optional(string, "full")
    cipher_suites       = optional(string, "legacy") # This can only be changed if 'Advanced Certificate Manager' is enabled on the DNS Zone (At time of writing, this costs $10/month)

    dns_records = optional(list(object({
      type     = string
      name     = string
      value    = string
      proxied  = optional(bool, false)
      priority = optional(number, null)
    })), [])
  })
  default = {}

  validation {
    condition     = anytrue([var.dns_zone.enabled && var.dns_zone.domain != "", var.dns_zone.enabled == false])
    error_message = "A domain is required when the DNS Zone is enabled."
  }

  validation {
    condition     = contains(["off", "flexible", "full", "strict", "origin_pull"], var.dns_zone.ssl)
    error_message = "DNS Zone ssl setting must be one of: 'off', 'flexible', 'full', 'strict', 'origin_pull'. (Default: 'full')"
  }

  validation {
    condition     = contains(["modern", "compatible", "legacy"], var.dns_zone.cipher_suites)
    error_message = "DNS Zone cipher suites must be one of: 'modern', 'compatible' or 'legacy'. (Default: 'legacy')"
  }
}

variable "app_services" {
  type = object({
    enabled = optional(bool, false)
    sku     = optional(string, "F1")
  })
  default = {}
}

variable "apps" {
  type = list(object({
    name                               = string
    sub_domain                         = optional(string, "")  # Use this if each application needs to run under a different sub-domain (E.g. Hosting via App Services, or using Cloudflare Access)
    use_existing_sub_domain            = optional(bool, false) # Enable this if the sub domain you want the app to use already exists or is managed outside of this module.
    requires_identity                  = optional(bool, true)
    requires_api_client_secret         = optional(bool, true)       # If true, then the app will have an API client secret created in the Key Vault
    additional_api_client_secret_names = optional(list(string), []) # Additional API client secrets to create in KV
    storage_write_access               = optional(list(string), []) # Storage accounts to grant read/write access to
    storage_read_access                = optional(list(string), []) # Storage accounts to grant read access to
  }))
  default = []
}

variable "resource_group" {
  description = "The name of the resource group to create or existing one to use"
  type = object({
    name              = optional(string, "") # If `use_custom_naming` is true then this must be set.
    location          = optional(string, "") # If `use_custom_naming` is true then this must be set.
    use_existing      = optional(bool, true)
    use_custom_naming = optional(bool, false)
  })
  default = {}
}

variable "tfstate" {
  description = "Terraform State storage account configuration"
  type = object({
    name              = optional(string, "") # If `use_custom_naming` are set then this must be set
    use_existing      = optional(bool, true)
    use_custom_naming = optional(bool, false)

    # Use the below settings to control and modify the Blob Storage Data Protection settings.
    data_retention_settings = optional(object({
      enabled = optional(bool, true) # (Optional) Enables data retention settings.
      # General settings for data retention
      change_feed_retention_in_days = optional(number, (365 * 3)) # (Optional) The duration of change feed events retention in days. The possible values are between 1 and 146000 days (400 years).
      # Container Retention policy settings.
      container_retention_days = optional(number, 90) # (Optional) Specifies the number of days that the container should be retained, between 1 and 365 days
    }), {})
  })
  default = {}
}

variable "config_export" {
  type = object({
    enabled                           = optional(bool, true)
    automation_account_export_enabled = optional(bool, true)  # Enable exporting the environment configuration to an Azure Automation Account
    file_export_enabled               = optional(bool, false) # Enable exporting the environment configuration to a file
    file_folder                       = optional(string, ".terraform/config/app-spaces")
    file_prefix                       = optional(string, "app-space")
    file_format                       = optional(string, "json")
    use_existing_automation_account   = optional(bool, true) # Must be false for standalone app spaces
  })
  default = {}

  validation {
    condition     = contains(["json", "yaml"], var.config_export.file_format)
    error_message = "Environment configuration export file format must be either 'json' or 'yaml'."
  }
}

variable "sql_servers" {
  # This map's keys must correspond with the key of the SQL Server in shared-hosts (if shared) or host-space state (if dedicated)
  type = map(object({
    type            = string # The type of SQL Server (Valid options: shared, dedicated)
    database_prefix = string
    sql_host_type   = string
    restore_settings = optional(object({
      excluded_databases = optional(list(string), [])
      overwrite_existing = optional(bool, false)
    }), {})
  }))
  default = {}

  validation {
    condition     = alltrue([for sql_server in var.sql_servers : contains(["shared", "dedicated"], sql_server.type)])
    error_message = "SQL Server type must be 'shared' or 'dedicated'."
  }
}

variable "alerts" {
  description = "Configuration for delivering alerts to the team members responsible for this namespace"
  type = list(object({
    group_by        = optional(list(string), ["namespace"])
    group_wait      = optional(string, "30s")
    group_interval  = optional(string, "5m")
    repeat_interval = optional(string, "12h")
    matchers = optional(list(object({
      name       = string
      value      = string
      match_type = optional(string, "=")
      })),
      [
        {
          name       = "severity"
          value      = "warning|critical"
          match_type = "=~"
        },
        {
          name       = "namespace"
          value      = "{k8s_namespace}"
          match_type = "="
        }
      ]
    )

    email = object({
      send_resolved = optional(bool, true)
      receivers     = list(string)
    })
  }))
  default = []
}

variable "prevent_destroy" {
  description = "Enable resource locks on various resources in the app-space module"
  type = object({
    resource_locks   = optional(bool, false) # This is the base resource_locks module
    storage_accounts = optional(bool, true)
    key_vault        = optional(bool, true)
  })
  default = {}
}
