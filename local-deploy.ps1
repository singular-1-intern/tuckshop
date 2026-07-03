<#
.SYNOPSIS
  Runs a deploy locally for the specified app. By default, application configuration files are templated,
  but the application is not actually deployed, this must be explicitly enabled. This script should only be
  used when testing deployment configuration or debugging problems, otherwise the deployments should run
  through the usual deployment pipelines. (As a further protection, this script is hard coded to disallow
  deployment against Production environments. These must always run through pipelines)

  It is also assumed that you have all required tooling installed and configured as follows:
  - Azure CLI installed
  - A valid Azure session in Azure CLI
  - The required permissions to deploy to the project's Staging cluster.
  - Recommended: A GitHub Personal Access Token (PAT) configured in your Nuget config file with permissions
    to read the project's repository.
    (If this is not set up, the deployment will fall back on the local profiles.json file instead of downloading it)
.PARAMETER App
  The name of the app to deploy. This must match the name of a project in the blueprint.json file
.PARAMETER Version
  The version of the Docker image to deploy (E.g. "v20"). If not specified, defaults to "v1". (Optional)
.PARAMETER ProjectPrefix
  The project prefix.
.PARAMETER LocationPrefix
  The location prefix of the host environment to deploy to.
.PARAMETER HostPrefix
  The prefix of the host environment of the Kubernetes cluster to deploy to.
.PARAMETER Environment
  The environment to deploy to.
.PARAMETER AzureTenantId
  The Azure Tenant ID where the Kubernetes cluster is hosted. Defaults to the Singular Tenant.
.PARAMETER AzureSubscriptionId
  The Azure Subscription ID where the Kubernetes cluster is hosted. Defaults to the correct Singular subscription depending
  on the project prefix.
.PARAMETER GitHubRepository
  The GitHub repository to retrieve the deployment profiles file from. Defaults to "".
.PARAMETER GitHubRepositoryDeployBranch
  The branch of the GitHub repository to retrieve the deployment profiles file from. Defaults to "main".
  If you are using a different branch for release builds, you must specify it here.
.PARAMETER Deploy
  If specified, the script will perform the deployment. If not specified, the script will only template the configuration files.
  This is useful for testing the deployment configuration without actually deploying the app. (Optional)
.PARAMETER DryRun
  Perform a dry run where the helm command is executed with the `--dry-run` flag.
.PARAMETER HelmDebug
  If specified, the script will run the helm command with the `--debug` flag.
.PARAMETER PreserveContext
  Preserve the global context after the script has completed. This is useful for debugging and testing.
  If this is set, the script will not clean up the global context at the end of execution.
  (Optional. Defaults to false)
.PARAMETER ShowTimings
  If specified, the script will show timings for the deployment steps.
#>
[CmdletBinding()]
param(
  [Parameter(Position = 0, Mandatory)][string]$App,
  [Parameter(Position = 1, Mandatory)][string]$Version,
  [string]$ProjectPrefix = "ts",
  [string]$LocationPrefix = "we",
  [string]$HostPrefix = "stg",
  [string]$Environment = "qa",
  [string]$AzureTenantId = ($Env:AZURE_TENANT_ID ?? "c9871c9d-7fe6-4b1d-a97a-22b172b872e7"),
  [string]$AzureSubscriptionId = $Env:AZURE_SUBSCRIPTION_ID,
  [string]$GitHubRepository = "",
  [string]$GitHubRepositoryDeployBranch = "main",
  [switch]$Deploy,
  [switch]$DryRun,
  [switch]$HelmDebug,
  [switch]$PreserveContext,
  [switch]$ShowTimings
)

. (Join-Path $PSScriptRoot "IaC/scripts/libraries/common.ps1")

Write-Host "`nLocal Deploy" -ForegroundColor Cyan

$blueprint = (Get-Content "$PSScriptRoot/blueprint.json" -Raw | ConvertFrom-Json)
$projects = $blueprint.projects | Where-Object { $_.type -eq "DotNet" -or $_.type -eq "React" }

$project = $projects | Where-Object { $_.name -eq $App }
if ($null -eq $project) {
  throw "Project '$App' not found. Please provide a valid project name."
}

# Prevent deployment from being run against a Production environment
$productionEnvs = @("ppt", "pp", "prd", "dr")
if ($Deploy.IsPresent -and ($HostPrefix -in $productionEnvs -or $Environment -in $productionEnvs)) {
  throw "Local deployments to Production environments are not allowed. Please use the deployment pipelines instead."
}

# Ensure the Azure environment variables are set
$Env:AZURE_TENANT_ID = $AzureTenantId

# Hard code the Subscription ID to either Singular Cloud or ShareTrust Staging (Determined using the Project Prefix)
if ($ProjectPrefix -eq "st") {
  # ShareTrust Staging Subscription
  $Env:AZURE_SUBSCRIPTION_ID = "6df155e6-e43c-490b-a85b-7b0a89f42774" # sub-devtest
} elseif ($Environment -in $productionEnvs) {
  # Singular Cloud Production Subscription
  $Env:AZURE_SUBSCRIPTION_ID = "aa7782d9-0b79-4740-9750-caf85a5aba67" # sc-sub-prod
} else {
  # Singular Cloud Staging Subscription
  $Env:AZURE_SUBSCRIPTION_ID = "2e538ecd-e2f2-40d9-bc8a-272df8f5e2f7" # sc-sub-devtest
}

if ($DryRun) { $Deploy = $true }

Write-Host "Deploying $App (version: $Version) to $Environment environment..."

# Get the GitHub PAT Token from the user's Nuget config file
$gitHubToken = ""
$nugetConfigFile = Join-Path $Env:USERPROFILE "AppData/Roaming/NuGet/NuGet.Config"
if (!(Test-Path $nugetConfigFile)) {
  Write-Log "A local Nuget Config was not found at '$nugetConfigFile'. Please configure as detailed in this guide: https://github.com/SingularSystems/singular-wiki/blob/main/GitHub/NugetConfigSetup.md" $Global:LogStyles.WarningColour
} else {
  [xml]$nugetConfig = (Get-Content $nugetConfigFile -Raw)
  $credentials = $nugetConfig.configuration.packageSourceCredentials

  if ($credentials -and $credentials.Singular_x0020_GitHub) {
    $gitHubToken = $credentials.Singular_x0020_GitHub.add |
      Where-Object { $_.key -eq 'ClearTextPassword' } |
      Select-Object -ExpandProperty value

    $Env:GitHubToken = $gitHubToken
    $Env:GitHubRepository = $GitHubRepository
    $Env:GitHubRepositoryDeployBranch = $GitHubRepositoryDeployBranch
  } else {
    Write-Log "Nuget Config does not contain 'Singular GitHub' credentials. Please configure as detailed in this guide: https://github.com/SingularSystems/singular-wiki/blob/main/GitHub/NugetConfigSetup.md" $Global:LogStyles.WarningColour
  }
}

# Run the deployment script
./IaC/scripts/deploy/deploy-helm-chart.ps1 `
  -ProjectPrefix $ProjectPrefix `
  -LocationPrefix $LocationPrefix `
  -HostPrefix $HostPrefix `
  -Environment $Environment `
  -AppName $App `
  -Chart "./IaC/helm/neo-web-app" `
  -ImageTag $Version `
  -ShowTimings:$ShowTimings `
  -SkipDeployment:(!$Deploy) `
  -DryRun:$DryRun `
  -HelmDebug:$HelmDebug `
  -PreserveContext:$PreserveContext

