resource "azurerm_service_plan" "main" {
  count = var.app_services.enabled ? 1 : 0

  name                = local.app_service_plan_name
  resource_group_name = local.resource_group_name
  location            = local.resource_group.location

  os_type  = "Linux"
  sku_name = var.app_services.sku
}

resource "azurerm_linux_web_app" "app_services" {
  for_each = { for app in(var.app_services.enabled ? var.apps : []) : app.name => app }

  name                = join("-", [local.resource_prefix, "app", each.value.name])
  resource_group_name = local.resource_group_name
  location            = azurerm_service_plan.main[0].location
  service_plan_id     = azurerm_service_plan.main[0].id

  site_config {
    always_on = contains(["FREE", "F1", "D1", "SHARED"], var.app_services.sku) ? false : true
  }

  dynamic "identity" {
    for_each = each.value.requires_identity ? [1] : []
    content {
      type         = "UserAssigned"
      identity_ids = [azurerm_user_assigned_identity.app_identities[each.value.name].id]
    }
  }

  # Not sure of this is necessary of we only have one identity specified in the identity block above
  key_vault_reference_identity_id = each.value.requires_identity ? azurerm_user_assigned_identity.app_identities[each.value.name].id : null

  lifecycle {
    ignore_changes = [
      # Ignore changes to app_settings and connection_string because a these are updated by the deployment script
      app_settings, connection_string, logs, site_config, tags
    ]
  }
}

# NOTE: For this custom domain name to bind you need to have the DNS TXT validation record and CNAME record created first in the DNS zone.
resource "azurerm_app_service_custom_hostname_binding" "app_services_custom_domains" {
  for_each = { for app in(var.app_services.enabled && var.dns_zone.enabled ? var.apps : []) : app.name => app }

  hostname            = join(".", [each.value.sub_domain == "" ? each.value.name : each.value.sub_domain, var.dns_zone.domain])
  app_service_name    = azurerm_linux_web_app.app_services[each.value.name].name
  resource_group_name = local.resource_group_name

  # Use this if you want to force the DNS record to be created first.
  depends_on = [
    cloudflare_dns_record.app_services_txt_dns_records,
    cloudflare_dns_record.app_services_cname_dns_records
  ]
}

# ToDo: See if there is a way around chicken-egg scenario here where the static website feature can't be enabled
#       before the DNS record exists, and the DNS record needing to refer to the static website endpoint, which is an
#       output of the storage account. There is another option where you can use an 'asverify' subdomain instead and
#       precreate the DNS record.

resource "cloudflare_dns_record" "app_services_cname_dns_records" {
  for_each = { for app in(var.app_services.enabled && var.dns_zone.enabled ? var.apps : []) : app.name => app }

  zone_id = module.cloudflare_zones[0].dns_zones[var.dns_zone.domain].id
  type    = "CNAME"
  name    = each.value.sub_domain == "" ? each.value.name : each.value.sub_domain

  content = azurerm_linux_web_app.app_services[each.value.name].default_hostname

  # On initial create, proxied must = false because the Azure Storage static website expects the CNAME DNS entry to exist before the storage account.
  # The error looks like this: "The custom domain name could not be verified. CNAME mapping from [var.dns_zone.domain] to any of
  # [local.static_website_storage_name].blob.core.windows.net,[local.static_website_storage_name].z6.web.core.windows.net does not exist.""
  proxied = var.dns_zone.proxying_enabled
  ttl     = var.dns_zone.proxying_enabled ? 1 : 300
}

resource "cloudflare_dns_record" "app_services_txt_dns_records" {
  for_each = { for app in(var.app_services.enabled && var.dns_zone.enabled ? var.apps : []) : app.name => app }

  zone_id = module.cloudflare_zones[0].dns_zones[var.dns_zone.domain].id
  type    = "TXT"
  name    = join(".", ["asuid", each.value.sub_domain == "" ? each.value.name : each.value.sub_domain])

  content = azurerm_linux_web_app.app_services[each.value.name].custom_domain_verification_id
  proxied = false
  ttl     = 300
}
