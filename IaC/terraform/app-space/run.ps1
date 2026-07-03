<#
.SYNOPSIS
  This script is used to run terraform commands in the app space module.
  It compiles a set of configurations and passes them to the run-terraform.ps1 script located in the scripts/operational folder.
.PARAMETER Location
  The Azure location of the module that resources are provisioned within. This is used in resource name generation.
.PARAMETER Env
  The environment in which the resources are provisioned. This is used in resource name generation.
.PARAMETER Action
  The terraform action to run. (Default: apply)
  Valid options are:
    plan         : Runs 'terraform plan'
    show-plan    : Runs 'terraform show <plan-file>', which outputs the plan to the console
    apply        : Runs 'terraform apply'
    upgrade      : Runs 'terraform init -upgrade', then 'terraform providers lock' for all platforms to ensure cross platform compatibility.
    destroy      : Runs 'terraform destroy'
    list-state   : Runs 'terraform list state'
    output       : Runs 'terraform output'
    force-unlock : Runs 'terraform force-unlock $LockId' (Requires the $LockId parameter)
.PARAMETER ProjectPrefix
  The prefix used in the resource name generation.
.PARAMETER TerraformVersion
  The version of terraform to use. (Default: 1.3.6)
.PARAMETER Target
  When running an apply, this parameter can be used to target a specific resource.
.PARAMETER LockId
  The lock ID used for the force-unlock action. This is required when using the force-unlock action.
.PARAMETER AutoApprove
  Automatically approve the terraform action.
.PARAMETER UsePlanFile
  Use a plan file for the terraform action.
.PARAMETER PlanMode
  The mode to use when running the 'plan' operation. (Default: apply)
    apply        : Default mode. Generates a plan file to be used in an apply operation.
    refresh-only : Generates a plan file that only refreshes the state.
    destroy      : Generates a plan file that destroys all resources.
.PARAMETER SkipStateCleanup
  Skip the cleanup of the state file after the terraform action is completed.
.PARAMETER ClearCache
  Clear the terraform cache before running the terraform action.
#>
[CmdletBinding()]
Param(
  # When modifying the parameters, ensure that the changes are applied to other run scripts too. Enforce consistency.
  [Parameter(Position = 0)][ValidateSet("we", "ne", "eus2", "cus", "zan", "zaw")]$Location = "we",
  [Parameter(Position = 1)][ValidateSet("ldev", "dev", "qa", "uat", "ppt", "pp", "prd", "dr")][string]$Env = "qa",
  [Parameter(Position = 2)][ValidateSet("plan", "show-plan", "apply", "upgrade", "destroy", "output", "list-state", "force-unlock")][string]$Action = "apply",
  [string]$ProjectPrefix = "ts",
  [string]$TerraformVersion = "1.3.6",
  [string]$Target = "",
  [string]$LockId = "",
  [switch]$AutoApprove,
  [switch]$UsePlanFile,
  [ValidateSet("apply", "refresh-only", "destroy")][string]$PlanMode = "apply",
  [switch]$SkipStateCleanup,
  [switch]$ClearCache
)

$ErrorActionPreference = "Stop"

# Locate the repository root path
$rootPath = $PSScriptRoot; while ($rootPath -and !((Test-Path (Join-Path $rootPath 'IaC')) -or (Test-Path (Join-Path $rootPath 'blueprint.json')) -or (Test-Path (Join-Path $rootPath '.git')))) { $rootPath = Split-Path $rootPath -Parent }
if (!$rootPath) { throw "Could not locate the root folder. Please ensure that a blueprint.json file, IaC folder or .git folder exists in the repository root." }

# Load libraries
$scriptsPath = (Join-Path $rootPath "IaC/scripts")
. (Join-Path $scriptsPath "libraries/common.ps1")

Write-Logs @(
  "`n",
  " █▀▀ ▀█▀ █▀█ █▀▀ █ █ █   █▀█ █▀▄   █▀▀ █   █▀█ █ █ █▀▄ ",
  " ▀▀█  █  █ █ █ █ █ █ █   █▀█ █▀▄   █   █   █ █ █ █ █ █ ",
  " ▀▀▀ ▀▀▀ ▀ ▀ ▀▀▀ ▀▀▀ ▀▀▀ ▀ ▀ ▀ ▀   ▀▀▀ ▀▀▀ ▀▀▀ ▀▀▀ ▀▀  "
  "`n[ App Space ]"
) $Global:LogStyles.Heading1Colour

# Tfvars files to be used. Latter files will override the values in the former files.
$tfvarsFiles = @(
  (Join-Path "tfvars" "terraform.tfvars"),
  (Join-Path "tfvars" $Location "terraform.$Env.tfvars")
)

# Extract tenant_id and subscription_id from the tfvars files
$azureIds = Get-TerraformAzureIdsFromTfvars -TfvarsFiles $tfvarsFiles -BasePath $PSScriptRoot
$backendConfig = @{
  # These are Azure specific arguments for state storage backend
  tenant_id            = $azureIds.TenantId
  subscription_id      = $azureIds.SubscriptionId
  resource_group_name  = "$($ProjectPrefix)-$($Location)-$($Env)-rg-app_space"
  storage_account_name = "$($ProjectPrefix)$($Location)$($Env)sttfstate"
}

# Ensure that the terraform runner exists
$runnerPath = Join-Path $scriptsPath 'operational/run-terraform.ps1'
if (!(Test-Path $runnerPath)) { throw "Could not locate the terraform runner script at $runnerPath" }

# If ARM_* environment variables aren't provided, default to AZURE_* environment variables.
# (The azurerm and azuread providers look for ARM_*)
$Env:ARM_TENANT_ID = $Env:ARM_TENANT_ID ?? $Env:AZURE_TENANT_ID
$Env:ARM_SUBSCRIPTION_ID = $Env:ARM_SUBSCRIPTION_ID ?? $Env:AZURE_SUBSCRIPTION_ID
$Env:ARM_CLIENT_ID = $Env:ARM_CLIENT_ID ?? $Env:AZURE_CLIENT_ID
$Env:ARM_CLIENT_SECRET = $Env:ARM_CLIENT_SECRET ?? $Env:AZURE_CLIENT_SECRET

# If there are Service Principal details in environment variables, pass them into the terraform module.
if (![string]::IsNullOrEmpty($Env:ARM_CLIENT_ID) -and ![string]::IsNullOrEmpty($Env:ARM_CLIENT_SECRET)) {
  $Env:TF_VAR_service_principal = "{ client_id = `"$($Env:ARM_CLIENT_ID)`", client_secret = `"$($Env:ARM_CLIENT_SECRET)`" }"
  $FoundServicePrincipal = $true
}

# Configuration Summary
# Configuration Summary
Write-Logs (Format-KeyValues ([ordered]@{
      "Action"             = $Action
      "Location"           = $Location
      "Environment"        = $Env
      "Subscription ID"    = $backendConfig.subscription_id
      "Resource Group"     = $backendConfig.resource_group_name
      "State Storage Name" = $backendConfig.storage_account_name
      "Principal Type"     = $($true -eq $FoundServicePrincipal ? 'Service Principal' : 'User')
    })
) $Global:LogStyles.EmphasisColour

. $runnerPath `
  -Action $Action `
  -ModulePath "$($PSScriptRoot)" `
  -Workspace $Env `
  -BackendConfig $backendConfig `
  -TfvarsFiles $tfvarsFiles `
  -TerraformVersion $TerraformVersion `
  -Target $Target `
  -LockId $LockId `
  -AutoApprove:$AutoApprove `
  -UsePlanFile:$UsePlanFile `
  -PlanMode $PlanMode `
  -SkipStateCleanup:$SkipStateCleanup `
  -ClearCache:$ClearCache

