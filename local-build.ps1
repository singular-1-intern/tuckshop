<#
.SYNOPSIS
  Runs a local docker build for the specified app
.PARAMETER App
  The name of the app to build. This must match the name of a project in the blueprint.json file
.PARAMETER Tag
  The tag to use for the Docker image. If not specified, the image is not tagged. (Optional)
.PARAMETER NugetCacheId
  The Nuget cache ID to use for the Docker build. This is used to cache the Nuget packages in the Docker image. (Optional)
.PARAMETER NeoPsVersion
  The version of Neo.PS to pass to the Docker build. Neo.PS is used to generate the code coverage report. (Optional)
.PARAMETER Plain
  Use the --progress=plain option when running the docker build. (Optional)
.PARAMETER UseNugetEnvVar
  Instead of loading the local user's Nuget.Config, get the nuget config from the 'NugetConfig' environment variable. (Optional)
.PARAMETER GenerateTag
  Automatically generate a tag for the Docker image. (Optional)
.PARAMETER CodeCoverageEnabled
  Enable Code Coverage report generation? (Optional)
.PARAMETER RunImage
  Run the built image after the build is complete. If used in conjunction with -RunDive, then -RunDive will be ignored. (Optional)
.PARAMETER RunDive
  Run the 'dive' tool on the built image to analyze the layers. (Optional)
#>
[CmdletBinding()]
param(
  [Parameter(Position = 0, Mandatory)][string]$App,
  [string]$Tag = "",
  [string]$NugetCacheId = "",
  [string]$NeoPsVersion = $Env:NeoPsVersion ?? "1.2.133",
  [switch]$Plain,
  [switch]$UseNugetEnvVar,
  [switch]$GenerateTag,
  [switch]$CodeCoverageEnabled = [System.Convert]::ToBoolean($Env:CodeCoverageEnabled) ?? $false,
  [switch]$RunImage,
  [switch]$RunDive
)

Write-Host "Local Build" -ForegroundColor Cyan

# Ensure BuiltKit is enabled
$Env:DOCKER_BUILDKIT = 1

$blueprint = (Get-Content "$PSScriptRoot/blueprint.json" -Raw | ConvertFrom-Json)
$generators = $blueprint.generators
$projects = $blueprint.projects | Where-Object { $_.type -eq "DotNet" -or $_.type -eq "React" }

$project = $projects | Where-Object { $_.name -eq $App }
if ($null -eq $project) {
  throw "Project '$App' not found. Please provide a valid project name."
}

$projectPath = (Join-Path $PSScriptRoot $project.folder)
$projectName = (Get-Culture).TextInfo.ToTitleCase($project.name.Replace("-", " ")).Replace(" ", "")

$baseDockerfileLocation = $PSScriptRoot
if ($project.type -eq "DotNet") {
  if (![string]::IsNullOrEmpty($generators.dotNetProjects.dockerFileMoveLocation)) {
    $baseDockerfileLocation = (Join-Path $PSScriptRoot $generators.dotNetProjects.dockerFileMoveLocation)
  }
} elseif ($project.type -eq "React") {
  if (![string]::IsNullOrEmpty($generators.reactProjects.dockerFileMoveLocation)) {
    $baseDockerfileLocation = (Join-Path $PSScriptRoot $generators.reactProjects.dockerFileMoveLocation)
  }
}

# If a Dockerfile is found in the base location, use it. Otherwise look for one in the Project folder
$dockerFilePath = (Join-Path $baseDockerfileLocation "Dockerfile.$projectName")
if (!(Test-Path $dockerFilePath)) {
  $dockerFilePath = (Join-Path $projectPath "Dockerfile")
  if (!(Test-Path $dockerFilePath)) {
    throw "No Dockerfile found for project '$App'."
  }
}

$dockerfileBasePath = (Split-Path -Path $dockerFilePath -Parent)
$dockerfileName = (Split-Path -Path $dockerFilePath -Leaf)

# If an option which requires it is enabled, force tagging of the image
if (($RunImage -or $CodeCoverageEnabled -or $RunDive) -and [string]::IsNullOrEmpty($Tag) -and !$GenerateTag) {
  Write-Host "Image Run, Code Coverage or Dive is enabled, generating a tag for the image..." -ForegroundColor Yellow
  $GenerateTag = $true
}

if ($GenerateTag) {
  $Tag = "singular.azurecr.io/$($project.name):v1"
}

Write-Host "App Name   : $App" -ForegroundColor DarkGray
Write-Host "Dockerfile : $($dockerFilePath.Substring($PSScriptRoot.Length + 1))" -ForegroundColor DarkGray
Write-Host "Tag        : $Tag" -ForegroundColor DarkGray

Write-Host "`nBuilding $($project.name)" -ForegroundColor Cyan

if (!$UseNugetEnvVar) {
  $nugetConfigFile = Join-Path $Env:USERPROFILE "AppData/Roaming/NuGet/NuGet.Config"
  if (!(Test-Path $nugetConfigFile)) {
    throw "Nuget Config not found at '$nugetConfigFile', exiting..."
  }

  $nugetConfigLines = Get-Content $nugetConfigFile

  # Filter out unwanted Nuget Sources from the Nuget.Config file
  $nugetConfigLines = $nugetConfigLines | Where-Object {
    $_ -notmatch "<add key=`"Microsoft Visual Studio Offline Packages`"" -and `
      $_ -notmatch "<add key=`"Singular Nexus`"" -and `
      $_ -notmatch "<add key=`"SingularPowerShell`""
  }

  # Merge the lines into a string
  $Env:NugetConfig = $nugetConfigLines -join "`n"

  Write-Host "Nuget Config: $nugetConfigFile" -ForegroundColor DarkGray
} else {
  Write-Host "Using Nuget Config from environment variable" -ForegroundColor DarkGray
  if ([string]::IsNullOrEmpty($Env:NugetConfig)) {
    throw "Nuget Config not found in environment variable, exiting..."
  }
}

try {
  Write-Host "Switching to path: $dockerfileBasePath" -ForegroundColor DarkGray
  Push-Location $dockerfileBasePath

  # Generate the arguments to pass to Docker
  $arguments = "build"
  $arguments += " -f $dockerfileName --secret id=NugetConfig,env=NugetConfig ."

  $arguments += ![string]::IsNullOrEmpty($Tag)                  ? " -t $Tag" : ""
  $arguments += ![string]::IsNullOrEmpty($NeoPsVersion)         ? " --build-arg NeoPsVersion=$NeoPsVersion" : ""
  $arguments += ![string]::IsNullOrEmpty($CodeCoverageEnabled)  ? " --build-arg CodeCoverageEnabled=$CodeCoverageEnabled" : " --build-arg CodeCoverageEnabled=$false"
  $arguments += ![string]::IsNullOrEmpty($NugetCacheId)         ? " --build-arg NugetCacheId=$NugetCacheId" : ""
  $arguments += $Plain                                          ? " --progress=plain" : ""

  Write-Host "`nDocker arguments:" -ForegroundColor DarkGray
  Write-Host "$arguments`n" -ForegroundColor DarkGray

  & docker ($arguments.Split(" "))
  if ($LASTEXITCODE -ne 0) {
    Write-Host "`nDocker build failed, exiting..." -ForegroundColor Red
    exit $LASTEXITCODE
  }

  if (![string]::IsNullOrEmpty($Tag)) {
    Write-Host "`nImage tagged as: $Tag`n" -ForegroundColor Green
  }

  # Check if Code Coverage generation process is allowed in the current running environment.
  if ($CodeCoverageEnabled) {
    Write-Host "Checking for coverage assets from build container" -ForegroundColor Cyan
    # Adjust the path depending on the host system
    $targetAssetsLocalPath = Join-Path $(Get-Location) "/tmp"

    # Ensure the directory exists
    if (-Not (Test-Path $targetAssetsLocalPath)) {
      New-Item -ItemType Directory -Path $targetAssetsLocalPath -Force
    }

    $id = $(docker create $Tag 2>&1)
    if ($LASTEXITCODE -ne 0) {
      Write-Host "Failed to create intermediate code coverage container to extract the assets, exiting..." -ForegroundColor Yellow
      exit $LASTEXITCODE
    }

    $copyOperationOutput = $(docker cp "$($id):/app/code-coverage" "$targetAssetsLocalPath" 2>&1)
    if ($LASTEXITCODE -ne 0) {
      Write-Host "`nFailed to copy code coverage assets from the container. No code coverage assets will be published. `n$copyOperationOutput" -ForegroundColor Yellow
      exit $LASTEXITCODE
    }

    # Remove the intermediate container
    $(docker rm -v $id) | Out-Null
  }

  if ($RunImage) {
    Write-Host "`nRunning the built image..." -ForegroundColor Cyan
    docker run -it --entrypoint /bin/bash $Tag
  }

  if ($RunDive -and !$RunImage) {
    try {
      # This fixes an issue where Dive sometimes fails to communicate with Docker (Env var is restored after running Dive)
      $originalDockerHost = $Env:DOCKER_HOST
      $Env:DOCKER_HOST = "npipe:////./pipe/docker_engine"

      Write-Host "`nRunning dive on the image to analyze the layers..." -ForegroundColor Cyan
      dive $Tag
    } finally {
      $Env:DOCKER_HOST = $originalDockerHost
    }
  }

} finally {
  Pop-Location
  if (!$UseNugetEnvVar) {
    # Clear the NugetConfig environment variable
    $Env:NugetConfig = ""
  }
}

