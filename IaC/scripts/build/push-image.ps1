<#
.SYNOPSIS
  Pushes a local docker image to an Azure Container Registry
.DESCRIPTION
  Takes a previously build image from your local docker repository and pushes it up to Azure Container Registry.
.PARAMETER Registry
  The name of the container registry (e.g. singular.azurecr.io)
.PARAMETER Repository
  The name of the repository within the container registry
.PARAMETER Tag
  The image tag of the docker image
.PARAMETER TenantId
  The Azure Tenant where the ACR is located.
  Defaults to the 'AZURE_TENANT_ID' environment variable.
.PARAMETER ServicePrincipal
  The Service Principal to connect to Azure with. Must have the 'AcrPush' role on the registry.
  Defaults to the 'AZURE_CLIENT_ID' environment variable.
.PARAMETER ServicePrincipalSecret
  The Service Principal's secret
  Defaults to the 'AZURE_CLIENT_SECRET' environment variable.
#>
[CmdletBinding()]
param(
  [Parameter(Mandatory)][string]$Repository,
  [Parameter(Mandatory)][string]$Tag,
  [string]$Registry = $Env:ContainerRegistry,
  [string]$TenantId = $Env:AZURE_TENANT_ID,
  [string]$ServicePrincipal = $Env:AZURE_CLIENT_ID,
  [string]$ServicePrincipalSecret = $Env:AZURE_CLIENT_SECRET
)

# Stop on any unhandled error
$ErrorActionPreference = "Stop"

# Locate the scripts root path and load libraries
$scriptsPath = $PSScriptRoot; while ($scriptsPath -and (Split-Path $scriptsPath -Leaf) -ne "scripts") { $scriptsPath = Split-Path $scriptsPath }
if (!$scriptsPath) { throw "Could not locate the scripts root folder. Please ensure this script is nested beneath a 'scripts' parent folder." }
. (Join-Path $scriptsPath "libraries/common.ps1")

Write-Logs @(
  "`n",
  " █▀█ █ █ █▀▀ █ █   ▀█▀ █▄█ █▀█ █▀▀ █▀▀",
  " █▀▀ █ █ ▀▀█ █▀█    █  █ █ █▀█ █ █ █▀▀",
  " ▀   ▀▀▀ ▀▀▀ ▀ ▀   ▀▀▀ ▀ ▀ ▀ ▀ ▀▀▀ ▀▀▀"
  ""
) $Global:LogStyles.Heading1Colour

# Configure context for the runtime environment if a script is present.
$runtimeContextScript = "$scriptsPath/context/configure-runtime-context.ps1"
if (Test-Path $runtimeContextScript) {
  & $runtimeContextScript
}

Write-Log "`n[ Push Image to Container Registry ]" $Global:LogStyles.Heading1Colour

if ([string]::IsNullOrEmpty($Registry)) {
  throw "Container registry name must be provided"
}

try {
  Write-Log "`nConnecting to Registry" $Global:LogStyles.Heading1Colour
  Write-Log "Registry: $Registry"

  # Log into Azure
  Write-Log "Logging into Tenant: $TenantId"
  $login = az login `
    --service-principal `
    --tenant $TenantId `
    --username $ServicePrincipal `
    --password $ServicePrincipalSecret

  if (!$login) {
    throw "Error logging into Azure"
  }

  # Log into the Azure Container Registry
  Write-Log "Logging into Azure Container Registry: $Registry"
  $acrLogin = az acr login --name $Registry

  if (!$acrLogin) {
    throw "Error logging into Azure Container Registry"
  }

  Write-Log "Connected to registry"

  # Push the image to the registry
  $imageName = "$($Registry)/$($Repository):$($Tag)"
  Write-Log "`nPushing image to registry" $Global:LogStyles.Heading1Colour
  Write-Log "Image name: $imageName"

  docker push $imageName
  if (-not $?) {
    throw "Error pushing docker image"
  }

  Write-Log "`nContainer image push complete`n" $Global:LogStyles.SuccessColour
} finally {
  # Sometimes the connection times out, so if there's no active login, suppress the error.
  try { az logout } catch { }
}

