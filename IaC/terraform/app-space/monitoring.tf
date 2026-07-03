resource "kubernetes_manifest" "alertmanager_config" {
  for_each = { for index, alerts in(local.k8s_namespace != null ? var.alerts : []) : join("-", [local.k8s_resource_prefix, index]) => alerts }

  manifest = {
    apiVersion = "monitoring.coreos.com/v1alpha1"
    kind       = "AlertmanagerConfig"

    metadata = {
      name      = join("-", [local.k8s_resource_prefix, "alerts-config", each.key])
      namespace = local.k8s_namespace.namespace.name

      labels = {
        alertmanagerConfig = "app-space"
      }
    }

    spec = {
      route = {
        receiver       = join("-", [local.resource_prefix, "app-space", "dev-team"])
        continue       = true
        groupBy        = each.value.group_by
        groupWait      = each.value.group_wait
        groupInterval  = each.value.group_interval
        repeatInterval = each.value.repeat_interval
        matchers = [for matcher in each.value.matchers : {
          name      = matcher.name
          value     = replace(matcher.value, "{k8s_namespace}", local.k8s_namespace.namespace.name) # Inject namespace into value if template value is present.
          matchType = matcher.match_type
        }]
      }

      receivers = [
        {
          name = join("-", [local.resource_prefix, "app-space", "dev-team"])
          emailConfigs = [
            {
              sendResolved = each.value.email.send_resolved
              to           = join(",", each.value.email.receivers)
            }
          ]
        }
      ]
    }
  }
}
