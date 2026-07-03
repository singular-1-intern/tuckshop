module "kube_prometheus_stack_resources" {
  source = "./../../../submodules/neo-iac-terraform/modules/kubernetes/neo_k8s_kube_prometheus_stack_resources"
  # source = "./../../../../neo-iac-terraform/modules/kubernetes/neo_k8s_kube_prometheus_stack_resources" # Use this for local module development

  resources = {
    namespace = local.k8s_namespace
    prefix    = local.resource_prefix
  }

  service_monitor_endpoints = concat(
    [
      for name, sql_server in var.sql_servers : {
        namespace = local.k8s_namespace
        name      = module.sql_server_base_virtual_machines[name].virtual_machine.computer_name

        metric_labels = {
          project          = var.prefixes.client_project
          environment      = local.environment_prefix
          location         = var.azure.location_prefix
          resource_type    = "virtual-machine"
          resource_role    = "sql-server"
          exporter_type    = "windows-exporter"
          hostname         = module.sql_server_base_virtual_machines[name].virtual_machine.computer_name
          operating_system = "windows"
          alerts_enabled   = var.sql_servers[name].enable_alerts
        }

        endpoint = {
          private_ip = local.sql_server_ips[name]
          hostname   = module.sql_server_base_virtual_machines[name].virtual_machine.computer_name
          port       = 9182
        }
      } if var.sql_servers[name].service_monitor_enabled
    ],
    [
      for name, ops_vm in var.ops_vms : {
        namespace = local.k8s_namespace
        name      = module.ops_virtual_machines[name].virtual_machine.computer_name

        metric_labels = {
          project          = var.prefixes.client_project
          environment      = local.environment_prefix
          location         = var.azure.location_prefix
          resource_type    = "virtual-machine"
          resource_role    = "ops"
          exporter_type    = "prometheus-exporter"
          hostname         = module.ops_virtual_machines[name].virtual_machine.computer_name
          operating_system = "linux"
          alerts_enabled   = var.ops_vms[name].enable_alerts
        }

        endpoint = {
          private_ip = local.ops_vms_ips[name]
          hostname   = module.ops_virtual_machines[name].virtual_machine.computer_name
          port       = 9100
        }
      } if var.ops_vms[name].service_monitor_enabled
    ]
  )
}
