# ┌───────────────────────────────────────────────────────────────────────────┐
# │ AKS Context Library                                                       │
# ├───────────────────────────────────────────────────────────────────────────┤
# │ Dependencies                                                              │
# │   Libraries : common.ps1; configure-cloud-provider-context.ps1            │
# │   Tools     : Azure CLI; kubectl; kubelogin                               │
# └───────────────────────────────────────────────────────────────────────────┘
<#
.SYNOPSIS
  Retrieves the kube config for an AKS cluster. An existing Azure connection must be established using Azure CLI.
  The parameters can either be passed explicitly, or set as environment variables by the CI/CD system.
  The environment variables use Azure SDK naming standards.

  This script requires that the following CLI tools are available:
  - Azure CLI (https://github.com/Azure/azure-cli)
  - kubelogin (https://github.com/Azure/kubelogin)
.PARAMETER ResourceGroup
  The name of the resource group that the cluster is under
.PARAMETER AksName
  The name of the AKS cluster resource
.PARAMETER UseDirectKubeLoginCall
  By default, the az aks get-credentials command will automatically call kubelogin to convert the kubeconfig file.
  This switch enables direct calls to kubelogin which uses Azure CLI authentication if az cli is connected as a User,
  otherwise it uses SPN authentication (Which relies on the AZURE_*  environment variables being present)
#>
[CmdletBinding()]
Param(
  [string]$ResourceGroup = $Env:AZURE_RESOURCE_GROUP,
  [string]$AksName = $Env:AZURE_AKS_NAME,
  [switch]$UseDirectKubeLoginCall
)

# Locate the scripts root path and load libraries
$scriptsPath = $PSScriptRoot; while ($scriptsPath -and (Split-Path $scriptsPath -Leaf) -ne "scripts") { $scriptsPath = Split-Path $scriptsPath }
if (!$scriptsPath) { throw "Could not locate the scripts root folder. Please ensure this script is nested beneath a 'scripts' parent folder." }
. (Join-Path $scriptsPath "libraries/common.ps1")

Write-Log "`n[ Configure AKS Context ]" $Global:LogStyles.Heading1Colour

# Confirm that the Azure context is available
$context = Get-GlobalContext
if ($null -eq $context.Azure) {
  throw "Azure context not found. Please run the 'configure-cloud-provider-context.ps1' script to configure the context."
}

# If SharedHosts state is available, use it to get the AKS Cluster details. Otherwise use the parameters.
if ($null -ne $context.State.SharedHosts) {
  Write-Log "Getting cluster details from Shared Hosts state"
  $aksCluster = $context.State.SharedHosts.aks_cluster
  $ResourceGroup = $aksCluster.aks.resource_group
  $AksName = $aksCluster.aks.name
}

# Ensure the parameters are set
if ([string]::IsNullOrEmpty($ResourceGroup)) { throw "ResourceGroup is required" }
if ([string]::IsNullOrEmpty($AksName)) { throw "AksName is required" }

Write-Log "`nAcquiring Kube Config" $Global:LogStyles.Heading2Colour
Write-Logs (Format-KeyValues ([ordered]@{
      "Resource Group" = $ResourceGroup
      "AKS Name"       = $AksName
    })
) $Global:LogStyles.EmphasisColour

try {
  # Generate a kube config file name unique to this cluster and service principal, then check if it already exists in the kube config folder.
  $UserFolder = $IsLinux ? $Env:HOME : $Env:USERPROFILE
  $ClientId = [string]::IsNullOrEmpty($context.Azure.ClientId) ? "default" : $context.Azure.ClientId
  $Env:KUBECONFIG = "$UserFolder/.kube/config_$($AksName)_$($ClientId)"

  if (Test-Path $Env:KUBECONFIG) {
    # A kube config file exists. Test if it is valid by checking connectivity using `kubectl version`.
    Write-Log "`nExisting kube config found. Testing connectivity..."
    $kubectlOutput = (kubectl version 2>&1)
    if ($kubectlOutput -match "Server Version") {
      Write-Log "Kube config is valid. No re-authentication is necessary." $Global:LogStyles.SuccessColour
      return
    } else {
      Write-Log "Kube config appears invalid [Error: $kubectlOutput]. Re-authenticating..." $Global:LogStyles.WarningColour
      # Remove the stale kube config file
      Remove-Item -Path $Env:KUBECONFIG -Force
    }
  }

  # We need to acquire a new kube config, so login to Azure
  # Check if the cluster is private with FQDN enabled
  Write-Log "`nGetting details for AKS cluster $AksName"
  $apiServerAccessProfile = (az aks show --resource-group $ResourceGroup --name $AksName --query "apiServerAccessProfile" -o json | ConvertFrom-Json)
  $aksPrivateLink = ($apiServerAccessProfile.enablePrivateCluster -eq $true -and $apiServerAccessProfile.enablePrivateClusterPublicFqdn -eq $true)

  # Request a kube config from Azure
  Write-Log "Getting credentials for AKS cluster $AksName" $Global:LogStyles.EmphasisColour

  if ($aksPrivateLink) {
    az aks get-credentials --resource-group $ResourceGroup --name $AksName --overwrite-existing --public-fqdn --file $Env:KUBECONFIG
  } else {
    az aks get-credentials --resource-group $ResourceGroup --name $AksName --overwrite-existing --file $Env:KUBECONFIG
  }

  if ($LASTEXITCODE -ne 0) {
    throw "Error retrieving AKS credentials from Azure"
  }

  if ($UseDirectKubeLoginCall) {
    # Use kubelogin to convert the kubeconfig to use exec credential plugin format and the service principal login method.
    if ($context.Azure.IsUser) {
      Write-Log "Using AzureCLI login method"
      kubelogin convert-kubeconfig -l azurecli --kubeconfig $Env:KUBECONFIG
    } else {
      Write-Log "Using SPN login method"
      kubelogin convert-kubeconfig -l spn --kubeconfig $Env:KUBECONFIG --client-id $context.Azure.ClientId --tenant-id $context.Azure.TenantId
    }

    if ($LASTEXITCODE -ne 0) {
      # Remove the kube config file, as it isn't correctly configured
      if (Test-Path $Env:KUBECONFIG) {
        Remove-Item -Path $Env:KUBECONFIG -Force
      }
      throw "An error occurred when running kubelogin"
    }
  }

  # Ensure the kube context is set to the cluster
  kubectl config use-context $AksName
  if ($LASTEXITCODE -ne 0) {
    throw "Unable to switch context to '$AksName'"
  }

  Write-Log "Kube config configured successfully." $Global:LogStyles.SuccessColour
} finally {
  # Set the credential environment variables for kubelogin to pick up
  if (!$context.Azure.IsUser) {
    $Env:AAD_SERVICE_PRINCIPAL_CLIENT_ID = $context.Azure.ClientId
    $Env:AAD_SERVICE_PRINCIPAL_CLIENT_SECRET = $context.Azure.ClientSecret
    $Env:AAD_SERVICE_PRINCIPAL_CLIENT_CERTIFICATE = $context.Azure.ClientCertificatePath
    $Env:AAD_SERVICE_PRINCIPAL_CLIENT_CERTIFICATE_PASSWORD = $context.Azure.ClientCertificatePassword
  }

  # Always add the context so that even if an error occurs, we ensure that any sensitive
  # environment variables are cleared.
  $kubeConfigContext = New-Context @{} -Cleanup {
    Write-Log "Cleanup: Clearing kubelogin environment variables" $Global:LogStyles.SuppressedColour
    $Env:KUBECONFIG = $null
    $Env:AAD_SERVICE_PRINCIPAL_CLIENT_ID = $null
    $Env:AAD_SERVICE_PRINCIPAL_CLIENT_SECRET = $null
    $Env:AAD_SERVICE_PRINCIPAL_CLIENT_CERTIFICATE = $null
    $Env:AAD_SERVICE_PRINCIPAL_CLIENT_CERTIFICATE_PASSWORD = $null
  }

  $context.Add("KubeConfig", $kubeConfigContext, $true)
}


