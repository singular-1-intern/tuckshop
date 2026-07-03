# ┌───────────────────────────────────────────────────────────────────────────┐
# │ GoCD Runtime Context Library                                              │
# ├───────────────────────────────────────────────────────────────────────────┤
# │ Dependencies                                                              │
# │   Libraries : common.ps1                                                  │
# │   Tools     : none                                                        │
# └───────────────────────────────────────────────────────────────────────────┘
<#
.SYNOPSIS
  Configure CLI tools context for the running GoCD agent.
.DESCRIPTION
  Applies configuration to ensure CLI tools work correctly when being executed by multiple agents at once.
.PARAMETER BaseTempFolder
  The base folder under which to put temp folders.
  (Optional. Defaults to the user's home folder)
.PARAMETER TempFolder
  The name of the temp folder to use. (Optional)
  If not set, the script will detect which GoCD agent is running and set this accordingly.
#>
[CmdletBinding()]
param(
  [string]$BaseTempFolder = $HOME,
  [string]$TempFolder
)

# Locate the scripts root path and load libraries
$scriptsPath = $PSScriptRoot; while ($scriptsPath -and (Split-Path $scriptsPath -Leaf) -ne "scripts") { $scriptsPath = Split-Path $scriptsPath }
if (!$scriptsPath) { throw "Could not locate the scripts root folder. Please ensure this script is nested beneath a 'scripts' parent folder." }
. (Join-Path $scriptsPath "libraries/common.ps1")

# Retrieve the global context
$context = Get-GlobalContext

# Set environment variable defaults
$Env:AZURE_CONFIG_DIR = $null
$Env:CICD_AGENT_ID = "default"

Write-Log "`n[ Configure Runtime Context ]" $Global:LogStyles.Heading1Colour

# Check if we are running under a GoCD pipeline
if (![string]::IsNullOrEmpty($Env:GO_PIPELINE_COUNTER)) {
  # Ensure that Azure CLI uses a different temp folder per agent
  #   By default, you can't have more than one process running Azure CLI at once because both az processes use the same temp folder.
  #   If one process logs out or otherwise changes the login context, it can cause the other process to fail with things like permission
  #   errors. This script solves the problem by detecting which GoCD agent the script is running under, and then setting Azure CLI to
  #   use a unique temp folder.
  $currentPath = (Get-Location).Path
  $matchRegex = $IsWindows ? "(?<=go[\\/]a)[0-9]*[\\/]{1}" : "(?<=/go-agent[-]*)[0-9]*[\\/]{1}"

  # Default Agent ID
  $agentId = "default"

  if ([string]::IsNullOrEmpty($TempFolder)) {
    $agentMatches = [Regex]::Match($currentPath, $matchRegex)
    if ($agentMatches.Length -gt 0) {
      $agentNumber = ("{0:d2}" -f [int]($agentMatches[0] -replace "[\\/]", ""))
      $folder = (Join-Path $BaseTempFolder ".gocd" "a$agentNumber-az-cli")
      $agentId = "gocd-agent$agentNumber"
    } else {
      $folder = (Join-Path $BaseTempFolder ".gocd" "default-az-cli")
    }
  } else {
    $folder = (Join-Path $BaseTempFolder $TempFolder)
  }

  # Ensure the temp folder exists
  New-Item -ItemType Directory -Force -Path $folder | Out-Null

  Write-Log "`nSetting environment variables" $Global:LogStyles.Heading2Colour

  # Set the environment variable so Azure CLI uses this folder to save its files
  $Env:AZURE_CONFIG_DIR = $folder

  # Set an Agent ID environment variable for any automation which needs to distinguish between agents
  $Env:CICD_AGENT_ID = $agentId

  # Also export the variables to the global context
  $runtimeContext = New-Context @{
    AzureConfigDir = $folder
    CiCdAgentId    = $agentId
  }

  $context.Add("Runtime", $runtimeContext, $true)

  Write-Logs (Format-KeyValues ([ordered]@{
        "AZURE_CONFIG_DIR" = $folder
        "CICD_AGENT_ID"    = $agentId
      })
  ) $Global:LogStyles.EmphasisColour


} else {
  Write-Log "Not running under a GoCD pipeline. Skipping context configuration." $Global:LogStyles.SuppressedColour
}

