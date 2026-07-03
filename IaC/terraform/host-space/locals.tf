locals {
  environment_prefix     = terraform.workspace
  shared_resource_prefix = join("-", compact([var.prefixes.core, var.azure.location_prefix, var.prefixes.shared_hosts]))
  short_resource_prefix  = join("-", compact([var.prefixes.client_project, local.environment_prefix]))
  resource_prefix        = join("-", compact([var.prefixes.client_project, var.azure.location_prefix, local.environment_prefix]))

  tags = {
    project       = var.prefixes.client_project
    project_label = var.project_label
    environment   = local.environment_prefix
  }

  # Host state
  host_space_base_state = jsondecode(data.azurerm_automation_variable_string.host_space_base_state.value)
  shared_host_state     = jsondecode(data.azurerm_automation_variable_string.shared_host_state.value)

  # Service Principals
  service_principal_names = {
    provisioners = local.host_space_base_state.service_principals.provisioners
    deployers    = local.host_space_base_state.service_principals.deployers
    operations   = local.host_space_base_state.service_principals.operations
  }

  # Azure Resource Names
  resource_group_name     = join("-", [local.resource_prefix, "rg", "host_space"])
  automation_account_name = join("-", [local.resource_prefix, "aa", "main"])

  # Networking
  networking_enabled    = length(keys(var.sql_servers)) > 0 || length(keys(var.ops_vms)) > 0
  network_subnet_name   = "servers"
  private_ip_ranges     = ["10.0.0.0/8", "172.16.0.0/12", "192.168.0.0/16"]
  nat_gateway_available = try(local.shared_host_state.network.nat_gateway != null, false)
  nat_gateway_id        = try(local.shared_host_state.network.nat_gateway.id, null)

  # App Spaces
  app_spaces = local.host_space_base_state.app_spaces

  # AKS Cluster
  k8s_state_available  = try(local.shared_host_state.aks_cluster != null, false)
  k8s_provisioned      = var.aks_cluster != null || local.k8s_state_available
  k8s_namespace        = try(local.k8s_namespaces[keys(local.k8s_namespaces)[0]].namespace.name, var.aks_cluster.namespace, "")
  k8s_namespace_exists = local.k8s_namespace != ""

  aks_cluster_resource = local.k8s_provisioned ? {
    name           = local.k8s_state_available ? local.shared_host_state.aks_cluster.aks.name : var.aks_cluster.name
    resource_group = local.k8s_state_available ? local.shared_host_state.aks_cluster.aks.resource_group : var.aks_cluster.resource_group
  } : null

  aks_cluster    = local.k8s_provisioned ? data.azurerm_kubernetes_cluster.main[0] : null
  k8s_namespaces = local.k8s_provisioned ? local.host_space_base_state.k8s_namespaces : null

  # Only enable alerts if the host space has a prefix which is not the same as one of the app spaces
  # (Otherwise the configuration on the app-space will cater for any alerts coming from the host space)
  k8s_alerts_enabled = try(local.k8s_namespaces != null && !anytrue([for key in keys(local.k8s_namespaces) : key == local.resource_prefix]), false)

  # A cluster certificate will only be retrieved here if the cluster is provisioned, and the user has access to the cluster. (If the user does not, the kube_config will be null)
  aks_cluster_ca_certificate = try(base64decode(local.aks_cluster.kube_config.0.cluster_ca_certificate), "")

  # SQL Server
  # (Note about Azure IPs. In a subnet, azure reserves the first four and last IP in the range, for a total of 5 IPs. Hence we need to start at index + 4)
  sql_server_cidr_offset   = 4 + length(keys(var.ops_vms))
  sql_server_ips           = { for index, name in keys(var.sql_servers) : name => cidrhost(local.host_space_base_state.cidr_range, index + local.sql_server_cidr_offset) }
  sql_server_app_usernames = { for name, sql_server in var.sql_servers : name => join("-", [local.short_resource_prefix, name, "app"]) }

  # Ops VMs
  ops_vm_admin_password_secret_names = { for name, ops_vm in var.ops_vms : name => join("-", compact(["vm", "ops", name, "admin-password"])) }

  ops_vms_cidr_offset = 4
  ops_vms_ips = {
    for index, name in keys(var.ops_vms) :
    name => coalesce(var.ops_vms[name].private_ip, cidrhost(local.host_space_base_state.cidr_range, index + local.ops_vms_cidr_offset))
  }

  # Generate a lookup for resources that must be created both per Ops VM and App Space
  ops_vm_app_spaces = merge([
    for name, ops_vm in var.ops_vms : {
      for space_prefix, app_space in local.app_spaces : join("-", [name, space_prefix]) => {
        ops_vm_name      = name
        ops_vm           = ops_vm
        app_space_prefix = space_prefix
        app_space        = app_space
      }
    }
  ]...)

  # Secrets
  sql_server_admin_username_secret_names = { for name, sql_server in var.sql_servers : name => join("-", ["vm", "sql", name, "admin-username"]) }
  sql_server_admin_password_secret_names = { for name, sql_server in var.sql_servers : name => join("-", ["vm", "sql", name, "admin-password"]) }
  sql_server_app_username_secret_names   = { for name, sql_server in var.sql_servers : name => join("-", ["vm", "sql", name, "app-username"]) }
  sql_server_app_password_secret_names   = { for name, sql_server in var.sql_servers : name => join("-", ["vm", "sql", name, "app-password"]) }

  generated_secret_names = flatten([
    values(local.sql_server_admin_password_secret_names),
    values(local.sql_server_app_password_secret_names),
    values(local.ops_vm_admin_password_secret_names),
    values(local.ops_vm_secret_names_nginx_proxy_app_space_api_token)
  ])

  secret_names = flatten([
    values(local.sql_server_admin_username_secret_names),
    values(local.sql_server_app_username_secret_names),
    [for app in azuread_application.acr_pull : join("-", [app.display_name, "password"])]
  ])

  secret_values = flatten([
    [for sql_vm in module.sql_server_base_virtual_machines : sql_vm.virtual_machine.admin_username],
    values(local.sql_server_app_usernames),
    [for password in azuread_service_principal_password.acr_pull : password.value]
  ])

  user_secrets = concat(
    values(local.ops_vm_secret_names_service_token_client_id),
    values(local.ops_vm_secret_names_service_token_client_secret),
    [for client_certificate in local.ops_vm_nginx_proxy_client_certificates : client_certificate.name]
  )

  secret_properties = merge(
    { for secret_name in local.generated_secret_names : secret_name => { allow_copy = true } },
    { for secret_name in local.secret_names : secret_name => { allow_copy = true } }
  )

  # Ansible (Should only run if enable and there's at least one VM to work on)
  ansible_any_hosts       = length([for name, ops_vm in var.ops_vms : ops_vm if ops_vm.ansible_enabled == true]) > 0
  ansible_enabled         = try(var.ansible.enabled, false) && local.ansible_any_hosts
  ansible_private_ssh_key = try(pathexpand(var.ansible.private_ssh_key), "")

  templates_path             = "${path.module}/templates"
  nginx_config_template_path = "${path.module}/templates/nginx.conf"
}

