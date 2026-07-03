using module Neo.PS.Azure # This is required to import any module classes or enums referenced by this script.

<#
.SYNOPSIS
  This script copies secrets from one Azure Key Vault to another, specifically for a disaster recovery scenario.
.PARAMETER Project
  The project prefix, used in name generation
.PARAMETER SourceLocation
  The Azure location of the source key vault
.PARAMETER TargetLocation
  The Azure Location of the target key vault in the disaster recover environment
.PARAMETER TargetEnvironment
  The target disaster recovery environment to copy secrets to
.PARAMETER SourceEnvironment
  The source environment to copy secrets from
.EXAMPLE
  .\azure-keyvault-secret-copy.ps1 -Project "nsmp" -SourceLocation "we" -TargetLocation "ne" -TargetEnvironment "ddr"
#>
[CmdletBinding()]
param(
  [Parameter(Mandatory)][string]$Project,
  [Parameter(Mandatory)][string]$SourceLocation,
  [Parameter(Mandatory)][string]$SourceEnvironment,
  [Parameter(Mandatory)][string]$TargetLocation,
  [Parameter(Mandatory)][string]$TargetEnvironment
)

try {
  $ErrorActionPreference = "Stop"
  $InformationPreference = "Continue"
  $VerbosePreference = ($PSBoundParameters["Verbose"] -or [System.Convert]::ToBoolean($Env:NeoVerboseLogging)) ? "Continue" : "SilentlyContinue"
  $DebugPreference = ($PSBoundParameters["Debug"] -or [System.Convert]::ToBoolean($Env:NeoDebugLogging)) ? "Continue" : "SilentlyContinue"
  $stopWatch = [System.Diagnostics.Stopwatch]::StartNew()

  # Locate the scripts root path and load libraries
  $scriptsPath = $PSScriptRoot; while ($scriptsPath -and (Split-Path $scriptsPath -Leaf) -ne "scripts") { $scriptsPath = Split-Path $scriptsPath }
  if (!$scriptsPath) { throw "Could not locate the scripts root folder. Please ensure this script is nested beneath a 'scripts' parent folder." }
  . (Join-Path $scriptsPath "libraries/common.ps1")

  # This causes the $Global:Context to be reset for the script run's session.
  $context = Get-GlobalContext -New

  # Configure context for the runtime environment if a script is present.
  $contextScript = "$scriptsPath/context/configure-runtime-context.ps1"
  if (Test-Path $contextScript) {
    & $contextScript
  }

  # Configure cloud provider context if a script is present.
  $cloudContextScript = "$scriptsPath/context/configure-cloud-context.ps1"
  if (Test-Path $cloudContextScript) {
    & $cloudContextScript -ConnectPowerShellAz
  }

  # Configure state context for source and target environments and capture the resulting app-space states
  $sourceStateKey = "SourceState"
  $targetStateKey = "TargetState"
  $stateContextScript = "$scriptsPath/context/configure-state-context.ps1"
  if (Test-Path $stateContextScript) {
    Write-NeoLog "`nConfiguring state context for source environment" $Global:NeoLogStyles.EmphasisColour
    & $stateContextScript -StateKey $sourceStateKey -ProjectPrefix $Project -LocationPrefix $SourceLocation -Environment $SourceEnvironment
    [PSCustomObject]$sourceContext = $context.$sourceStateKey

    Write-NeoLog "`nConfiguring state context for target environment" $Global:NeoLogStyles.EmphasisColour
    & $stateContextScript -StateKey $targetStateKey -ProjectPrefix $Project -LocationPrefix $TargetLocation -Environment $TargetEnvironment
    [PSCustomObject]$targetContext = $context.$targetStateKey
  } else {
    throw "Could not locate the state context script. Please ensure '$stateContextScript' exists."
  }

  $sourceAppSpaceState = $sourceContext.AppSpace
  $sourceSubscriptionId = $sourceAppSpaceState.azure.subscription_id
  $sourceResourceGroup = $sourceAppSpaceState.resource_group.name
  $sourceVaultName = $sourceAppSpaceState.key_vault.key_vault.name

  $targetAppSpaceState = $targetContext.AppSpace
  $targetSubscriptionId = $targetAppSpaceState.azure.subscription_id
  $targetResourceGroup = $targetAppSpaceState.resource_group.name
  $targetVaultName = $targetAppSpaceState.key_vault.key_vault.name

  # For now we make use of the 1st SQL Server in the App Space because full support for multiple SQL Servers is not yet implemented across the board.
  $sqlStateKey = $sourceAppSpaceState.sql_servers ? "sql_servers" : "sql_server"
  $sqlKey = $sqlStateKey -eq "sql_servers" ? ($sourceAppSpaceState.$sqlStateKey[0].PSObject.Properties | Select-Object -ExpandProperty Name) : ($sourceAppSpaceState.$sqlStateKey.key)
  $sqlType = $sqlStateKey -eq "sql_servers" ? ($sourceAppSpaceState.$sqlStateKey.$sqlKey.type) : ($sourceAppSpaceState.$sqlStateKey.type)

  # Work out if the host space is shared or dedicated
  $isDedicatedSql = ($sqlType -eq [SqlServerTypeEnum]::Dedicated)
  $sourceHostVaultName = $isDedicatedSql ? $sourceContext.HostSpace.key_vault.vault.name : $sourceContext.SharedHosts.key_vault.name
  $sourceHostVaultResourceGroup = $isDedicatedSql ? $sourceContext.HostSpace.key_vault.vault.resource_group : $sourceContext.SharedHosts.resource_group.name

  Write-NeoLogHeading "Key Vault Secret Copy Operation Information"
  Write-NeoLog "[ General Configuration ]" $Global:NeoLogStyles.Heading2Colour
  Write-NeoLogs (Format-NeoKeyValues ([ordered]@{
        "• Project Prefix"         = $Project
        "• Host Environment Type"  = $($sqlType -eq "shared") ? "Shared" : "Dedicated"
        "• Host KV (SQL Secrets)"  = $sourceHostVaultName
        "• Host KV Resource Group" = $sourceHostVaultResourceGroup
        "• Verbose Logging"        = ($VerbosePreference -eq "Continue") ? "Enabled" : "Disabled"
        "• Debug Logging"          = ($DebugPreference -eq "Continue") ? "Enabled" : "Disabled"
      })
  )  $Global:NeoLogStyles.EmphasisColour

  Write-NeoLog "`n[ Source Configuration ]" $Global:NeoLogStyles.Heading2Colour
  Write-NeoLogs (Format-NeoKeyValues ([ordered]@{
        "• Location"                = $SourceLocation
        "• Environment"             = $SourceEnvironment
        "• Subscription Id"         = $sourceSubscriptionId
        "• Resource Group"          = $sourceResourceGroup
        "• App KV (Client Secrets)" = $sourceVaultName
      })
  )  $Global:NeoLogStyles.EmphasisColour

  Write-NeoLog "`n[ Target Configuration ]" $Global:NeoLogStyles.Heading2Colour
  Write-NeoLogs (Format-NeoKeyValues ([ordered]@{
        "• Location"                = $TargetLocation
        "• Environment"             = $TargetEnvironment
        "• Subscription Id"         = $targetSubscriptionId
        "• Resource Group"          = $targetResourceGroup
        "• App KV (Client Secrets)" = $targetVaultName
      })
  )  $Global:NeoLogStyles.EmphasisColour
  Write-NeoLogHeadingFooter

  if ([string]::IsNullOrEmpty($sourceHostVaultName)) { throw "No Source Host Key Vault found for SQL Server type '$sqlType' within source App Space State" }

  Write-NeoLog "Copying secrets from Source Vault '$sourceHostVaultName' to Target Vault '$targetVaultName'" $NeoLogStyles.EmphasisColour
  Copy-NeoAzSecret -SourceVaultName $sourceHostVaultName `
    -SourceSecretName "vm-sql-$($sqlKey)-app-username" `
    -TargetVaultName $targetVaultName `
    -TargetSecretName "KeyVault--SqlAppUsername"

  Copy-NeoAzSecret -SourceVaultName $sourceHostVaultName `
    -SourceSecretName "vm-sql-$($sqlKey)-app-password" `
    -TargetVaultName $targetVaultName `
    -TargetSecretName "KeyVault--SqlAppPassword"

  Write-NeoLog "Copying secrets from '$sourceVaultName' to '$targetVaultName'" $NeoLogStyles.EmphasisColour

  # Get the source secrets, excluding those associated with certificates
  $sourceSecrets = Get-AzKeyVaultSecret -VaultName $sourceVaultName |
    Where-Object { [string]::IsNullOrEmpty($_.ContentType) -or $_.ContentType -notmatch "application/x-pkcs12|application/x-pem-file" }

  foreach ($sourceSecret in $sourceSecrets) {

    # Check if this secret should be copied between app spaces
    $secretProperties = $sourceAppSpaceState.key_vault.secret_properties.$($sourceSecret.Name)
    if ($null -eq $secretProperties) {
      Write-NeoLog "Skipping secret '$($sourceSecret.Name)' because no corresponding properties were found in the source app space state" $NeoLogStyles.WarningColour
      continue
    }

    if (-not $secretProperties.allow_copy) {
      Write-NeoLog "Skipping secret '$($sourceSecret.Name)' because it's allow_copy property is set to false" $NeoLogStyles.WarningColour
      continue
    }

    Copy-NeoAzSecret -SourceVaultName $sourceVaultName `
      -SourceSecretName $sourceSecret.Name `
      -TargetVaultName $targetVaultName `
      -TargetSecretName $sourceSecret.Name
  }
  Write-NeoLog "`nDONE - All Key Vault Secrets have been copied (Time elapsed: $($stopWatch.Elapsed.ToString("hh\:mm\:ss")))" $Global:NeoLogStyles.SuccessColour

} catch {
  Write-NeoLog "ERROR: Key Vault Secret copy operation failed." $NeoLogStyles.ErrorColour
  throw $_
} finally {
  $stopWatch.Stop()
  Write-Verbose "Total script execution time: $($stopWatch.Elapsed.ToString("hh\:mm\:ss"))"
  $Global:Context.Cleanup()
}

