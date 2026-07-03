# ┌───────────────────────────────────────────────────────────────────────────┐
# │ Cloud Context Library                                                     │
# ├───────────────────────────────────────────────────────────────────────────┤
# │ Dependencies                                                              │
# │   Libraries : common.ps1; azure.ps1                                       │
# │   Tools     : Azure CLI; Azure PowerShell                                 │
# └───────────────────────────────────────────────────────────────────────────┘
<#
.SYNOPSIS
  Connects to Azure using Azure CLI.
  Parameters can either be passed explicitly, or set as environment variables by the CI/CD system.
  The environment variables use Azure SDK naming standards.

  This script requires that the following CLI tools are available:
  - Azure CLI (https://github.com/Azure/azure-cli)
.PARAMETER TenantId
  The Azure Tenant
  (Required. Defaults to the environment variable 'AZURE_TENANT_ID')
.PARAMETER SubscriptionId
  The Azure Subscription
  (Required. Defaults to the environment variable 'AZURE_SUBSCRIPTION_ID')
.PARAMETER ClientId
  The client / application id of the service principal to connect with.
  (Required. Defaults to the environment variable 'AZURE_CLIENT_ID'))
.PARAMETER ClientSecret
  The service principal's secret.
  Either ClientSecret or ClientCertificatePath must be provided.
  (Required. Defaults to the environment variable 'AZURE_CLIENT_SECRET')
.PARAMETER ClientCertificatePath
  Path to a certificate to use to authenticate service principal. (Expects a PEM or DER file containing the private key).
  Either ClientSecret or ClientCertificatePath must be provided.
  (Required. Defaults to the environment variable 'AZURE_CLIENT_CERTIFICATE_PATH')
.PARAMETER ClientCertificatePassword
  Password for the certificate to use to authenticate service principal.
  (Optional. Defaults to the environment variable 'AZURE_CLIENT_CERTIFICATE_PASSWORD')
  (ToDo: This is not used yet, but is here for future use. Investigate how to use it with the az CLI)
#>
[CmdletBinding()]
Param(
  [string]$TenantId = $Env:AZURE_TENANT_ID,
  [string]$SubscriptionId = $Env:AZURE_SUBSCRIPTION_ID,
  [string]$ClientId = $Env:AZURE_CLIENT_ID,
  [string]$ClientSecret = $Env:AZURE_CLIENT_SECRET,
  [string]$ClientCertificatePath = $Env:AZURE_CLIENT_CERTIFICATE_PATH,
  [string]$ClientCertificatePassword = $Env:AZURE_CLIENT_CERTIFICATE_PASSWORD,
  [switch]$ConnectPowerShellAz
)

# Locate the scripts root path and load libraries
$scriptsPath = $PSScriptRoot; while ($scriptsPath -and (Split-Path $scriptsPath -Leaf) -ne "scripts") { $scriptsPath = Split-Path $scriptsPath }
if (!$scriptsPath) { throw "Could not locate the scripts root folder. Please ensure this script is nested beneath a 'scripts' parent folder." }
. (Join-Path $scriptsPath "libraries/common.ps1")
. (Join-Path $scriptsPath "libraries/azure.ps1")

Write-Log "`n[ Configure Azure Context ]" $Global:LogStyles.Heading1Colour
$context = Get-GlobalContext

# Check if we have a valid user session.
# This is for local testing, if found we can skip Azure authentication.
# In a CI/CD scenario we'd always use a Service Principal and would not re-use the session)
$userContext = Get-AzureCliUserContext
$isUser = $null -ne $userContext

# Ensure that the parameters which use environment variable defaults actually have values.
if ([string]::IsNullOrEmpty($TenantId)) { throw "TenantId is required" }
if ([string]::IsNullOrEmpty($SubscriptionId)) { throw "SubscriptionId is required" }

if (!$isUser) {
  if ([string]::IsNullOrEmpty($ClientId)) { throw "ClientId is required" }
  if ([string]::IsNullOrEmpty($ClientSecret) -and [string]::IsNullOrEmpty($ClientCertificatePath)) { throw "Either ClientSecret or ClientCertificatePath must be provided" }
}

Write-Logs (Format-KeyValues ([ordered]@{
      "Tenant ID"                     = $TenantId
      "Subscription ID"               = $SubscriptionId
      ($isUser ? "User" : "ClientId") = $($true -eq $isUser ? $userContext.Account.user.name : $ClientId)
      "Principal Type"                = $($true -eq $isUser ? 'User' : 'Service Principal')
    })
) $Global:LogStyles.EmphasisColour

# Connect to Azure
if (!$isUser) {
  Write-Log "`nLogging into Azure"

  $login = $null
  if (![string]::IsNullOrEmpty($ClientSecret)) {
    $login = az login --service-principal --tenant $TenantId --username $ClientId --password $ClientSecret
  } else {
    $login = az login --service-principal --tenant $TenantId --username $ClientId --certificate $ClientCertificatePath
  }

  if (!$login -or (-not $?)) { throw "Unable to log into Azure (ClientId '$ClientId')" }
} else {
  Write-Log "`nExisting User login found, skipping authentication" $Global:LogStyles.SuppressedColour
}

# Set the active subscription
Write-Log "Setting Active Subscription to '$SubscriptionId'"
az account set --subscription $SubscriptionId
if (-not $?) {
  throw "Unable to switch to subscription $SubscriptionId"
}

if ($ConnectPowerShellAz.IsPresent) {
  $azPowerShellContext = Get-AzPowerShellContext
  Disable-AzContextAutosave -Scope Process | Out-Null # This is a security best practice to avoid persisting credentials to disk.

  if (-not $azPowerShellContext) {
    Write-Log "`nLogging PowerShell Az into Azure"
    if ($isUser) {
      throw "Unable to log into PowerShell Az as User, please run 'Connect-AzAccount -Subscription $SubscriptionId -Tenant $TenantId'" 
    } else {
      if ([string]::IsNullOrEmpty($ClientId)) { throw "ClientId is required" }
      if ([string]::IsNullOrEmpty($ClientSecret) -and [string]::IsNullOrEmpty($ClientCertificatePath)) { throw "Either ClientSecret or ClientCertificatePath must be provided to connect PowerShell Az" }
      $login = $null
      
      Write-Verbose "Connecting as Service Principal"
      if (![string]::IsNullOrEmpty($ClientSecret)) {
        $credential = [System.Management.Automation.PSCredential]::new($ClientId, $(ConvertTo-SecureString -String $ClientSecret -AsPlainText -Force))
        # See https://learn.microsoft.com/en-us/powershell/module/az.accounts/connect-azaccount?view=azps-14.4.0#serviceprincipalwithsubscriptionid
        $login = Connect-AzAccount -ServicePrincipal -TenantId $TenantId -Subscription $SubscriptionId -Credential $credential
        if (!$login -or (-not $?)) { throw "Unable to log into PowerShell Az as Service Principal using ClientId '$ClientId'" }
      } else {
        # See https://learn.microsoft.com/en-us/powershell/module/az.accounts/connect-azaccount?view=azps-14.4.0#serviceprincipalcertificatefilewithsubscriptionid
        $login = Connect-AzAccount -ServicePrincipal -TenantId $TenantId -Subscription $SubscriptionId -CertificatePath $ClientCertificatePath
        if (!$login -or (-not $?)) { throw "Unable to log into PowerShell Az as Service Principal using ClientId '$ClientId' and Certificate" }
      }
    }
  } else {
    Write-Log "`nExisting Az PowerShell login found, skipping authentication" $Global:LogStyles.SuppressedColour
  }
}

# Create the Azure Context and add it to the global context
$azContext = New-Context @{
  # Azure Details
  TenantId                = $TenantId
  SubscriptionId          = $SubscriptionId
  ClientId                = $ClientId
  ClientSecret            = $ClientSecret
  ClientCertificatePath   = $ClientCertificatePath

  # Connection Details
  Account                 = $isUser ? $userContext.Account : (az account show | ConvertFrom-Json)
  Token                   = $isUser ? $userContext.Token : (az account get-access-token | ConvertFrom-Json)
  IsUser                  = $isUser
  IsPowerShellAzConnected = $ConnectPowerShellAz.IsPresent
  CreatedOn               = (Get-Date)
} -Cleanup {
  if (!$this.IsUser) {
    Write-Log "Logging out of Azure..." $Global:LogStyles.SuppressedColour
    az logout
  }
}

$context.Add("Azure", $azContext, $true)

Write-Log "Azure Context Saved"

