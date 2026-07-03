# This is required to import any module classes or enums referenced by this script.
using module Neo.PS.Azure
using module Neo.PS.Core

<#
.SYNOPSIS
  Restore a set of databases with a common prefix. Supports restoring to the same SQL instance, or another server.
  To restore within the same region, both SQL Server VMs should be registered against a Recovery Services Vault. (Note: You cannot register the same machine in more than one RSV)
  If restoring cross-region, each SQL Server VM should be registered against a separate Recovery Services Vault in their region.
  In order to allow adding of the SQL App user, the SQL Server must be accessible to this script over the network or internet.
.PARAMETER Project
  The project prefix, used in name generation
.PARAMETER SourceLocation
  The Azure SourceLocation that the resources resides in
.PARAMETER SourceEnvironment
  The environment prefix, used in naming the resource
.PARAMETER SourceDbPrefix
  The prefix of the source databases (Used to locate DBs to restore on the source server)
.PARAMETER TargetLocation
  The Azure TargetLocation that the resources will be restored to
.PARAMETER TargetEnvironment
  The environment prefix, used in naming the resource
.PARAMETER TargetDbPrefix
  The prefix of the target databases (The source prefix will be replaced with this)
.PARAMETER TargetSqlHostType
  Used to control how the SQL Host is connected to. Options are:
    public-ip        : Use the public IP linked to the Azure VM (Assumes the public IP follows the same naming convention as the VM resource. It will replace "vm" with "pip" in the name)
    public-hostname  : Use the FQDN on the public IP linked to the Azure VM (Assumes the public IP follows the same naming convention as the VM resource. It will replace "vm" with "pip" in the name)
    private-ip       : Use the private IP on the Azure VM resource
    private-hostname : Use the computer name on the Azure VM resource
    auto             : The auto option will use the public IP address if one exists. Otherwise it defaults to the private IP. (Note that this behaviour is not suitable for all scenarios. E.g. If you run the script inside the private network, rather use the "private-ip" option)
.PARAMETER TargetPointInTime
  The target point in time to restore backups from. The closest restore points to this will be selected for each database.
  If not provided, the current date and time is used.
.PARAMETER ExcludedDatabases
  The names of databases to exclude from restore (SourceDbPrefix is added to these names). This should be a comma-separated list in a string or a string array.
.PARAMETER FromSqlKey
  The source SQL server key for targeted restores (e.g., 'sql01'). When provided with ToSqlKey, creates a single explicit restore pair.
  Required when SourceEnvironment equals TargetEnvironment. Can also be used for cross-environment, cross-region (CRR), and cross-subscription (CSR) restores.
.PARAMETER ToSqlKey
  The target SQL server key for targeted restores (e.g., 'sql02'). When provided with FromSqlKey, creates a single explicit restore pair.
  Required when SourceEnvironment equals TargetEnvironment. Can also be used for cross-environment, cross-region (CRR), and cross-subscription (CSR) restores.
.PARAMETER TargetVaultResourceGroupOverride
  If the target Recovery Services Vault is in a different resource group to the SQL VM, specify it here.
.PARAMETER AutoApprove
  Automatically approves the restore plan. For use in automation scenarios.
.PARAMETER OverwriteExisting
  If the databases already exist, should they be overwritten?
.PARAMETER SkipDatabaseRekeyProcess
  If set, this will skip the process of rekeying Always Encrypted databases after restore. Use with caution as this may leave the database in an unusable state if the target SQL Server does not have access to the correct Column Master Key.
.PARAMETER PreserveContext
  If specified, the global context will be preserved after script execution for reuse in
  subsequent scripts. This can speed up local development/testing by avoiding repeated
  authentication and discovery calls. However, preserving context between runs may lead to
  stale or inconsistent data if the underlying Azure/infrastructure state changes (for example,
  if resources are added, removed, or modified while reusing a saved context). This switch is
  intended primarily for local development and testing scenarios and is generally not
  recommended for long-running or production automation. (Optional, defaults to false)
#>
[CmdletBinding()]
param(
  [Parameter(Mandatory)][string]$Project,
  [Parameter(Mandatory)][string]$SourceLocation,
  [Parameter(Mandatory)][string]$SourceEnvironment,
  [Parameter()][string]$SourceDbPrefix,
  [Parameter(Mandatory)][string]$TargetLocation,
  [Parameter(Mandatory)][string]$TargetEnvironment,
  [Parameter()][string]$TargetDbPrefix,
  [Parameter()][ValidateSet("public-ip", "public-hostname", "private-ip", "private-hostname", "auto")][string]$TargetSqlHostType = "auto",
  [Parameter()][Nullable[DateTime]]$TargetPointInTime = $null,
  [Parameter()][string]$ExcludedDatabases = $null,
  [Parameter()][string]$FromSqlKey,
  [Parameter()][string]$ToSqlKey,
  [Parameter()][string]$TargetVaultResourceGroupOverride,
  [Parameter()][switch]$AutoApprove,
  [Parameter()][switch]$OverwriteExisting,
  [Parameter()][switch]$SkipDatabaseRekeyProcess,
  [Parameter()][switch]$PreserveContext
)

try {
  $ErrorActionPreference = "Stop"
  $InformationPreference = "Continue"
  $VerbosePreference = ($PSBoundParameters["Verbose"] -or [System.Convert]::ToBoolean($Env:NeoVerboseLogging)) ? "Continue" : "SilentlyContinue"
  $DebugPreference = ($PSBoundParameters["Debug"] -or [System.Convert]::ToBoolean($Env:NeoDebugLogging)) ? "Continue" : "SilentlyContinue"
  $VerboseEnabled = $VerbosePreference -eq "Continue"
  
  Write-NeoLogs @(
    "",
    " █▀▄ █▀█ ▀█▀ █▀█ █▀▄ █▀█ █▀▀ █▀▀   █▀▄ █▀▀ █▀▀ ▀█▀ █▀█ █▀▄ █▀▀",
    " █ █ █▀█  █  █▀█ █▀▄ █▀█ ▀▀█ █▀▀   █▀▄ █▀▀ ▀▀█  █  █ █ █▀▄ █▀▀",
    " ▀▀  ▀ ▀  ▀  ▀ ▀ ▀▀  ▀ ▀ ▀▀▀ ▀▀▀   ▀ ▀ ▀▀▀ ▀▀▀  ▀  ▀▀▀ ▀ ▀ ▀▀▀",
    ""
  ) $Global:NeoLogStyles.Heading1Colour

  # Locate the scripts root path and load libraries
  $scriptsPath = $PSScriptRoot; while ($scriptsPath -and (Split-Path $scriptsPath -Leaf) -ne "scripts") { $scriptsPath = Split-Path $scriptsPath }
  if (!$scriptsPath) { throw "Could not locate the scripts root folder. Please ensure this script is nested beneath a 'scripts' parent folder." }
  . (Join-Path $scriptsPath "libraries/common.ps1")

  [PSCustomObject]$context = Get-GlobalContext -New:$(!$PreserveContext)
  
  # Decide if the Source/Target environment(s) need (re)configuration
  # - If PreserveContext is disabled, always reconfigure (fresh discovery)
  # - If there is no existing SourceEnvironment, reconfigure to establish context
  # - Otherwise, reconfigure only if any relevant input differs from saved context
  $sourceChanged =
  (!$PreserveContext) -or
  ($null -eq $context.SourceEnvironment) -or
  (
    $context.SourceEnvironment.Project -ne $Project -or
    $context.SourceEnvironment.Location -ne $SourceLocation -or
    $context.SourceEnvironment.Environment -ne $SourceEnvironment -or
    $context.SourceEnvironment.DbPrefix -ne $SourceDbPrefix -or
    (-not (Test-NeoArrayEquality $context.SourceEnvironment.ExcludedDatabases $ExcludedDatabases))
  )

  $targetChanged =
  (!$PreserveContext) -or
  ($null -eq $context.TargetEnvironment) -or
  (
    $context.TargetEnvironment.Project -ne $Project -or
    $context.TargetEnvironment.Location -ne $TargetLocation -or
    $context.TargetEnvironment.Environment -ne $TargetEnvironment -or
    $context.TargetEnvironment.DbPrefix -ne $TargetDbPrefix -or
    (-not (Test-NeoArrayEquality $context.TargetEnvironment.ExcludedDatabases $ExcludedDatabases))
  )

  # Configure runtime and cloud contexts only if either environment changed
  if ($sourceChanged -or $targetChanged) {
    Write-NeoLog "Source or Target state configuration has changed, configuring Runtime and Cloud Contexts" $Global:NeoLogStyles.SuppressedColour
    $contextScript = "$scriptsPath/context/configure-runtime-context.ps1"
    if (Test-Path $contextScript) {
      & $contextScript
    }

    $cloudContextScript = "$scriptsPath/context/configure-cloud-context.ps1"
    if (Test-Path $cloudContextScript) {
      & $cloudContextScript -ConnectPowerShellAz
    }
  }

  # Configure state context scripts
  $stateContextScript = "$scriptsPath/context/configure-state-context.ps1"

  # Configure source environment if it changed
  if ($sourceChanged) {
    Write-NeoLog "`nSource environment changed. Configuring source context." $Global:NeoLogStyles.EmphasisColour
  
    if ($null -ne $context.SourceEnvironment) {
      $context.PSObject.Properties.Remove("SourceEnvironment")
      $context.PSObject.Properties.Remove("SourceState")
    }
  
    $context.Add("SourceEnvironment", (New-Context @{
          Project           = $Project
          Location          = $SourceLocation
          Environment       = $SourceEnvironment
          DbPrefix          = $SourceDbPrefix
          ExcludedDatabases = $ExcludedDatabases
        })
    )

    & $stateContextScript -StateKey "SourceState" -ProjectPrefix $Project -LocationPrefix $SourceLocation -Environment $SourceEnvironment
  } else {
    Write-NeoLog "Source environment unchanged. Reusing context." $Global:NeoLogStyles.SuppressedColour
  }

  # Configure target environment if it changed
  if ($targetChanged) {
    Write-NeoLog "`nTarget environment changed. Configuring target context." $Global:NeoLogStyles.EmphasisColour
    
    if ($null -ne $context.TargetEnvironment) {
      $context.PSObject.Properties.Remove("TargetEnvironment")
      $context.PSObject.Properties.Remove("TargetState")
    }
    
    $context.Add("TargetEnvironment", (New-Context @{
          Project           = $Project
          Location          = $TargetLocation
          Environment       = $TargetEnvironment
          DbPrefix          = $TargetDbPrefix
          ExcludedDatabases = $ExcludedDatabases
        })
    )

    & $stateContextScript -StateKey "TargetState" -ProjectPrefix $Project -LocationPrefix $TargetLocation -Environment $TargetEnvironment
  } else {
    Write-NeoLog "Target environment unchanged. Reusing context." $Global:NeoLogStyles.SuppressedColour
  }
  
  # State Objects (Source & Target)
  [PSCustomObject]$sourceContext = $context.SourceState
  [PSCustomObject]$targetContext = $context.TargetState

  # Initialize restore context from state objects
  # Note: Partial key validation (one of FromSqlKey/ToSqlKey without the other) and
  # same-environment validation are handled by RsvRestoreContext.Initialize()
  $restoreContext = [RsvRestoreContext]::new()
  $restoreContext.Initialize($sourceContext, $targetContext, $SourceEnvironment, $TargetEnvironment, $FromSqlKey, $ToSqlKey) | Out-Null

  $crossRegionValidation = $restoreContext.ValidateCrossRegionConstraints($SourceLocation, $TargetLocation)
  if (-not $crossRegionValidation.IsValid) {
    throw $crossRegionValidation.Errors[0]
  }

  $subscriptionId = $restoreContext.SubscriptionId

  # Report missing SQL servers
  $missingSqlServers = $restoreContext.GetMissingSqlServers()
  if ($missingSqlServers.Count -gt 0) {
    Write-NeoLog "`nWARNING: The following SQL servers exist in the source but not in the target state:" $Global:NeoLogStyles.WarningColour
    foreach ($missingSql in $missingSqlServers) {
      Write-NeoLog "  - $missingSql" $Global:NeoLogStyles.WarningColour
    }
    Write-NeoLog "These SQL server(s) will be skipped during the restore process.`n" $Global:NeoLogStyles.WarningColour
  }

  # Build restore pairs, configurations, and validate
  $restoreContext.BuildRestorePairs() | Out-Null
  $restoreContext.BuildConfigurations() | Out-Null
  $restoreContext.AddExcludedDatabases($ExcludedDatabases)
  $restoreContext.GetValidatedPairs() | Out-Null

  $overwriteCount = $restoreContext.GetOverwriteCount()
  $pairDescriptions = $restoreContext.GetPairDescriptions()
  # Check if using targeted restore (FromSqlKey/ToSqlKey) vs matching keys restore
  $isTargetedRestore = $restoreContext.IsTargetedRestore()

  Write-NeoLogHeading "Configuration Overview"
  Write-NeoLogs (Format-NeoKeyValues ([ordered]@{
        "Subscription"              = $subscriptionId
        "Restore Mode"              = ($isTargetedRestore ? "Targeted (FromSqlKey→ToSqlKey)" : "Matching State Keys")
        "Restore Pairs"             = ($pairDescriptions -join ", ")
        "Pair Count"                = $restoreContext.RestorePairs.Count
        "Auto Approve"              = ($AutoApprove.IsPresent ? "Yes" : "No")
        "Overwrite Enabled (count)" = $overwriteCount
      })
  ) $Global:NeoLogStyles.EmphasisColour

  $serverMatrix = $restoreContext.RestorePairs | ForEach-Object {
    [RsvRestoreConfig]$sourceConfig = $restoreContext.GetSourceConfig($_)
    [RsvRestoreConfig]$targetConfig = $restoreContext.GetTargetConfig($_)

    [PSCustomObject]@{
      "Source → Target" = "$($_.SourceSqlKey) → $($_.TargetSqlKey)"
      "Source Vault"    = $sourceConfig.VaultName
      "Target Vault"    = $targetConfig.VaultName
      "Target VM"       = $targetConfig.SqlVmName
      "Host Type"       = $targetConfig.SqlHostType
      "DB Prefix (S→T)" = "$($sourceConfig.DatabasePrefix) → $($targetConfig.DatabasePrefix)"
      "Overwrite"       = ($targetConfig.OverwriteExisting ? "Yes" : "No")
      "Excluded Count"  = $targetConfig.ExcludedDatabases.Count
    }
  }

  Write-NeoLogs ($serverMatrix | Format-Table -AutoSize | Out-String) $Global:NeoLogStyles.EmphasisColour
  Write-NeoLogHeadingFooter

  # Loop over each restore pair and execute restore
  foreach ($pair in $restoreContext.RestorePairs) {
    $sourceKey = $pair.SourceSqlKey
    $targetKey = $pair.TargetSqlKey
    [RsvRestoreConfig]$sourceConfig = $restoreContext.GetSourceConfig($pair)
    [RsvRestoreConfig]$targetConfig = $restoreContext.GetTargetConfig($pair)

    # Use model methods to resolve overwrite and excluded databases
    # Pass $null when -OverwriteExisting wasn't specified to use config value
    $overwriteExisting = $targetConfig.ResolveOverwriteExisting(($PSBoundParameters.ContainsKey('OverwriteExisting') ? $OverwriteExisting.IsPresent : $null))
    $excludedDatabasesList = $targetConfig.ExcludedDatabases
    # For targeted restores, provide source SQL VM name to distinguish from multiple SQL instance's databases being registered on the same RSV.
    $sourceSqlVmName = $isTargetedRestore ? $sourceConfig.SqlVmName : $null

    Write-NeoLog "Processing restore: $sourceKey → $targetKey" $Global:NeoLogStyles.EmphasisColour
    # Note: SqlKey parameter represents the target SQL key and is used to lookup the target SQL admin password.
    # For targeted restores (same-environment), SourceSqlVmName filters databases by source server name.
    Restore-NeoAzRsvSqlDatabases `
      -Project $Project `
      -SubscriptionId $subscriptionId `
      -SqlKey $targetKey `
      -SourceResourceGroup $sourceConfig.VaultResourceGroup `
      -SourceEnvironment $SourceEnvironment `
      -SourceVaultName $sourceConfig.VaultName `
      -SourceDbPrefix $sourceConfig.DatabasePrefix `
      -SourceKeyVaultName $sourceConfig.AppKeyVaultName `
      -SourceSqlVmName $sourceSqlVmName `
      -TargetResourceGroup $targetConfig.VaultResourceGroup `
      -TargetEnvironment $TargetEnvironment `
      -TargetVaultName $targetConfig.VaultName `
      -TargetDbPrefix $targetConfig.DatabasePrefix `
      -TargetKeyVaultName $targetConfig.AppKeyVaultName `
      -TargetSqlVmName $targetConfig.SqlVmName `
      -TargetSqlAdminUser $targetConfig.SqlAdminUser `
      -TargetSqlAppUser $targetConfig.SqlAppUser `
      -TargetSqlHostType $targetConfig.SqlHostType `
      -TargetVaultResourceGroupOverride $TargetVaultResourceGroupOverride `
      -TargetPointInTime $TargetPointInTime `
      -ExcludedDatabases $excludedDatabasesList `
      -AutoApprove $AutoApprove.IsPresent `
      -OverwriteExisting $overwriteExisting `
      -SkipDatabaseRekeyProcess:$SkipDatabaseRekeyProcess.IsPresent `
      -InformationAction Continue `
      -Verbose:$VerboseEnabled
          
  }
} catch {
  Write-NeoLog "ERROR: The Database Restore operation(s) failed." $Global:NeoLogStyles.ErrorColour
  throw $_
} finally {
  # Ensure the global context cleanup is run before exiting
  if (!$PreserveContext) {
    Write-NeoLog "Cleaning up Context" $Global:NeoLogStyles.SuppressedColour
    $Global:Context.Cleanup()
  } else {
    Write-NeoLog "Preserve Context enabled. Skipping cleanup." $Global:NeoLogStyles.WarningColour
  }

  # Clear secrets for all target configs (source configs don't load secrets)
  foreach ($config in $restoreContext.TargetConfigs.Values) {
    if ($null -ne $config) { $config.ClearAdminSecret() }
  }
}

