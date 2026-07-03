<#
.SYNOPSIS
  This script clears the contents of an Azure Blob Storage Account by removing all of it's containers & blobs.
.PARAMETER Project
  The project prefix. (Required)
.PARAMETER Location
  The Azure Location that the storage account resides within. (Required)
.PARAMETER Environment
  The Azure Environment that the storage account resides within. (Required)
.PARAMETER ExcludedStorageAccounts
  A comma-separated list of storage account names to exclude from the clear operation. (Optional; Default to $null)
.PARAMETER ForceClear
  Forces the clear operation to proceed, even if the clear operation is not allowed in the automation state. (Optional)
.PARAMETER AutoApprove
  Automatically approves the clear operation. For use in automation scenarios. (Optional)
#>
[CmdletBinding()]
param(
  [Parameter(Mandatory)][string]$Project,
  [Parameter(Mandatory)][string]$Location,
  [Parameter(Mandatory)][string]$Environment,
  [Parameter()][string[]]$ExcludedStorageAccounts = $null,
  [Parameter()][switch]$ForceClear,
  [Parameter()][switch]$AutoApprove
)

function Get-StorageAccountsFromAppSpaceState {

  if ($null -eq $context.State.AppSpace -or -not $context.State.AppSpace.PSObject.Properties.Name -contains "storage_accounts") {
    throw "'context.State.AppSpace' state does not contain 'storage_accounts'. Ensure the state context was configured for the target environment."
  }

  $storageAccounts = @{}
  $context.State.AppSpace.storage_accounts.PSObject.Properties | ForEach-Object {
    $suffix = $_.Name
    # If the current Storage Account's suffix is excluded, skip it.
    if (($ExcludedStorageAccounts ?? "").Split(",") -contains $suffix) { return }
    $blobAccount = $_.Value
    $storageAccounts[$suffix] = @{
      Name            = $blobAccount.name
      Suffix          = $suffix
      Id              = $blobAccount.id
      ClearOperations = $blobAccount.clear_operations
    }
  }

  if ($storageAccounts.Count -eq 0) { throw "No Storage Accounts found after applying exclusions." }
  return $storageAccounts
}

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

  $targetBlobStorageAccounts = Get-StorageAccountsFromAppSpaceState
  $resourceGroup = $context.State.AppSpace.resource_group.name

  Write-NeoLogHeading "Blob Storage Account Clear Operation Information"
  Write-NeoLogs (Format-NeoKeyValues ([ordered]@{
        "Project Prefix"    = $Project
        "Location"          = $Location
        "Resource Group"    = $resourceGroup
        "Environment"       = $Environment
        "Excluded Suffixes" = $([string]::IsNullOrEmpty($ExcludedStorageAccounts) ? "None" : $ExcludedStorageAccounts.Split(",") -join ", ")
        "Included Suffixes" = $([string]::IsNullOrEmpty($targetBlobStorageAccounts) ? "None" : $targetBlobStorageAccounts.Keys -join ", ")
        "Force Clear"       = $ForceClear.IsPresent ? "Yes" : "No"
        "Auto Approve"      = $AutoApprove.IsPresent ? "Yes" : "No"
        "Verbose Logging"   = ($VerbosePreference -eq "Continue") ? "Enabled" : "Disabled"
        "Debug Logging"     = ($DebugPreference -eq "Continue") ? "Enabled" : "Disabled"
      })
  )  $Global:NeoLogStyles.EmphasisColour
  Write-NeoLogHeadingFooter


  foreach ($targetStorageAccount in $targetBlobStorageAccounts.Values) {
    if ($targetStorageAccount.ClearOperations.allowed -or $ForceClear.IsPresent) {
      $targetStorageAccountKeys = Get-NeoAzStorageAccountKeys -ResourceGroup $resourceGroup -StorageName $targetStorageAccount.Name
      Clear-NeoAzStorageAccount `
        -StorageAccountName $targetStorageAccount.Name `
        -StorageAccountKey $targetStorageAccountKeys[0].Value `
        -AutoApprove:$AutoApprove.IsPresent

    } else {
      Write-NeoLog "Clear operation not allowed for '$($targetStorageAccount.Name)', you can force a clear through the command flag '-ForceClear'" $Global:NeoLogStyles.WarningColour
    }
  }
  Write-NeoLog "DONE - All Clear Operations Completed (Time elapsed: $($stopWatch.Elapsed.ToString("hh\:mm\:ss")))`n" $Global:NeoLogStyles.SuccessColour

} catch {
  Write-NeoLog "ERROR: Storage Account clear operation failed." $NeoLogStyles.ErrorColour
  throw $_
} finally {
  $stopWatch.Stop()
  Write-Verbose "Total script execution time: $($stopWatch.Elapsed.ToString("hh\:mm\:ss"))"
  $Global:Context.Cleanup()
}
