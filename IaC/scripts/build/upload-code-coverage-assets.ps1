<#
.SYNOPSIS
    Uploads code coverage assets to the cloud.
.PARAMETER AssetsLocation
    The location of the code coverage assets on disk to upload.
.PARAMETER ClientName
    The name of the client to upload the code coverage assets for.
.PARAMETER NeoAzTenantId
    The Azure Tenant Id
.PARAMETER NeoAzSubscriptionId
    The Azure Subscription Id
.PARAMETER NeoAzServicePrincipalClientId
    The Service Principal Client Id
.PARAMETER NeoAzServicePrincipalSecret
    The Service Principal Secret
#>
[CmdletBinding()]
param(
    [Parameter()][string]$AssetsLocation = $Env:CodeCoverageReportAssetsLocation ?? "/tmp/code-coverage",
    [Parameter()][string]$ClientName = $Env:CodeCoverageClientName ?? "SingularSystems",
    [string]$NeoAzTenantId = $Env:AZURE_TENANT_ID ?? $Env:NeoAzTenantId,
    [string]$NeoAzSubscriptionId = $Env:AZURE_SUBSCRIPTION_ID ?? $Env:NeoAzSubscriptionId,
    [string]$NeoAzServicePrincipalClientId = $Env:AZURE_CLIENT_ID ?? $Env:NeoAzServicePrincipalClientId,
    [string]$NeoAzServicePrincipalSecret = $Env:AZURE_CLIENT_SECRET ?? $Env:NeoAzServicePrincipalSecret
)

if ([System.Convert]::ToBoolean($Env:CodeCoverageEnabled)) {
    # Allow the environment variable(s) to be overridden if an argument has been supplied.
    $Env:AZURE_TENANT_ID, $Env:NeoAzTenantId = $NeoAzTenantId, $NeoAzTenantId
    $Env:AZURE_SUBSCRIPTION_ID, $Env:NeoAzSubscriptionId = $NeoAzSubscriptionId, $NeoAzSubscriptionId
    $Env:AZURE_CLIENT_ID, $Env:NeoAzServicePrincipalClientId = $NeoAzServicePrincipalClientId, $NeoAzServicePrincipalClientId
    $Env:AZURE_CLIENT_SECRET, $Env:NeoAzServicePrincipalSecret = $NeoAzServicePrincipalSecret, $NeoAzServicePrincipalSecret
    
    Push-NeoCldCodeCoverageAssets `
        -ReportsAssetsLocation $AssetsLocation `
        -ClientName $ClientName `
        -Verbose
}

