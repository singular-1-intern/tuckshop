<#
.SYNOPSIS
  Executes a PowerShell script inside a docker container.
  (This is a standalone 'bootstrap' script to get a custom script running inside a container image with required dependencies preinstalled)
  NOTE: The parameters in this script have all been prefixed with 'Sr_' to avoid potential conflicts with the script being executed.
.PARAMETER Sr_ScriptPath
  The path of the script to execute. Must be relative to the VolumePath
.PARAMETER Sr_VolumeMappings
  The volumes to map to the container. These must be specified using "{LocalPath}:{ContainerPath}" format and can be a comma separated to map multiple volumes,
  E.g: "./host/path1:/container/path1,./host/path2:/container/path2".
  Please Note:
  - It is assumed that ScriptPath is relative to the first volume mapping.
  - The container's working directory is set to the folder containing the script
  - Host paths must be relative to the working directory the script runs under.
  - Container paths must be absolute paths.
  (Defaults to: "./:/tmp/scripts")
.PARAMETER Sr_EnvironmentVariables
  Comma separated list of environment variables to pass into the container. Supports using wildcards (E.g. 'NeoAz*').
  Note that environment variable values may not have spaces in them.
  (Defaults to 'Neo*,AZURE_*,CLOUDFLARE_*,GO_PIPELINE_*')
.PARAMETER Sr_Shell
  Shell to use to execute the script (Defaults to 'pwsh')
.PARAMETER Sr_Interactive
  Should the session be interactive?
.PARAMETER Sr_ContainerRegistry
  The container registry holding the container to run (Defaults to singular.azurecr.io)
.PARAMETER Sr_ContainerRepository
  The container repository of the container to run (Defaults to neo-ps)
.PARAMETER Sr_ContainerImageTag
  The container tag / version of the container (Defaults to v1.2.154)
.PARAMETER Sr_ContainerRegistryAuthType
  The kind of authentication required by the registry. Possible values: None, Azure  (Defaults to 'Azure')
.PARAMETER UnboundArgs
  Additional parameters required by the script can be specified, and they will be automatically passed in when the script is executed in the container.
  Note that the script cannot accept any parameters with the same name as one of the parameters in this script.
  Most primitive data types can be used, such as [string], [int] or [DateTime], but [bool] should be avoided in favour of using [switch].
  Passing of complex object types is not supported.
#>
[CmdletBinding()]
param(
  [Parameter(Mandatory, Position = 0)][string]$Sr_ScriptPath,
  [string]$Sr_VolumeMappings = "./:/tmp/scripts",
  [string]$Sr_EnvironmentVariables = "Neo*,Operations*,CodeCoverage*,AZURE_*,AAD_*,CLOUDFLARE_*,GO_PIPELINE_*",
  [string]$Sr_Shell = "pwsh",
  [switch]$Sr_Interactive,
  [string]$Sr_ContainerRegistry = $Env:NeoContainerRegistry ?? "singular.azurecr.io",
  [string]$Sr_ContainerRepository = $Env:NeoContainerRepository ?? "neo-ps",
  [string]$Sr_ContainerImageTag = $Env:NeoContainerImageTag ?? (![string]::IsNullOrEmpty($Env:NeoPsVersion) ? "v$($Env:NeoPsVersion)" : "v1.2.154"),
  [ValidateSet("", "None", "Azure")]$Sr_ContainerRegistryAuthType = $Env:NeoContainerRegistryAuthType ?? "Azure",
  [Parameter(ValueFromRemainingArguments)]$UnboundArgs
)

$ErrorActionPreference = "Stop"
$InformationPreference = "Continue"

# Locate the scripts root path and load libraries
$scriptsPath = $PSScriptRoot; while ($scriptsPath -and (Split-Path $scriptsPath -Leaf) -ne "scripts") { $scriptsPath = Split-Path $scriptsPath }
if (!$scriptsPath) { throw "Could not locate the scripts root folder. Please ensure this script is nested beneath a 'scripts' parent folder." }
. (Join-Path $scriptsPath "libraries/common.ps1")

Write-Logs @(
  "`n",
  " █▀▀ █▀▀ █▀▄ ▀█▀ █▀█ ▀█▀   █▀▄ █ █ █▀█ █▀█ █▀▀ █▀▄",
  " ▀▀█ █   █▀▄  █  █▀▀  █    █▀▄ █ █ █ █ █ █ █▀▀ █▀▄",
  " ▀▀▀ ▀▀▀ ▀ ▀ ▀▀▀ ▀    ▀    ▀ ▀ ▀▀▀ ▀ ▀ ▀ ▀ ▀▀▀ ▀ ▀"
  ""
) $Global:LogStyles.Heading1Colour

Write-Logs (Format-KeyValues ([ordered]@{
      "Working Directory"            = (Get-Location)
      "Script Path"                  = $Sr_ScriptPath
      "Volume Mappings"              = $Sr_VolumeMappings
      "Environment Variables"        = $Sr_EnvironmentVariables
      "Shell"                        = $Sr_Shell
      "Container Registry"           = $Sr_ContainerRegistry
      "Container Repository"         = $Sr_ContainerRepository
      "Container Image Tag"          = $Sr_ContainerImageTag
      "Container Registry Auth Type" = $Sr_ContainerRegistryAuthType
    })) $Global:LogStyles.EmphasisColour

# Check if the container image exists locally
$imageName = "$Sr_ContainerRegistry/$($Sr_ContainerRepository):$($Sr_ContainerImageTag)"
$imageId = docker images -q $imageName

Write-Log "`nDocker Image Check" $Global:LogStyles.Heading1Colour

if ([string]::IsNullOrEmpty($imageId) -and $Sr_ContainerRegistryAuthType -ne "None") {
  # If not, we need to connect to the container registry so docker can pull it down
  Write-Log "Image $($Global:LogStyles.EmphasisColour)$($imageName)$($Global:LogStyles.TextColour) does not exist locally."

  if ($Sr_ContainerRegistryAuthType -eq "Azure") {
    Write-Log "Connecting to Azure Container Registry..."

    # Use existing login, if there is one
    $azToken = (az account get-access-token | ConvertFrom-Json)
    if ($null -ne $azToken -and [DateTime]$azToken.expiresOn -gt [DateTime]::Now) {
      Write-Log "Using existing Azure login"
    } else {
      $tenantId = $Env:AZURE_TENANT_ID
      $servicePrincipalClientId = $Env:AZURE_CLIENT_ID
      $servicePrincipalSecret = $Env:AZURE_CLIENT_SECRET

      Assert-HasValue $tenantId "Unable to connect to Azure, please ensure the AZURE_TENANT_ID environment variable is set."
      Assert-HasValue $servicePrincipalClientId "Unable to connect to Azure, please ensure the AZURE_CLIENT_ID environment variable is set."
      Assert-HasValue $servicePrincipalSecret "Unable to connect to Azure, please ensure the AZURE_CLIENT_SECRET environment variable is set."

      Write-Log "Using Azure credentials from environment variables"
      $azLogin = az login -t $tenantId -u $servicePrincipalClientId -p $servicePrincipalSecret --service-principal
      if (!$azLogin) { throw "Error logging into Azure" }
    }

    Write-Log "Connecting to container registry: $($Global:LogStyles.EmphasisColour)$Sr_ContainerRegistry"
    $acrLogin = az acr login --name $Sr_ContainerRegistry
    if (!$acrLogin) { throw "Error logging into Azure Container Registry '$Sr_ContainerRegistry'" }
  }
} else {
  Write-Log "Image $($Global:LogStyles.EmphasisColour)$($imageName)$($Global:LogStyles.TextColour) exists locally."
}

Write-Log "`nEnvironment Variables" $Global:LogStyles.Heading1Colour
Write-Log "Checking for environment variables matching filters: $Sr_EnvironmentVariables"

# Get Environment Variables to pass into the docker container
$neoVariables = @{}
$Sr_EnvironmentVariables.Split(",") | ForEach-Object {
  # Check if any environment variables exist
  $value = (Test-Path Env:$_) ? (Get-ChildItem Env:$_) : ""
  if ($value -is [Object[]]) {
    $value | ForEach-Object { $neoVariables[$_.Name] = $_.Value }
  } elseif ($value -is [hashtable]) {
    foreach ($key in $value.Keys) {
      $neoVariables[$key] = $value.$key
    }
  } elseif ($value -is [System.Collections.DictionaryEntry]) {
    $neoVariables[$value.Name] = $value.Value
  } elseif (![string]::IsNullOrWhiteSpace($value)) {
    # Assume it's a single key/value pair
    $neoVariables[$_] = $value
  } else {
    # No environment variables were found. Leave neoVariables empty.
  }
}

Write-Log "Environment Variables found:"
$neoVariables.Keys | Sort-Object | ForEach-Object { Write-Log " $($Global:LogStyles.SymbolColour)- $($Global:LogStyles.TextColour)$_" }

# Generate Volume Mappings
Write-Log "`nVolume Mappings" $Global:LogStyles.Heading1Colour

if ([string]::IsNullOrEmpty($Sr_VolumeMappings)) { throw "VolumeMappings must be specified." }
$volumeMappingArguments = @()
$Sr_ScriptPathsResolved = $false
$Sr_VolumeMappingsList = $Sr_VolumeMappings.Split(",")

foreach ($volumeMapping in $Sr_VolumeMappingsList) {
  $volumeMappingItems = $volumeMapping.Split(":")
  $hostVolumePath = $volumeMappingItems[0]
  $containerVolumePath = $volumeMappingItems[1]

  # Resolve the Host Volume Path to get the absolute location, and to verify that it exists
  $hostVolumePath = (Resolve-Path $hostVolumePath -ErrorAction Stop)

  # We need to resolve script paths using the first volume mapping
  if (!$Sr_ScriptPathsResolved) {
    $hostScriptPath = (Resolve-Path (Join-Path $hostVolumePath $Sr_ScriptPath))
    $scriptRelativePath = ((Split-Path -Path $hostScriptPath).Replace($hostVolumePath, ".").Replace("\", "/"))
    $scriptName = (Split-Path -Path $hostScriptPath -Leaf)

    $containerWorkingPath = "$containerVolumePath/$scriptRelativePath"
    $containerScriptPath = "$containerVolumePath/$scriptRelativePath/$scriptName"

    Write-Log "Container Script Path : $containerScriptPath"
    $Sr_ScriptPathsResolved = $true
  }

  Write-Log "Volume Mapping        : $hostVolumePath -> $containerVolumePath"
  $volumeMappingArguments += @("-v", "$($hostVolumePath):$($containerVolumePath)")
}

# We need to dynamically generate the docker command to include the environment variables.
# Since Invoke-Expression is dangerous and can lead to code injections, we use the
# call operator (&) instead.
$envVarArguments = @()
$neoVariables.ForEach({ $_.GetEnumerator().ForEach({
        # Escape any double quotes in environment strings, or the docker command will fail.
        $escapedValue = $_.Value.Replace("`"", "\`"")
        $envVarArguments += @("--env", "$($_.Name)=$($escapedValue)")
      }) })

$interactivityFlags = $Sr_Interactive ? @("-it") : @()
$terminal = "xterm-256color"

$verbose = $VerbosePreference -eq "Continue" ? $true : $false
$debug = $DebugPreference -eq "Continue" ? $true : $false
$preferenceParams = ($Sr_Shell -eq "pwsh" ? @("-InformationAction:$InformationPreference", "-Debug:$debug", "-Verbose:$verbose", "-ErrorAction:$ErrorActionPreference", "-WarningAction:$WarningPreference") : @())

try {
  Write-Log "`nExecute Script" $Global:LogStyles.Heading1Colour

  $dockerArguments = @()
  $dockerArguments += @("run", "-w", $containerWorkingPath)
  $dockerArguments += $volumeMappingArguments
  $dockerArguments += ![string]::IsNullOrEmpty($terminal) ? @("-e", "TERM=$terminal") : @()
  $dockerArguments += $envVarArguments
  $dockerArguments += $interactivityFlags
  $dockerArguments += @(
    $imageName
    $Sr_Shell
    $containerScriptPath
  )

  $dockerArguments += $UnboundArgs
  $dockerArguments += $preferenceParams

  # Show the Docker Arguments if debug is enabled
  Write-Debug "Docker Arguments: "
  $dockerArguments.ForEach({
      Write-Debug "  $_"
    })

  # The call operator requires an array of strings with no spaces for the arguments
  Write-Log "Executing Script $($Global:LogStyles.EmphasisColour)$scriptName$($Global:LogStyles.TextColour) inside container $($Global:LogStyles.EmphasisColour)$imageName$($Global:LogStyles.TextColour)"
  Write-Log "`n────────────────────────────────────────────────────────────────────────" $Global:LogStyles.SymbolColour
  Write-Log ""

  & docker $dockerArguments

  # Ensure that we throw an error if one occurred in the container
  if (-not $?) {
    throw "An error occurred during containerised script execution."
  }

  Write-Log "`n────────────────────────────────────────────────────────────────────────" $Global:LogStyles.SymbolColour
  Write-Log ""
  Write-Log "Containerised Script Execution Completed" $Global:LogStyles.SuccessColour

} catch {
  throw "Error: $($_.Exception.Message)"
} finally {
  # If we logged into Azure using a Service Principal, log out
  if ($azLogin) {
    Write-Information "`nLogging out of Azure..."
    az logout
  }
}

