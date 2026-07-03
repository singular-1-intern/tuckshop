<#
.SYNOPSIS
  Builds and tags a docker image. This script should be run with repository root as the working directory
.PARAMETER ProjectName
  The name of the project (PascalCase)
.PARAMETER ProjectPath
  The base path of the project relative to the repository root
.PARAMETER Registry
  The container registry to use in the image tag.
.PARAMETER Repository
  The container repository to use in the image tag.
.PARAMETER Tag
  The tag to use in the image tag. (Optional, omit this parameter to disable tagging of the image)
.PARAMETER DockerFile
  The name of the docker file to build. (Optional. Defaults to searching for '$Dockerfile.$ProjectName' in the repository root, or '$Dockerfile' in project root)
.PARAMETER NugetConfig
  The Nuget.Config as text. (Optional. Defaults to $Env:NugetConfig)
.PARAMETER NugetCacheId
  The Docker nuget cache ID. The Dockerfile typically defaults this to 'nuget', and it doesn't need to be overridden unless you have a specific reason for wanting
  your project to use its own cache.
#>
[CmdletBinding()]
param(
  [Parameter(Mandatory)][string]$ProjectName,
  [Parameter(Mandatory)][string]$ClientName,
  [Parameter(Mandatory)][string]$ProjectPath,
  [Parameter(Mandatory)][string]$Repository,
  [string]$Registry = $Env:ContainerRegistry,
  [string]$Tag = "",
  [string]$Dockerfile = "Dockerfile",
  [string]$NugetConfig = $Env:NugetConfig,
  [string]$NugetCacheId = "",
  [string]$NeoPsVersion = $Env:NeoPsVersion,
  [switch]$CodeCoverageEnabled = [System.Convert]::ToBoolean($Env:CodeCoverageEnabled) ?? $true
)

$ErrorActionPreference = "Stop"

# Locate the scripts root path and load libraries
$scriptsPath = $PSScriptRoot; while ($scriptsPath -and (Split-Path $scriptsPath -Leaf) -ne "scripts") { $scriptsPath = Split-Path $scriptsPath }
if (!$scriptsPath) { throw "Could not locate the scripts root folder. Please ensure this script is nested beneath a 'scripts' parent folder." }
. (Join-Path $scriptsPath "libraries/common.ps1")

Write-Logs @(
  "`n",
  " █▀▄ █ █ ▀█▀ █   █▀▄   ▀█▀ █▄█ █▀█ █▀▀ █▀▀",
  " █▀▄ █ █  █  █   █ █    █  █ █ █▀█ █ █ █▀▀",
  " ▀▀  ▀▀▀ ▀▀▀ ▀▀▀ ▀▀    ▀▀▀ ▀ ▀ ▀ ▀ ▀▀▀ ▀▀▀"
  ""
) $Global:LogStyles.Heading1Colour

# Configure context for the runtime environment if a script is present.
$runtimeContextScript = "$scriptsPath/context/configure-runtime-context.ps1"
if (Test-Path $runtimeContextScript) {
  & $runtimeContextScript
}

Write-Log "`n[ Build Container Image ]" $Global:LogStyles.Heading1Colour

if ([string]::IsNullOrEmpty($Registry)) {
  throw "Container registry name must be provided"
}

try {
  Push-Location

  # Allow the environment variable to be overridden if an argument has been supplied
  $Env:NugetConfig = $NugetConfig

  # Check for a Dockerfile in the repository root
  $rootDockerfile = "$Dockerfile.$ProjectName"
  $buildPath = (Join-Path (Get-Location) $ProjectPath)
  $rootPath = (Get-Item (Get-Location))
  $path = (Get-Item (Join-Path (Get-Location) $ProjectPath))

  do {
    if (Test-Path (Join-Path $path.FullName $rootDockerfile)) {
      # Build using the Dockerfile at the repository root
      Write-Log "Found Dockerfile in parent folder." $Global:LogStyles.SuppressedColour
      $Dockerfile = $rootDockerfile
      $buildPath = $path.FullName
      break
    }
    $path = $path.Parent
  } while ($path -and ($path.FullName -ne $rootPath.Parent.FullName))

  $relativeBuildPath = $buildPath.Substring($rootPath.FullName.Length).Replace("\", "/").TrimStart("\").TrimStart("/")
  Write-Logs (Format-KeyValues ([ordered]@{
        "Build Path"      = "/$relativeBuildPath"
        "Dockerfile Name" = $Dockerfile
      })
  ) $Global:LogStyles.EmphasisColour

  Set-Location $buildPath

  # Build and tag the container image
  $tagImage = ![string]::IsNullOrEmpty($Tag)
  $imageName = "$($Registry)/$($Repository):$($Tag)"

  # Generate the arguments to pass to Docker
  $arguments = "build"

  $arguments += ![string]::IsNullOrEmpty($CodeCoverageEnabled) ? " --build-arg CodeCoverageEnabled=$CodeCoverageEnabled" : ""
  # Load the NeoPsVersion as a build argument from the env variables
  $arguments += ![string]::IsNullOrEmpty($NeoPsVersion) ? " --build-arg NeoPsVersion=$NeoPsVersion" : ""

  # Use a custom Nuget Cache ID?
  $arguments += ![string]::IsNullOrEmpty($NugetCacheId) ? " --build-arg NugetCacheId=$NugetCacheId" : ""

  if ($tagImage) {
    $arguments += " -t $imageName"
  }

  $arguments += " -f $dockerfile --secret id=NugetConfig,env=NugetConfig ."

  Write-Log "`nBuilding Docker Image" $Global:LogStyles.Heading1Colour
  Write-Logs (Format-KeyValues ([ordered]@{
        "Name"             = ($tagImage ? $imageName : "<no image name>")
        "Docker arguments" = $arguments
      })
  ) $Global:LogStyles.EmphasisColour
  Write-Log ""

  & docker ($arguments.Split(" "))

  if (-not $?) {
    throw "Error building docker image"
  }

  Write-Log "`nDocker Image Build Complete" $Global:LogStyles.SuccessColour

  # Check if Code Coverage generation process is allowed in the current running environment.
  if ($CodeCoverageEnabled -eq $true) {
    Pop-Location
    Write-Log "`nChecking for coverage assets from build container" $Global:LogStyles.Heading1Colour
    # Adjust the path depending on the host system
    $targetAssetsVolumePath = Join-Path $(Get-Location) "/tmp"
    $podAssetsLocation = "/tmp/code-coverage"

    # Ensure the directory exists
    if (-Not (Test-Path $targetAssetsVolumePath)) {
      New-Item -ItemType Directory -Path $targetAssetsVolumePath -Force
    }

    $id = $(docker create $imageName 2>&1)
    if ($LASTEXITCODE -ne 0) {
      Write-Log "Failed to create intermediate code coverage container to extract the assets, exiting..." $Global:LogStyles.WarningColour
      exit
    }

    $copyOperationOutput = $(docker cp "$($id):/app/code-coverage" "$targetAssetsVolumePath" 2>&1)
    if ($LASTEXITCODE -ne 0) {
      Write-Log "`nFailed to copy code coverage assets from the container. No code coverage assets will be published. `n$copyOperationOutput" $Global:LogStyles.WarningColour
      exit
    }

    # Remove the intermediate container
    $(docker rm -v $id) | Out-Null

    # Initiate the script to upload the code coverage assets
    $invokeScript = "$scriptsPath/operational/invoke-script-in-container.ps1"
    & $invokeScript -Sr_ScriptPath ./IaC/scripts/build/upload-code-coverage-assets.ps1 -Sr_VolumeMappings "./:/tmp/scripts,./tmp:/tmp" -AssetsLocation $podAssetsLocation -ClientName $ClientName
  }
} finally {
  Pop-Location
}

