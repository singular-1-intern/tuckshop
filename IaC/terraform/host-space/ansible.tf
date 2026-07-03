# NOTE: Running Ansible has a number of prerequisites and limitations:
# 1. The Ops VM(s) must be provisioned in a separate run before provisioning with var.ansible.enabled = true.
# 2. Due to embedded shell scripts and configuration files, this file must be saved with linux style (LF) line endings.
# 3. This portion of the module only works on Linux. If you are running on Windows, you will need to disable Ansible in the tfvars file.
# 4. The linux host must have Ansible installed
# 5. The linux host must have the private SSH key in the user's .ssh folder
# 6. The SSH key's password must be set in the environment variable TF_VAR_ansible_ssh_passphrase
# 7. The Key Vault secrets for the Service Token's Client ID and Secret must be populated manually before the ansible run.

data "azurerm_key_vault_secret" "ops_vm_admin_user_passwords" {
  for_each = { for name, ops_vm in var.ops_vms : name => ops_vm if ops_vm.ansible_enabled }

  name         = local.ops_vm_admin_password_secret_names[each.key]
  key_vault_id = module.key_vault.key_vault.id

  depends_on = [module.key_vault]
}

# Zero Trust: Service Token and Machine Identity Certificate
data "azurerm_key_vault_secret" "ops_vm_zero_trust_service_token_client_ids" {
  for_each = { for name, value in local.ops_vm_secret_names_service_token_client_id : name => value }

  name         = each.value
  key_vault_id = module.key_vault.key_vault.id

  depends_on = [module.key_vault]
}

data "azurerm_key_vault_secret" "ops_vm_zero_trust_service_token_client_secrets" {
  for_each = { for name, value in local.ops_vm_secret_names_service_token_client_secret : name => value }

  name         = each.value
  key_vault_id = module.key_vault.key_vault.id

  depends_on = [module.key_vault]
}

# Certificates used to prove the identity of machine users
data "azurerm_key_vault_certificate" "ops_vm_identity_certificates" {
  for_each = { for name, ops_vm in var.ops_vms : local.ops_vm_certificate_keys[name] => ops_vm if ops_vm.features.warp_cli != null }

  name         = local.ops_vm_certificates[each.key].name
  key_vault_id = module.key_vault.key_vault.id

  depends_on = [module.key_vault]
}

# Nginx Proxy: API Key, Certificates
data "azurerm_key_vault_secret" "ops_vm_nginx_proxy_app_space_api_token_secrets" {
  for_each = { for name, value in local.ops_vm_secret_names_nginx_proxy_app_space_api_token : name => value }

  name         = each.value
  key_vault_id = module.key_vault.key_vault.id

  depends_on = [module.key_vault]
}

data "azurerm_key_vault_certificate_data" "ops_vm_nginx_proxy_certificates_data" {
  for_each = { for name, ops_vm in var.ops_vms : name => ops_vm if var.ops_vms[name].features.nginx_proxy != null }

  name         = local.ops_vm_nginx_proxy_certificates[local.ops_vm_nginx_proxy_certificate_keys[each.key]].name
  key_vault_id = module.key_vault.key_vault.id

  depends_on = [module.key_vault]
}

# Fetch any client certificates from User Secrets and save them as .cer files
data "azurerm_key_vault_secret" "ops_vm_nginx_proxy_client_certificates" {
  for_each = { for secret in local.ops_vm_nginx_proxy_client_certificates : secret.name => secret }

  name         = each.key
  key_vault_id = module.key_vault.key_vault.id

  depends_on = [module.key_vault]
}

# Nginx Proxy: Generate Configuration File
data "template_file" "nginx_location_files" {
  for_each = { for name, item in local.ops_vms_nginx_proxy_locations : name => item if local.ansible_enabled }

  template = file("${local.templates_path}/${each.value.location.template}")

  vars = {
    environment_prefix    = each.value.app_space_prefix
    nginx_proxy_api_key   = base64encode(data.azurerm_key_vault_secret.ops_vm_nginx_proxy_app_space_api_token_secrets[each.value.ops_vm_app_space_key].value)
    proxy_ssl_server_name = each.value.location.proxy_ssl_server_name ? "on" : "off"
    proxy_headers = join(
      "\n                ", # Indent needed for any header line after the first
      [for header in each.value.location.proxy_headers : "proxy_set_header ${header.name} \"${header.value}\";"]
    )

    path      = trimprefix(each.value.location.path, "/")
    proxy_url = each.value.location.proxy_url
  }
}

data "template_file" "nginx_config_file" {
  for_each = { for name, ops_vm in var.ops_vms : name => ops_vm if var.ops_vms[name].features.nginx_proxy != null && local.ansible_enabled }

  template = file(local.nginx_config_template_path)

  vars = {
    nginx-locations = join("\n\n", [for name, item in local.ops_vms_nginx_proxy_locations : data.template_file.nginx_location_files[name].rendered if item.ops_vm_name == each.key])
  }
}

module "configure_ops_vms" {
  source = "./../../../submodules/neo-iac-terraform/modules/ansible/neo_asb_run_playbook"
  # source = "./../../../../../devops/neo-iac-terraform/modules/ansible/neo_asb_run_playbook" # Use this for local module development

  for_each = { for name, ops_vm in var.ops_vms : name => ops_vm if local.ansible_enabled && ops_vm.ansible_enabled }

  playbook = {
    name         = "linux-vms-playbook.yaml"
    working_path = "${path.root}/../../ansible"
  }

  host = {
    group    = "ops",
    name     = module.ops_virtual_machines[each.key].virtual_machine.computer_name
    hostname = each.value.ansible_use_hostname ? module.ops_virtual_machines[each.key].virtual_machine.computer_name : module.ops_virtual_machines[each.key].network.private_ip
    username = module.ops_virtual_machines[each.key].virtual_machine.admin_username
  }

  host_secrets = {
    user_password              = data.azurerm_key_vault_secret.ops_vm_admin_user_passwords[each.key].value
    ssh_private_key_passphrase = var.ansible_ssh_passphrase
  }

  files = concat(
    each.value.features.node_exporter != null ? [
      # Custom Metrics Scripts
      {
        filename = "collect_warp_metrics.sh"
        content  = file("${path.root}/templates/collect_warp_metrics.sh")
      },
      {
        # Using 'template_file' didn't work here because of certain advanced bash script confusing the terraform templater.
        filename = "collect_endpoint_metrics.sh"
        content = replace(file("${path.root}/templates/collect_endpoint_metrics.sh"), "{ENDPOINTS}", join("\n  ", [
          for name, item in local.ops_vms_nginx_proxy_locations :
          join("", [
            "[\"${item.probe_url}\"]",
            "=\"neoauth: ${base64encode(data.azurerm_key_vault_secret.ops_vm_nginx_proxy_app_space_api_token_secrets[item.ops_vm_app_space_key].value)}\""
          ])
          if item.ops_vm_name == each.key && item.probe_url != null
        ]))
      }
    ] : [],

    each.value.features.warp_cli != null ? [
      # Certificate for use in gateway policies to verify identity
      {
        filename = "${module.ops_virtual_machines[each.key].virtual_machine.computer_name}.cer"
        content  = data.azurerm_key_vault_certificate.ops_vm_identity_certificates[local.ops_vm_certificate_keys[each.key]].certificate_data_base64
      }
    ] : [],

    # Nginx Proxy Certificate and Key Files
    each.value.features.nginx_proxy != null ? [
      {
        filename = "nginx-proxy.conf"
        content  = data.template_file.nginx_config_file[each.key].rendered
      },
      {
        filename = "nginx-proxy.crt"
        content  = data.azurerm_key_vault_certificate_data.ops_vm_nginx_proxy_certificates_data[each.key].pem
      },
      {
        filename = "nginx-proxy.key"
        content  = data.azurerm_key_vault_certificate_data.ops_vm_nginx_proxy_certificates_data[each.key].key
      }
    ] : [],

    # Nginx Proxy Client Certificates
    [
      for secret in local.ops_vm_nginx_proxy_client_certificates : {
        filename = "${secret.name}.crt"
        content  = data.azurerm_key_vault_secret.ops_vm_nginx_proxy_client_certificates[secret.name].value
      }
    ]
  )

  vars = join("\n", [
    each.value.features.node_exporter != null ?
    <<-DOC
      node_exporter_custom_metrics_scripts:
        - script: "{TEMP_PATH}/collect_warp_metrics.sh"
          interval: "*/2 * * * *"  # Every 2 minutes
        - script: "{TEMP_PATH}/collect_endpoint_metrics.sh"
          interval: "*/2 * * * *"  # Every 2 minutes
    DOC
    : "",

    each.value.features.warp_cli != null ?
    <<-DOC
      cloudflare_warp:
        team_name: "${each.value.features.warp_cli.team_name}"
        service_token_client_id: "${data.azurerm_key_vault_secret.ops_vm_zero_trust_service_token_client_ids[each.key].value}"
        service_token_client_secret: "${data.azurerm_key_vault_secret.ops_vm_zero_trust_service_token_client_secrets[each.key].value}"
        device_posture_files:
          - "{TEMP_PATH}/${module.ops_virtual_machines[each.key].virtual_machine.computer_name}.cer"
    DOC
    : "",

    each.value.features.nginx_proxy != null ?
    <<-DOC
      nginx_config_files:
        - "{TEMP_PATH}/nginx-proxy.conf"

      nginx_certificate_files:
        - "{TEMP_PATH}/nginx-proxy.crt"
        - "{TEMP_PATH}/nginx-proxy.key"
    DOC
    : "",

    length(each.value.features.nginx_proxy.client_certificates) == 0 ? "" :
    join("\n", [for certificate in local.ops_vm_nginx_proxy_client_certificates : "  - {TEMP_PATH}/${certificate.name}.crt" if certificate.ops_vm == each.key])
  ])

  dry_run       = var.ansible.dry_run_enabled
  cleanup_files = var.ansible.file_cleanup_enabled
}

