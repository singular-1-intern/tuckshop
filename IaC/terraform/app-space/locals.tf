locals {
  environment_prefix = terraform.workspace
  resource_prefix    = join("-", compact([var.prefixes.project, var.azure.location_prefix, local.environment_prefix]))

  tags = {
    project          = var.prefixes.project
    project_label    = var.project_label
    host_environment = var.prefixes.host_space
    environment      = local.environment_prefix
  }

  # Azure Resource Names
  resource_group_name     = var.resource_group.use_custom_naming ? var.resource_group.name : join("-", [local.resource_prefix, "rg", "app_space"])
  resource_group_location = var.resource_group.use_existing ? var.resource_group.location : var.azure.location
  resource_group          = var.resource_group.use_existing ? data.azurerm_resource_group.main[0] : azurerm_resource_group.main[0]

  tfstate_storage_account = join("", [replace(local.resource_prefix, "-", ""), "st", "tfstate"])
  automation_account_name = join("-", [local.resource_prefix, "aa", "main", "app-space"])
  service_bus_name        = join("-", [local.resource_prefix, "sb", var.service_bus.name])
  app_service_plan_name   = join("-", [local.resource_prefix, "asp"])

  # Parent State
  app_space_base_state = data.azurerm_automation_variable_string.app_space_base_state.value == null ? null : jsondecode(data.azurerm_automation_variable_string.app_space_base_state.value)
  is_standalone        = local.app_space_base_state == null
  shared_hosts_state   = length(data.azurerm_automation_variable_string.shared_hosts_state) > 0 ? jsondecode(data.azurerm_automation_variable_string.shared_hosts_state[0].value) : null
  aks_cluster_state    = length(data.azurerm_automation_variable_string.shared_hosts_state) > 0 ? local.shared_hosts_state.aks_cluster : null
  aks_cluster          = local.k8s_provisioned ? data.azurerm_kubernetes_cluster.main[0] : null
  k8s_namespace        = local.is_standalone ? null : local.app_space_base_state.k8s_namespace
  k8s_provisioned      = try(local.aks_cluster_state != null, false)
  k8s_resource_prefix  = join("-", [var.prefixes.project, local.environment_prefix])

  # A cluster certificate will only be retrieved here if the cluster is provisioned, and the user has access to the cluster. (If the user does not, the kube_config will be null)
  aks_cluster_ca_certificate = try(base64decode(local.aks_cluster.kube_config.0.cluster_ca_certificate), "")

  # We either use the principals found in state or use principals defined as input or otherwise null
  service_principal_names = {
    provisioners = local.is_standalone ? try(var.access.provisioner_service_principals.principals, null) : try(local.app_space_base_state.service_principals.provisioners, null)
    deployers    = local.is_standalone ? try(var.access.deployer_service_principals.principals, null) : try(local.app_space_base_state.service_principals.deployers, null)
    operations   = local.is_standalone ? try(var.access.operations_service_principals.principals, null) : try(local.app_space_base_state.service_principals.operations, null)
  }

  db_script_runner_service_principal = try(local.app_space_base_state.local_principals.db_script_runner_service_principal, null)

  # Apps
  apps_that_require_identities = { for app in var.apps : app.name => app if app.requires_identity }
  app_names_camelcase          = { for app in var.apps : app.name => join("", [for name_part in split("-", app.name) : title(name_part)]) }

  # Key Vault Secrets
  api_client_secret_names = flatten([
    [for app in var.apps : join("--", ["KeyVault", "ApiClientSecrets", local.app_names_camelcase[app.name]]) if app.requires_api_client_secret],
    [join("--", ["KeyVault", "ApiClientSecrets", "Swagger"])]
  ])

  additional_api_client_secret_names = distinct(flatten([
    for app in var.apps : [
      for key_name in app.additional_api_client_secret_names : join("--", ["KeyVault", "ApiClientSecrets", key_name])
    ]
  ]))

  generated_secret_names = [
    join("--", ["KeyVault", "SuperUserPassword"]),
    join("--", ["KeyVault", "TestUserPassword"])
  ]

  user_secret_names = [
    join("--", ["KeyVault", "SqlAppUsername"]),
    join("--", ["KeyVault", "SqlAppPassword"]),
    join("--", ["KeyVault", "SendGridApiKey"]),
  ]

  secret_properties = merge(
    { for secret_name in local.generated_secret_names : secret_name => { allow_copy = true } },
    { join("--", ["KeyVault", "SendGridApiKey"]) = { allow_copy = true } }
  )

  storage_readers = flatten([
    for app in var.apps : [
      for storage_name in app.storage_read_access : {
        app_name     = app.name
        storage_name = storage_name
      }
    ]
  ])

  storage_writers = flatten([
    for app in var.apps : [
      for storage_name in app.storage_write_access : {
        app_name     = app.name
        storage_name = storage_name
      }
    ]
  ])

  use_storage_azuread = var.azure.storage_account_shared_access_key_enabled ? false : true

  # K8s DNS Records
  k8s_dns_records = local.k8s_provisioned ? [
    for sub_domain in var.dns_zone.sub_domains : { type = "A", name = sub_domain, value = local.aks_cluster_state.network.ingress_ip.ip_address, proxied = true, priority = null }
  ] : []

  # Merged DNS Records list
  dns_records = concat(var.dns_zone.dns_records, local.k8s_dns_records)

  # Default Application Subdomain
  default_app_sub_domain = var.dns_zone.enabled ? try(coalesce(var.dns_zone.primary_sub_domain, var.dns_zone.sub_domains[0]), "") : ""

  # Subdomains for Apps that have a subdomain explicitly set
  # (This is for K8s apps only, App Services records must be CNAMEs)
  app_dns_records = { for app in var.apps : app.sub_domain => {
    name    = app.sub_domain
    content = local.aks_cluster_state.network.ingress_ip.ip_address
  } if app.sub_domain != "" && !app.use_existing_sub_domain && local.k8s_provisioned && !var.app_services.enabled }

  # Cipher Suites (Last Updated: 2024-11-26 - Source: https://developers.cloudflare.com/ssl/edge-certificates/additional-options/cipher-suites/recommendations/)
  cipher_suites_modern     = ["ECDHE-ECDSA-AES128-GCM-SHA256", "ECDHE-ECDSA-CHACHA20-POLY1305", "ECDHE-RSA-AES128-GCM-SHA256", "ECDHE-RSA-CHACHA20-POLY1305", "ECDHE-ECDSA-AES256-GCM-SHA384", "ECDHE-RSA-AES256-GCM-SHA384"]
  cipher_suites_compatible = ["ECDHE-ECDSA-AES128-GCM-SHA256", "ECDHE-ECDSA-CHACHA20-POLY1305", "ECDHE-RSA-AES128-GCM-SHA256", "ECDHE-RSA-CHACHA20-POLY1305", "ECDHE-ECDSA-AES256-GCM-SHA384", "ECDHE-RSA-AES256-GCM-SHA384", "ECDHE-ECDSA-AES128-SHA256", "ECDHE-RSA-AES128-SHA256", "ECDHE-ECDSA-AES256-SHA384", "ECDHE-RSA-AES256-SHA384"]
  cipher_suites_legacy     = [] # Using an empty list cause Cloudflare to use the default (Legacy) cipher suites
}
