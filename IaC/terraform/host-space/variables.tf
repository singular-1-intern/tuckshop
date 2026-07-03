variable "project_label" {
  description = "Friendly name of the client project"
  type        = string
}

variable "prefixes" {
  description = "Used in resource naming and tagging"
  type = object({
    core           = optional(string, "sc") # Prefix for the shared project resources
    shared_hosts   = string                 # Prefix for the shared host resources
    client_project = string                 # The client project prefix
    host_space     = string                 # Prefix of the host space to provision resources in
  })
}

variable "azure" {
  description = "The Azure tenant & subscription, location and login details"
  type = object({
    tenant_id                = string               # The Azure Tenant (AAD) ID
    subscription_id          = string               # The subscription to provision resources against
    location                 = string               # The location to create project resources under
    location_prefix          = optional(string, "") # The location prefix (Optional). Will be included in resource names if provided.
    purge_protection_enabled = bool                 # Enable purge protection on resources which support it?
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
    host_admins       = optional(list(string), [])
    host_managers     = optional(list(string), [])
    host_readers      = optional(list(string), [])
    host_data_writers = optional(list(string), [])
    host_data_readers = optional(list(string), [])
  })
  default = {}
}

variable "ips_whitelist" {
  description = "IPs to add to allow lists for the various resources (Subnets, Azure Kubernetes Service, Virtual Machines, etc)"
  type        = list(string)
  default     = []
}

variable "network" {
  description = "The network address spaces"
  type = object({
    enable_private_ips_deny_rule           = optional(bool, false)
    default_outbound_access_enabled        = optional(bool, true)  # Enable default outbound access on the servers subnet?
    deny_outbound_internet_traffic         = optional(bool, false) # Deny all outbound internet access from the servers subnet? (Only applied if default_outbound_access_enabled is false)
    link_subnets_to_nat_gateway_if_present = optional(bool, true)  # If a NAT Gateway is present in the shared hosts, link the local servers subnet to it.

    additional_subnet_security_rules = optional(list(object({
      subnet_name                  = string
      name                         = string
      priority                     = number # 100 to 4096
      direction                    = string # Inbound, Outbound
      access                       = string # Allow, Deny
      protocol                     = string # Tcp, Udp, Icmp
      source_port_ranges           = list(string)
      source_address_prefixes      = list(string) # If you specify multiple address prefixes, you must enable the 'Microsoft.Network/AllowMultipleAddressPrefixesOnSubnet' feature on your subscription.
      destination_port_ranges      = list(string)
      destination_address_prefixes = list(string) # If you specify multiple address prefixes, you must enable the 'Microsoft.Network/AllowMultipleAddressPrefixesOnSubnet' feature on your subscription.
    })), [])
  })

  default = {}
}

variable "aks_cluster" {
  description = "Details of an existing AKS Cluster to use for the host space (Generally only for use where your shared hosts does not have a cluster, and you want to deploy to an alternative one)"
  type = object({
    name           = string
    resource_group = string
    namespace      = string # Default namespace to deploy resources into
  })
  default = null
}

variable "sql_servers" {
  description = "SQL Server VM configurations. Note that each item's key is used as the name suffix appended to generated resource names."
  type = map(object({
    vm_size                 = string
    enable_alerts           = optional(bool, true)
    service_monitor_enabled = optional(bool, true)  # Enable Prometheus Service Monitor for this VM?
    enable_secure_enclaves  = optional(bool, false) # Enable SQL Server Secure Enclaves features on the server?
    short_admin_username    = optional(bool, false) # Enable this option if you hit a length limit error on the admin username. Will use "adm" instead of "admin".
    short_computer_name     = optional(bool, false) # Enable this option if you hit a length limit error on, or wish to exclude the location from, the computer name.
    sql_iaas_enabled        = optional(bool, true)  # Install SQL IaaS Agent extension on this VM?

    os_disk_type   = optional(string, "Standard_LRS")
    os_disk_size   = optional(number, 128)
    data_disk_type = optional(string, "StandardSSD_LRS")
    data_disk_size = optional(number, 64)
    logs_disk_type = optional(string, "StandardSSD_LRS")
    logs_disk_size = optional(number, 64)

    public_ip = optional(object({
      enabled           = optional(bool, false)
      link_nic          = optional(bool, true) # Terraform can struggle to remove a public IP when it's associated with a NIC. This can be used to first disassociate the NIC before deleting the public IP.
      allocation_method = optional(string, "Static")
      sku               = optional(string, "Standard")
    }), {})

    public_access = optional(object({
      sql                  = optional(bool, false)      # Enable inbound access to SQL port 1433
      rdp                  = optional(bool, false)      # Enable inbound access to RDP port 3389
      outbound             = optional(bool, false)      # Allows outbound traffic from the public IP (For use when default outbound access is disabled on the VM's subnet)
      allowed_inbound_ips  = optional(list(string), []) # List of allowed inbound IPs when inbound access is enabled (E.g. For SQL or RDP). This is merged with the 'ips_whitelist' variable, so it's only required in cases where more granular IP whitelisting is required.
      allowed_outbound_ips = optional(list(string), []) # List of allowed outbound IPs when outbound access is enabled (If left empty, outbound traffic is allowed to all destinations)
    }), {})

    image = optional(object({
      offer     = optional(string, "sql2022-ws2022")
      publisher = optional(string, "MicrosoftSQLServer")
      sku       = optional(string, "web-gen2")
      version   = optional(string, "latest")
    }), {})

    virtual_machine = optional(object({
      install_aad_integration = optional(bool, true) # Install Azure AD integration on the VM?
      install_azure_policy    = optional(bool, true) # Install Azure Policy on the VM?
      vm_patch_mode           = optional(string, "Manual") # Patch mode to use if Azure Policy is installed. Possible values are AutomaticByPlatform, AutomaticByOS, Manual, and Off.
    }), {})

    shutdown_schedule = optional(object({
      enabled               = optional(bool, false)
      daily_recurrence_time = optional(string, "1800")
      timezone              = optional(string, "South Africa Standard Time")
    }), {})

    recovery_services_vault = optional(object({
      policies = optional(object({
        virtual_machine = optional(string, "")
        sql_server      = optional(string, "")
      }))
    }), null)
  }))
  default = {}

  validation {
    condition     = alltrue([for sql_server in var.sql_servers : !sql_server.public_ip.enabled || (sql_server.public_ip.enabled && (sql_server.public_access.sql || sql_server.public_access.rdp || sql_server.public_access.outbound))])
    error_message = "When a public IP is enabled on an SQL Server, at least one access type (SQL, RDP or Outbound) must be enabled."
  }

  # Cannot enforce this anymore because terraform won't allow a condition to reference another variable, and in this case, we want to check if ips_whitelist has values
  # (Hoping this will be resolved in future versions of Terraform)
  # validation {
  #   condition     = alltrue([for sql_server in var.sql_servers : !sql_server.public_ip.enabled || (sql_server.public_ip.enabled && length(sql_server.public_access.allowed_inbound_ips) > 0)])
  #   error_message = "When a public IP is enabled on an SQL Server, a list of the allowed IPs must be provided."
  # }
}

variable "ops_vms" {
  description = "Operations VM configurations. Note that each item's key is used as the name suffix appended to generated resource names."
  type = map(object({
    vm_size                 = optional(string, "Standard_B1s")
    public_ssh_key          = string # Path to a public SSH key. (You can use "{path.module}" to specify a path relative to the module)
    private_ip              = optional(string, "")
    install_aad_integration = optional(bool, true)
    service_monitor_enabled = optional(bool, true) # Enable Prometheus Service Monitor for this VM?
    enable_alerts           = optional(bool, true)
    ansible_enabled         = optional(bool, true)  # Are ansible runs enabled for this VM?
    ansible_use_hostname    = optional(bool, false) # If false, the Private IP is used, if true, the VM's hostname is used

    public_ip = optional(object({
      enabled           = optional(bool, false)
      link_nic          = optional(bool, true) # Terraform can struggle to remove a public IP when it's associated with a NIC. This can be used to first disassociate the NIC before deleting the public IP.
      allocation_method = optional(string, "Static")
      sku               = optional(string, "Standard")
    }), {})

    public_access = optional(object({
      ssh                  = optional(bool, false)      # Allow SSH over the public IP?
      https                = optional(bool, false)      # Allow HTTPS over the public IP?
      outbound             = optional(bool, false)      # Allows outbound traffic (For use when default outbound access is disabled on the VM's subnet, and a NAT Gateway or Public IP is being used instead)
      allowed_inbound_ips  = optional(list(string), []) # List of allowed inbound IPs when inbound access is enabled (E.g. For SSH or HTTPS). This is merged with the 'ips_whitelist' variable, so it's only required in cases where more granular IP whitelisting is required.
      allowed_outbound_ips = optional(list(string), []) # List of allowed outbound IPs when outbound access is enabled (If left empty, outbound traffic is allowed to all destinations)
    }), {})

    private_access = optional(object({
      ssh      = optional(bool, true)
      https    = optional(bool, false)
      outbound = optional(bool, false) # Allows outbound traffic (For use when default outbound access is disabled on the VM's subnet, and a NAT Gateway or Public IP is being used instead)
    }), {})

    features = optional(object({
      node_exporter = optional(bool, true) # Configure Prometheus Node Exporter

      warp_cli = optional(object({
        team_name = string
      }), null)

      nginx_proxy = optional(object({
        client_certificates = optional(list(string), []) # A list of certificate names to use for client certificates added to the Key Vault & installed onto the Nginx Proxy
        locations = optional(map(object({
          protocol  = optional(string, "https")
          path      = string
          proxy_url = string
          template  = optional(string, "nginx-location.conf")

          # Enables or disables the passing of the server name through the TLS Server Name Indication (SNI) extension when Nginx establishes a connection with a proxied SSL/TLS server.
          proxy_ssl_server_name = optional(bool, false)

          proxy_headers = optional(list(object({
            name  = string
            value = string
          })), [])

          probe = optional(object({
            path                = string
            excluded_app_spaces = optional(list(string), [])
          }), null)
        })), {})
      }), null)
    }), {})

    image = optional(object({
      publisher = optional(string, "Debian")
      offer     = optional(string, "debian-12")
      sku       = optional(string, "12-gen2")
      version   = optional(string, "latest")
    }), {})

    storage = optional(object({
      type = optional(string, "Standard_LRS")
      size = optional(number, 32)
    }), {})
  }))
  default = {}
}

variable "backups" {
  description = "Backup configuration."
  type = object({
    recovery_services_vault = optional(object({
      enabled             = optional(bool, true)
      soft_delete_enabled = optional(bool, true)
      alerts = optional(object({
        enabled = optional(bool, false)
        emails  = optional(list(string), [])

        # These specify conditions that the alert must match for a notification to be sent.
        # This can for example, be used to suppress low severity alerts.
        alert_context_filters = optional(list(object({
          operator = string       # Possible values are "Equals", "NotEquals", "Contains", and "DoesNotContain".
          values   = list(string) # String text to filter on.
        })), [])
      }), {})
    }), {})
  })
  default = {}

  validation {
    condition     = alltrue([for filter in var.backups.recovery_services_vault.alerts.alert_context_filters : contains(["Equals", "NotEquals", "Contains", "DoesNotContain"], filter.operator)])
    error_message = "The operator must be one of Equals, NotEquals, Contains, or DoesNotContain"
  }

  validation {
    condition     = var.backups.recovery_services_vault.alerts.enabled ? length(var.backups.recovery_services_vault.alerts.emails) > 0 : true
    error_message = "At least one email must be specified if alerts are enabled"
  }
}

variable "backup_policies" {
  description = "Backup Policy configuration."

  type = object({
    # SQL Server Backup policies to apply to the Recovery Service Vault
    # (This has been kept separate from the "backups" variable to allow enabling/disabling of RSV without having to re-define all the backup policies)
    sql_server_policies = optional(map(object({
      enable_compression = optional(bool, false) # Enable compression of backups? This may only be available on specific versions of SQL Server.

      full_backup = optional(object({
        weekly_retention_weeks   = optional(number, 1) # How many weeks should weekly backups be retained for?
        monthly_retention_months = optional(number, 0) # How many months should monthly backups be retained for?
        yearly_retention_years   = optional(number, 0) # How many years should yearly backups be retained for?
      }), {})

      differential_backup = optional(object({
        retention_days = optional(number, 7) # How many days should differential backups be retained for?
      }), {})

      log_backup = optional(object({
        frequency_in_minutes = optional(number, 120) # How often should log backups run?
        retention_days       = optional(number, 7)   # How many days should log backups be retained for?
      }), {})
    })), {})
  })
  default = {}
}

variable "config_export" {
  type = object({
    enabled                           = optional(bool, true)
    automation_account_export_enabled = optional(bool, true)  # Enable exporting the environment configuration to an Azure Automation Account
    file_export_enabled               = optional(bool, false) # Enable exporting the environment configuration to a file
    file_folder                       = optional(string, ".terraform/config/host-spaces")
    file_prefix                       = optional(string, "host-space")
    file_format                       = optional(string, "json")
  })
  default = {}

  validation {
    condition     = contains(["json", "yaml"], var.config_export.file_format)
    error_message = "Environment configuration export file format must be either 'json' or 'yaml'."
  }
}

variable "container_registry" {
  type = object({
    enabled        = optional(bool, false)
    sku            = optional(string, "Basic")
    secret_version = optional(number, 0)
  })
  default = {}
}

variable "alerts" {
  # NOTE: Configuring alerts is only necessary if the host space environment prefix is not the same as one of the
  #       app spaces. Otherwise, any host space alerts will come through via the app-space's alerts configuration.
  #       (This module has logic to only deploy alerts configuration if the host space environment prefix is
  #        different to the app space environment prefix)
  description = "Configuration for delivering alerts to the team members responsible for this host space."
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

variable "ansible" {
  description = "Ansible configuration"
  type = object({
    enabled              = optional(bool, false)
    private_ssh_key      = optional(string, "") # Path to private SSH key used to connect to Ansible managed VMs
    dry_run_enabled      = optional(bool, false)
    file_cleanup_enabled = optional(bool, true)
  })
  default = {}
}

variable "ansible_ssh_passphrase" {
  description = "Ansible SSH passphrase (Should be passed in via environment variable: TF_VAR_ansible_ssh_passphrase)"
  type        = string
  default     = ""
}

variable "prevent_destroy" {
  description = "Enable resource locks on various resources in the host-space module"
  type = object({
    resource_locks          = optional(bool, false) # This is the base resource_locks module
    key_vault               = optional(bool, true)
    sql_machines            = optional(bool, true)
    ops_machines            = optional(bool, true)
    recovery_services_vault = optional(bool, false)
  })
  default = {}
}
