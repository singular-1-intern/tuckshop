<#
.SYNOPSIS
  This script clears the contents of an Azure Blob Storage Account by removing all of it's containers.
.PARAMETER Project
  The project prefix. (Required)
.PARAMETER SourceLocation
  The Azure SourceLocation that the storage account(s) reside(s) within. (Required)
.PARAMETER TargetLocation
  The Azure TargetLocation that the storage account(s), to restore to, reside(s) within. (Required)
.PARAMETER TargetEnvironment
  The target environment prefix, used in finding the target storage account to copy to. (Required)
.PARAMETER SourceEnvironment
  The source environment prefix, used in finding the source storage account to copy from. (Required)
.PARAMETER ExcludedStorageAccounts
  A comma-separated list of storage account names to exclude from the copy operation. (Optional; Default to $null)
.PARAMETER OverwriteExisting
  Overwrites the existing data in the target storage account. (Optional; Defaults to $false)
.PARAMETER ForceCopy
  Forces the copy operation to proceed, even if the copy operation is not allowed in the automation state. (Optional)
.PARAMETER AutoApprove
  Automatically approves the copy operation. For use in automation scenarios. (Optional)
#>
[CmdletBinding()]
param(
  [Parameter(Mandatory)][string]$Project,
  [Parameter(Mandatory)][string]$SourceLocation,
  [Parameter(Mandatory)][string]$SourceEnvironment,
  [Parameter(Mandatory)][string]$TargetLocation,
  [Parameter(Mandatory)][string]$TargetEnvironment,
  [Parameter()][string]$ExcludedStorageAccounts = $null,
  [Parameter()][switch]$OverwriteExisting,
  [Parameter()][switch]$ForceCopy,
  [Parameter()][switch]$AutoApprove
)

function Get-StorageAccountsFromAppSpaceState {
  [OutputType([hashtable])]
  [CmdletBinding()]
  Param(
    [PSObject]$AutomationState
  )

  if ($null -eq $AutomationState -or -not $AutomationState.PSObject.Properties.Name -contains "storage_accounts") {
    throw "'AutomationState' state for $Project in region $Location does not contain 'storage_accounts'. Ensure the state context was configured for the target environment."
  }

  $storageAccounts = @{}
  $AutomationState.storage_accounts.PSObject.Properties | ForEach-Object {
    $suffix = $_.Name
    # If the current Storage Account's suffix is excluded, skip it.
    if (($ExcludedStorageAccounts ?? "").Split(",") -contains $suffix) { return }
    $blobAccount = $_.Value
    $storageAccounts[$suffix] = @{
      Name           = $blobAccount.name
      Suffix         = $suffix
      Id             = $blobAccount.id
      CopyOperations = $blobAccount.copy_operations
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

  # Source configurations
  $sourceAppSpaceState = $sourceContext.AppSpace
  $sourceSubscriptionId = $sourceAppSpaceState.azure.subscription_id
  $sourceResourceGroup = $sourceAppSpaceState.resource_group.name
  $sourceBlobStorageAccounts = Get-StorageAccountsFromAppSpaceState -AutomationState $sourceAppSpaceState

  # Target configurations
  $targetAppSpaceState = $targetContext.AppSpace
  $targetSubscriptionId = $targetAppSpaceState.azure.subscription_id
  $targetResourceGroup = $targetAppSpaceState.resource_group.name
  $targetBlobStorageAccounts = Get-StorageAccountsFromAppSpaceState -AutomationState $targetAppSpaceState

  Write-NeoLogHeading "Blob Storage Account Copy Operation Information"
  Write-NeoLog "[ General Configuration ]" $Global:NeoLogStyles.Heading2Colour
  Write-NeoLogs (Format-NeoKeyValues ([ordered]@{
        "• Project Prefix"     = $Project
        "• Overwrite Existing" = $OverwriteExisting.IsPresent ? "Yes" : "No"
        "• Force Copy"         = $ForceCopy.IsPresent ? "Yes" : "No"
        "• Auto Approve"       = $AutoApprove.IsPresent ? "Yes" : "No"
        "• Verbose Logging"    = ($VerbosePreference -eq "Continue") ? "Enabled" : "Disabled"
        "• Debug Logging"      = ($DebugPreference -eq "Continue") ? "Enabled" : "Disabled"
        "• Excluded Suffixes"  = $([string]::IsNullOrEmpty($ExcludedStorageAccounts) ? "None" : $ExcludedStorageAccounts.Split(",") -join ", ")
        "• Included Suffixes"  = $([string]::IsNullOrEmpty($sourceBlobStorageAccounts) ? "None" : $sourceBlobStorageAccounts.Keys -join ", ")
      })
  )  $Global:NeoLogStyles.EmphasisColour
    
  Write-NeoLog "`n[ Source Configuration ]" $Global:NeoLogStyles.Heading2Colour
  Write-NeoLogs (Format-NeoKeyValues ([ordered]@{
        "• Location"        = $SourceLocation
        "• Environment"     = $SourceEnvironment
        "• Subscription Id" = $sourceSubscriptionId
        "• Resource Group"  = $sourceResourceGroup
      })
  )  $Global:NeoLogStyles.EmphasisColour

  Write-NeoLog "`n[ Target Configuration ]" $Global:NeoLogStyles.Heading2Colour
  Write-NeoLogs (Format-NeoKeyValues ([ordered]@{
        "• Location"        = $TargetLocation
        "• Environment"     = $TargetEnvironment
        "• Subscription Id" = $targetSubscriptionId
        "• Resource Group"  = $targetResourceGroup
      })
  )  $Global:NeoLogStyles.EmphasisColour
  Write-NeoLogHeadingFooter

  foreach ($sourceStorageAccount in $sourceBlobStorageAccounts.Values) {
    $targetStorageAccount = $targetBlobStorageAccounts.Values | Where-Object { $_.Name.EndsWith($sourceStorageAccount.Suffix) }
    if ([string]::IsNullOrEmpty($targetStorageAccount.Name)) {
      Write-Verbose "Copy not allowed to non-existing target, verify that the target Storage Account has been provisioned. [Source Account: '$($sourceStorageAccount.Name)']"
      continue
    }

    $shouldCopyProceed = (($sourceStorageAccount.CopyOperations.outbound_enabled -eq $true -and $targetStorageAccount.CopyOperations.allow_inbound_copy -eq $true) -or $ForceCopy.IsPresent)
    if (!$shouldCopyProceed) {
      Write-NeoLog "Copy operation not allowed for '$($targetStorageAccount.Name)', you can force a copy through the command flag -ForceCopy" $Global:NeoLogStyles.WarningColour
      continue
    }

    Write-NeoLog "`nReplicating Blob Storage Account containers (excluding data) from '$($sourceStorageAccount.Name)' to '$($targetStorageAccount.Name)'." $Global:NeoLogStyles.Heading2Colour
    Sync-NeoAzStorageAccountContainers `
      -SourceResourceGroup $sourceResourceGroup `
      -SourceStorageAccountName $sourceStorageAccount.Name `
      -TargetResourceGroup $targetResourceGroup `
      -TargetStorageAccountName $targetStorageAccount.Name

    $targetStorageAccountSasToken = Get-NeoAzStorageSasToken -SubscriptionId $targetSubscriptionId -ResourceGroup $targetResourceGroup -StorageAccount $targetStorageAccount.Name
    $sourceStorageAccountSasToken = Get-NeoAzStorageSasToken -SubscriptionId $sourceSubscriptionId -ResourceGroup $sourceResourceGroup -StorageAccount $sourceStorageAccount.Name
    $sourceStorageAccountRemoteDetails = (az storage account show --resource-group $sourceResourceGroup --name $sourceStorageAccount.Name | ConvertFrom-Json)
    $targetStorageAccountRemoteDetails = (az storage account show --resource-group $targetResourceGroup --name $targetStorageAccount.Name | ConvertFrom-Json)

  
    $targetUrl = "https://$($targetStorageAccount.Name).blob.core.windows.net/?$targetStorageAccountSasToken"
    $sourceUrl = $null
    if ($sourceStorageAccountRemoteDetails.Location -eq $targetStorageAccountRemoteDetails.Location) {
      $sourceUrl = "https://$($sourceStorageAccount.Name).blob.core.windows.net/?$sourceStorageAccountSasToken"
    } else {
      $sourceUrl = "https://$($sourceStorageAccount.Name)-secondary.blob.core.windows.net/?$sourceStorageAccountSasToken"
    }

    $overwriteExisting = ($OverwriteExisting.IsPresent -or $targetStorageAccount.CopyOperations.allow_inbound_overwrite)
    Write-NeoLog "Copying Blob Storage Account data from '$($sourceStorageAccount.Name)' to '$($targetStorageAccount.Name)'.`n" $Global:NeoLogStyles.EmphasisColour
  
    # Need to revise the "azcopy sync" command that needs to run on a container level opposed to blob copy running on service level.
    Copy-NeoAzStorageAccountData `
      -SourceUrl $sourceUrl `
      -TargetUrl $targetUrl `
      -OverwriteExisting:$overwriteExisting `
      -AutoApprove:$AutoApprove.IsPresent
  }
  Write-NeoLog "`nDONE - All Copy Operations Completed (Time elapsed: $($stopWatch.Elapsed.ToString("hh\:mm\:ss")))" $Global:NeoLogStyles.SuccessColour

} catch {
  Write-NeoLog "ERROR: Storage Account copy operation failed." $NeoLogStyles.ErrorColour
  throw $_
} finally {
  $stopWatch.Stop()
  Write-Verbose "Total script execution time: $($stopWatch.Elapsed.ToString("hh\:mm\:ss"))"
  $Global:Context.Cleanup()
}
