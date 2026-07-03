# This is required to import any module classes or enums referenced by this script.
using module Neo.PS.Azure
using module Neo.PS.Core

<#
.NOTES
  Please note that this script relies on NeoPs version 1.2.124 or higher.
.SYNOPSIS
  This script manages the enabling/disabling of backups for a SQL VM in Azure using a Recovery Services Vault (RSV).
  
  Enabling RSV Backups performs the following actions:
    1. Registers the SQL VM against RSV Backup Infrastructure and enables a Backup Container.
    2. Enables auto-protection on the SQL instance (and applies the provided backup policy).
  
  Disabling RSV Backups performs the following actions:
    1. Disables the SoftDelete feature on the RSV.
    2. Disables auto-protection on the SQL instance.
    3. Un-registers the backup container.
.PARAMETER Project
  The project to use for resource naming.
.PARAMETER Location
  The Azure location where the SQL VM and Recovery Services Vault are located.
.PARAMETER Environment
  The environment in which the SQL VM and Recovery Services Vault are located.
.PARAMETER EnableRsvBackups
  Enable RSV backups for the SQL VM. This will register the VM and enable backups for all databases.
.PARAMETER DisableRsvBackups
  Disable RSV backups for the SQL VM. This will unregister the VM and disable backups for all databases.
.PARAMETER EnableAutoProtect
  Enable auto-protect for the SQL VM. This will periodically scan the SQL instance for new databases and perform backups based on the provided backup policy.
.PARAMETER PurgeBackups
  If set, this will purge backup items instead of soft deleting them. Use with caution as this action is irreversible.
  This parameter is only applicable when disabling RSV backups.
.PARAMETER SyncRsvAutoProtection
  If set, this will ensure that the auto-protection state is enabled.
  This is useful for ensuring that the desired state is enforced without making changes to the backup registration.
.PARAMETER ReregisterRsvContainer
  If set, this will re-register the RSV container for the SQL VM. This is useful if the container has been unregistered outside of this script.
#>
[CmdletBinding()]
Param(
  [Parameter(Mandatory)][string]$Project,
  [Parameter(Mandatory)][string]$Location = $Env:OperationsAzureRsvLocation,
  [Parameter(Mandatory)][string]$Environment = $Env:OperationsAzureRsvEnvironment,
  [Parameter()][switch]$EnableRsvBackups = ![string]::IsNullOrEmpty($Env:OperationsAzureRsvEnableBackups) ? [System.Convert]::ToBoolean($Env:OperationsAzureRsvEnableBackups) : $false,
  [Parameter()][switch]$DisableRsvBackups = ![string]::IsNullOrEmpty($Env:OperationsAzureRsvDisableBackups) ? [System.Convert]::ToBoolean($Env:OperationsAzureRsvDisableBackups) : $false,
  [Parameter()][switch]$EnableAutoProtect = ![string]::IsNullOrEmpty($Env:OperationsAzureRsvEnableAutoProtection) ? [System.Convert]::ToBoolean($Env:OperationsAzureRsvEnableAutoProtection) : $false,
  [Parameter()][switch]$PurgeBackups = ![string]::IsNullOrEmpty($Env:OperationsAzurePurgeBackups) ? [System.Convert]::ToBoolean($Env:OperationsAzurePurgeBackups) : $false,
  [Parameter()][switch]$SyncRsvAutoProtection = ![string]::IsNullOrEmpty($Env:OperationsAzureSyncRsvAutoProtection) ? [System.Convert]::ToBoolean($Env:OperationsAzureSyncRsvAutoProtection) : $false,
  [Parameter()][switch]$ReregisterRsvContainer = ![string]::IsNullOrEmpty($Env:OperationsAzureReregisterRsvContainer) ? [System.Convert]::ToBoolean($Env:OperationsAzureReregisterRsvContainer) : $false
)

try {
  $ErrorActionPreference = "Stop"
  $InformationPreference = "Continue"
  $VerbosePreference = ($PSBoundParameters["Verbose"] -or [System.Convert]::ToBoolean($Env:NeoVerboseLogging)) ? "Continue" : "SilentlyContinue"
  $DebugPreference = ($PSBoundParameters["Debug"] -or [System.Convert]::ToBoolean($Env:NeoDebugLogging)) ? "Continue" : "SilentlyContinue"
  $VerboseEnabled = $VerbosePreference -eq "Continue"

  Write-NeoLogs @(
    "",
    " █▀▄ █▀█ █▀▀ █ █ █ █ █▀█   █▀▄▀█ █▀█ █▀█ █▀█ █▀▀ █▀▀ █▀▄▀█ █▀▀ █▀█ ▀█▀",
    " █▀▄ █▀█ █   █▀▄ █ █ █▀▀   █ ▀ █ █▀█ █ █ █▀█ █ █ █▀▀ █ ▀ █ █▀▀ █ █  █ ",
    " ▀▀  ▀ ▀ ▀▀▀ ▀ ▀ ▀▀▀ ▀     ▀   ▀ ▀ ▀ ▀ ▀ ▀ ▀ ▀▀▀ ▀▀▀ ▀   ▀ ▀▀▀ ▀ ▀  ▀ ",
    ""
  ) $Global:NeoLogStyles.Heading1Colour

  # Exactly one of EnableRsvBackups or DisableRsvBackups must be specified
  if (-not ($EnableRsvBackups.IsPresent -xor $DisableRsvBackups.IsPresent)) {
    throw "You must specify exactly one of -EnableRsvBackups or -DisableRsvBackups."
  }
  
  $stopWatch = [System.Diagnostics.Stopwatch]::StartNew()
  # Locate the scripts root path and load libraries
  $scriptsPath = $PSScriptRoot; while ($scriptsPath -and (Split-Path $scriptsPath -Leaf) -ne "scripts") { $scriptsPath = Split-Path $scriptsPath }
  if (!$scriptsPath) { throw "Could not locate the scripts root folder. Please ensure this script is nested beneath a 'scripts' parent folder." }
  . (Join-Path $scriptsPath "libraries/common.ps1")

  # This causes the $Global:Context to be reset for the script run's session.
  [PSCustomObject]$context = Get-GlobalContext -New
  
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

  # Configure state context if a script is present.
  $stateContextScript = "$scriptsPath/context/configure-state-context.ps1"
  if (Test-Path $stateContextScript) {
    & $stateContextScript -ProjectPrefix $Project -LocationPrefix $Location -Environment $Environment
  }

  # Initialize backup context and build configurations using fluent API
  [RsvBackupContext]$backupContext = [RsvBackupContext]::new()
  $backupContext.Initialize($context.State, $Environment).BuildConfigurations().ValidateConfigurations() | Out-Null

  Write-NeoLogHeading "Configuration Overview"
  Write-NeoLogs (Format-NeoKeyValues ([ordered]@{
        "Project Prefix"           = $Project
        "Location"                 = $Location
        "Environment"              = $Environment
        "Server Count"             = $backupContext.GetServerCount()
        "Servers"                  = $backupContext.GetServerKeysString()
        "Enable AutoProtect"       = $EnableAutoProtect.IsPresent ? "Yes" : "No"
        "Enable RSV Backups"       = $EnableRsvBackups.IsPresent ? "Yes" : "No"
        "Disable RSV Backups"      = $DisableRsvBackups.IsPresent ? "Yes" : "No"
        "Purge Backups"            = $PurgeBackups.IsPresent ? "Yes" : "No"
        "Sync RSV AutoProtection"  = $SyncRsvAutoProtection.IsPresent ? "Yes" : "No"
        "Reregister RSV Container" = $ReregisterRsvContainer.IsPresent ? "Yes" : "No"
        "Verbose Logging"          = $VerboseEnabled ? "Enabled" : "Disabled"
      })
  ) $Global:NeoLogStyles.EmphasisColour

  Write-NeoLogs ($backupContext.GetServerMatrix() | Format-Table -AutoSize | Out-String) $Global:NeoLogStyles.EmphasisColour
  Write-NeoLogHeadingFooter

  $serverKeys = $backupContext.ValidServers | Sort-Object

  # Process each SQL server
  foreach ($sqlKey in $serverKeys) {
    [RsvBackupConfig]$config = $backupContext.GetConfig($sqlKey)
    
    Write-NeoLog "Processing SQL Server '$sqlKey'" $Global:NeoLogStyles.EmphasisColour
    
    $vault = Get-AzRecoveryServicesVault -ResourceGroupName $config.ResourceGroup -Name $config.VaultName
    if ($null -eq $vault) { 
      Write-NeoLog "WARNING: Vault '$($config.VaultName)' not found under resource group '$($config.ResourceGroup)'. Skipping this server." $Global:NeoLogStyles.WarningColour
      continue
    }

    $backupOperationTimer = [System.Diagnostics.Stopwatch]::StartNew()
    if ($EnableRsvBackups.IsPresent) {
      Write-NeoLog "`nEnabling SQL VM Backups to Recovery Services Vault" $Global:NeoLogStyles.Heading2Colour
      Enable-NeoAzRecoveryServicesVaultBackups `
        -Vault $vault `
        -ResourceGroup $config.ResourceGroup `
        -SqlVmName $config.SqlVmName `
        -BackupPolicyName $config.BackupPolicyName `
        -SqlServerType $config.SqlType `
        -DatabasePrefix $config.DatabasePrefix `
        -EnableAutoProtect:$EnableAutoProtect.IsPresent `
        -SyncRsvAutoProtection:$SyncRsvAutoProtection.IsPresent `
        -ReregisterRsvContainer:$ReregisterRsvContainer.IsPresent `
        -VaultSoftDeleteState $config.SoftDeleteEnabled `
        -Verbose:$VerboseEnabled
    }

    if ($DisableRsvBackups.IsPresent) {
      Write-NeoLog "`nDisabling SQL VM Backups to Recovery Services Vault" $Global:NeoLogStyles.Heading2Colour
      Disable-NeoAzRecoveryServicesVaultBackups `
        -Vault $vault `
        -ResourceGroup $config.ResourceGroup `
        -SqlVmName $config.SqlVmName `
        -SqlServerType $config.SqlType `
        -DatabasePrefix $config.DatabasePrefix `
        -PurgeBackups:$PurgeBackups.IsPresent `
        -VaultSoftDeleteState $config.SoftDeleteEnabled `
        -Verbose:$VerboseEnabled
    }
    Write-NeoLog "`nCompleted processing of SQL Server '$sqlKey' (Time elapsed: $($backupOperationTimer.Elapsed.ToString("hh\:mm\:ss")))" $Global:NeoLogStyles.SuccessColour
    $backupOperationTimer.Stop()
  }

  Write-NeoLog "`nDONE - SQL Server RSV Backup Management Complete (Time elapsed: $($stopWatch.Elapsed.ToString("hh\:mm\:ss")))" $Global:NeoLogStyles.SuccessColour
} catch {
  Write-NeoLog "ERROR: SQL Server RSV Backup Management operation failed." $Global:NeoLogStyles.ErrorColour
  throw $_
} finally {
  if ($stopWatch) {
    $stopWatch.Stop()
  }
  # If an exception occurs in the above main loop body before reaching ($backupOperationTimer.Stop()), the stopwatch will remain running until the script exits, so we ensure it is stopped here.
  if ($backupOperationTimer -and $backupOperationTimer.IsRunning) { $backupOperationTimer.Stop() }
  if ($stopWatch) {
    Write-Verbose "Total script execution time: $($stopWatch.Elapsed.ToString("hh\:mm\:ss"))"
  }
  if ($Global:Context -and $Global:Context.PSObject.Methods.Name -contains 'Cleanup') {
    $Global:Context.Cleanup()
  }
}

