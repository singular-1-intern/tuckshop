resource "azurerm_container_registry" "container_registry" {
  count = var.container_registry.enabled ? 1 : 0

  name                = join("", ["singular", var.prefixes.client_project, var.azure.location_prefix])
  resource_group_name = data.azurerm_resource_group.main.name
  location            = var.azure.location
  sku                 = var.container_registry.sku
}

## acr_pull Service Principal

# Create an Azure AD application
resource "azuread_application" "acr_pull" {
  count = var.container_registry.enabled ? 1 : 0

  display_name     = join("-", [var.prefixes.client_project, "sp-acrpull"])
  sign_in_audience = "AzureADMyOrg"

  lifecycle {
    ignore_changes = [owners]
  }
}

# Create Service Principal associated with the Azure AD App
resource "azuread_service_principal" "acr_pull" {
  count = var.container_registry.enabled ? 1 : 0

  client_id = azuread_application.acr_pull[0].client_id

  lifecycle {
    ignore_changes = [owners]
  }
}

# Create Service Principal password that expires in 100 years
resource "time_static" "service_principal_password" {}
resource "azuread_service_principal_password" "acr_pull" {
  count = var.container_registry.enabled ? 1 : 0

  service_principal_id = azuread_service_principal.acr_pull[0].id
  end_date             = timeadd(time_static.service_principal_password.rfc3339, "867000h")
  rotate_when_changed = {
    secret_version = var.container_registry.secret_version
  }

  lifecycle {
    # Have had cases where the time_static resource seems to have shifted its date, which was resulting in terraform
    # wanting to recreate this password. (You may also see the provider saying that "end_date_relative" is deprecated,
    # but if the field is removed, then terraform goes back to wanting to recreate the password.)
    ignore_changes = [end_date, end_date_relative]
  }
}

# Assign roles to the above service principals
resource "azurerm_role_assignment" "acr_pull" {
  count = var.container_registry.enabled ? 1 : 0

  principal_id                     = azuread_service_principal.acr_pull[0].object_id
  role_definition_name             = "AcrPull"
  scope                            = azurerm_container_registry.container_registry[0].id
  skip_service_principal_aad_check = true
}
