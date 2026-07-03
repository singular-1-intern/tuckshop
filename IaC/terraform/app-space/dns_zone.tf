# Cloudflare DNS Zones
module "cloudflare_zones" {
  source = "./../../../submodules/neo-iac-terraform/modules/cloudflare/neo_cf_dns_zones_v5"
  # source = "./../../../../neo-iac-terraform/modules/cloudflare/neo_cf_dns_zones_v5" # Use this for local module development

  count = var.dns_zone != null && var.dns_zone.enabled ? 1 : 0

  domains = [var.dns_zone.domain]
}

# Adhoc DNS records for the DNS Zone
resource "cloudflare_dns_record" "dns_entries" {
  for_each = length(data.kubernetes_secret_v1.cloudflare_api_token) > 0 ? { for dns_record in(var.dns_zone.enabled ? local.dns_records : []) : "${dns_record.type}_${dns_record.name}" => dns_record } : {}

  zone_id  = module.cloudflare_zones[0].dns_zones[var.dns_zone.domain].id
  type     = each.value.type
  name     = each.value.name
  content  = each.value.value
  proxied  = each.value.proxied
  ttl      = each.value.proxied ? 1 : 300
  priority = each.value.priority
}

resource "cloudflare_zone_setting" "main_zone_max_upload" {
  count = length(module.cloudflare_zones) > 0 ? 1 : 0

  zone_id    = module.cloudflare_zones[0].dns_zones[var.dns_zone.domain].id
  setting_id = "max_upload"
  value      = var.dns_zone.max_upload
}

resource "cloudflare_zone_setting" "main_zone_min_tls_version" {
  count = length(module.cloudflare_zones) > 0 ? 1 : 0

  zone_id    = module.cloudflare_zones[0].dns_zones[var.dns_zone.domain].id
  setting_id = "min_tls_version"
  value      = var.dns_zone.minimum_tls_version
}

resource "cloudflare_zone_setting" "main_zone_ssl" {
  count = length(module.cloudflare_zones) > 0 ? 1 : 0

  zone_id    = module.cloudflare_zones[0].dns_zones[var.dns_zone.domain].id
  setting_id = "ssl"
  value      = var.dns_zone.ssl
}

resource "cloudflare_zone_setting" "main_zone_tls_1_3" {
  count = length(module.cloudflare_zones) > 0 ? 1 : 0

  zone_id    = module.cloudflare_zones[0].dns_zones[var.dns_zone.domain].id
  setting_id = "tls_1_3"
  value      = "on"
}

resource "cloudflare_zone_setting" "main_zone_ciphers" {
  count = length(module.cloudflare_zones) > 0 && var.dns_zone.cipher_suites != "legacy" ? 1 : 0

  zone_id    = module.cloudflare_zones[0].dns_zones[var.dns_zone.domain].id
  setting_id = "ciphers"
  value = concat(
    var.dns_zone.cipher_suites == "modern" ? local.cipher_suites_modern : [],
    var.dns_zone.cipher_suites == "compatible" ? local.cipher_suites_compatible : [],
    var.dns_zone.cipher_suites == "legacy" ? local.cipher_suites_legacy : []
  )
}

# Create Subdomains for any apps which have a subdomain explicitly set
resource "cloudflare_dns_record" "app_dns_records" {
  for_each = local.app_dns_records

  zone_id = module.cloudflare_zones[0].dns_zones[var.dns_zone.domain].id
  type    = "A"
  name    = each.value.name
  content = each.value.content
  proxied = var.dns_zone.proxying_enabled
  ttl     = var.dns_zone.proxying_enabled ? 1 : 300
}
