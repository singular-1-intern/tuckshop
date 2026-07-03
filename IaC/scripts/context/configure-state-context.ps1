# ┌───────────────────────────────────────────────────────────────────────────┐
# │ State Context Library                                                     │
# ├───────────────────────────────────────────────────────────────────────────┤
# │ Dependencies                                                              │
# │   Libraries : common.ps1; azure.ps1; configure-cloud-provider-context.ps1 │
# │   Tools     : Azure PowerShell                                            │
# └───────────────────────────────────────────────────────────────────────────┘
<#
.SYNOPSIS
  Retrieves IaC state from Azure Automation Accounts and adds it to the global context.
  Supports either Singular Cloud or ShareTrust environments.
.PARAMETER ProjectPrefix
  The project prefix, used to generate Azure resource names.
.PARAMETER LocationPrefix
  The location prefix, used to generate Azure resource names.
.PARAMETER HostPrefix
  The prefix of the Shared Hosts Environment (or ShareTrust Host Environment) to retrieve state for.
.PARAMETER Environment
  The prefix of the App Space Environment (or ShareTrust App Environment) to retrieve state for.
  If there is an associated Host Space, its state will also be retrieved.
.PARAMETER StateKey
  The key to use when adding the retrieved state to the global context.
  Defaults to "State".
#>
[CmdletBinding()]
Param(
  [string]$ProjectPrefix = $Env:ProjectPrefix,
  [string]$LocationPrefix = $Env:LocationPrefix,
  [string]$HostPrefix = $Env:HostPrefix,
  [string]$Environment = $Env:EnvironmentPrefix,
  [string]$StateKey = "State"
)

# Locate the scripts root path and load libraries
$scriptsPath = $PSScriptRoot; while ($scriptsPath -and (Split-Path $scriptsPath -Leaf) -ne "scripts") { $scriptsPath = Split-Path $scriptsPath }
if (!$scriptsPath) { throw "Could not locate the scripts root folder. Please ensure this script is nested beneath a 'scripts' parent folder." }
. (Join-Path $scriptsPath "libraries/common.ps1")
. (Join-Path $scriptsPath "libraries/azure.ps1")

Write-Log "`n[ Configure State ]" $Global:LogStyles.Heading1Colour

# Ensure that the parameters which use environment variable defaults actually have values.
if ([string]::IsNullOrEmpty($ProjectPrefix)) { throw "ProjectPrefix is required" }
if ([string]::IsNullOrEmpty($LocationPrefix)) { throw "LocationPrefix is required" }
if ([string]::IsNullOrEmpty($Environment)) { throw "Environment is required" }

# Confirm that the required context is available
$context = Get-GlobalContext
if ($null -eq $context.Azure) {
  throw "Azure context not found. Please run the 'configure-cloud-provider-context.ps1' script to configure the context."
}

function Get-State($StateContainer, $Name, $AzToken, $SubscriptionId, $ResourceGroup, $AutomationAccount, $VariableName) {
  $stateVariable = (Get-AzureAutomationAccountVariable `
      -SubscriptionId $SubscriptionId `
      -ResourceGroup $ResourceGroup `
      -AutomationAccount $AutomationAccount `
      -VariableName $VariableName `
      -AzToken $AzToken `
      -JsonDecode)

  if ($null -ne $stateVariable) {
    Write-Log "$($Name.PadRight(12)): Retrieved State '$VariableName' from Automation Account '$AutomationAccount'"
    $StateContainer[$Name] = $stateVariable
  } else {
    Write-Log "$($Name.PadRight(12)): State not found in Automation Account '$AutomationAccount'"
  }

  return $stateVariable
}

$subscriptionId = $context.Azure.SubscriptionId
$azToken = $context.Azure.Token
$state = @{}

# NOTE: The resource names and available IaC state differs between ShareTrust and Singular Cloud.
#       This will be removed eventually once ShareTrust migrates to the Singular Cloud IaC code base.
$isShareTrust = ($ProjectPrefix -eq "st" -or $ProjectPrefix -eq "stt")
if ($isShareTrust) {
  # ShareTrust IaC State Retrieval
  #   State Keys Reference
  #    > Host Environment : automation-account-state, cluster-main-state, host-state
  #    > App Environment  : app-environment-state

  if ([string]::IsNullOrEmpty($HostPrefix)) { throw "HostPrefix is required" }
  # There is a test environment which uses the 'stt' prefix to avoid conflicting with other ShareTrust environments/resources.
  # This is a minor hack to ensure we still retrieve the Host Environment state correctly.
  $HostProjectPrefix = "st"

  # Host Environment State (SharedHosts)
  $resourcePrefix = "$HostProjectPrefix-$HostPrefix"
  $resourceGroup = "$resourcePrefix-rg-host"
  $automationAccount = "$resourcePrefix-aa-main"
  Get-State $state "SharedHosts" $azToken $subscriptionId $resourceGroup $automationAccount "host-state" | Out-Null
  Get-State $state "Cluster" $azToken $subscriptionId $resourceGroup $automationAccount "cluster-main-state" | Out-Null

  # App Environment State (AppSpace)
  $resourcePrefix = "$ProjectPrefix-$Environment"
  $resourceGroup = "$resourcePrefix-rg"
  $automationAccount = "$resourcePrefix-aa-main-app-space"
  Get-State $state "AppSpace" $azToken $subscriptionId $resourceGroup $automationAccount "app-environment-state" | Out-Null

} else {
  # Singular Cloud IaC State Retrieval
  #   State Keys Reference:
  #    > SharedHosts : automation-account-state, cluster-main-state, host-state
  #    > HostSpace   : host-space-base-state, host-space-state
  #    > AppSpace    : app-space-base-state, app-space-state

  # App Space State
  $resourceGroup = "$ProjectPrefix-$LocationPrefix-$Environment-rg-app_space"
  $automationAccount = "$ProjectPrefix-$LocationPrefix-$Environment-aa-main-app-space"
  $appSpaceState = (Get-State $state "AppSpace" $azToken $subscriptionId $resourceGroup $automationAccount "app-space-state")

  if ($null -eq $appSpaceState) {
    Write-Log "App Space State not found. Skipping additional state retrieval." $Global:LogStyles.SuppressedColour
  } else {
    Get-State $state "AppSpaceBase" $azToken $subscriptionId $resourceGroup $automationAccount "app-space-base-state" | Out-Null

    # Shared Hosts State
    if (![string]::IsNullOrEmpty($appSpaceState.prefixes.shared_hosts_project) -and (![string]::IsNullOrEmpty($appSpaceState.prefixes.shared_hosts_environment))) {
      $resourcePrefix = "$($appSpaceState.prefixes.shared_hosts_project)-$LocationPrefix-$($appSpaceState.prefixes.shared_hosts_environment)"
      $resourceGroup = "$resourcePrefix-rg-shared_hosts"
      $automationAccount = "$resourcePrefix-aa-main"
      Get-State $state "SharedHosts" $azToken $subscriptionId $resourceGroup $automationAccount "host-state" | Out-Null
    } else {
      Write-Log "Shared Hosts State not found." $Global:LogStyles.SuppressedColour
    }

    # Host Space State
    if (![string]::IsNullOrEmpty($appSpaceState.prefixes.host_space_project) -and (![string]::IsNullOrEmpty($appSpaceState.prefixes.host_space_environment))) {
      $resourcePrefix = "$($appSpaceState.prefixes.host_space_project)-$LocationPrefix-$($appSpaceState.prefixes.host_space_environment)"
      $resourceGroup = "$resourcePrefix-rg-host_space"
      $automationAccount = "$resourcePrefix-aa-main"
      Get-State $state "HostSpace" $azToken $subscriptionId $resourceGroup $automationAccount "host-space-state" | Out-Null
    } else {
      Write-Log "No Host Space specified. Skipping state retrieval." $Global:LogStyles.SuppressedColour
    }
  }
}

# Add the state to the global context
$context.Add($StateKey, (New-Context $state), $true)

if ($context.$StateKey.PSObject.Properties.Length -gt 0) {
  Write-Log "`nState added to context:" $Global:LogStyles.Heading2Colour
  $context.$StateKey.PSObject.Properties | ForEach-Object {
    Write-Log " - $($_.Name)"
  }
} else {
  Write-Log "`nWARNING: No IaC state was retrieved. This may cause issues with downstream logic which requires the state." $Global:LogStyles.WarningColour
}

