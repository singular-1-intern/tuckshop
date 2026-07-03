<#
.SYNOPSIS
  Runner script which can execute a variety of Terraform operations
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
.PARAMETER ModulePath
  The path to the Terraform module to run the action against.
.PARAMETER Workspace
  The workspace to run the action against. The workspace will be created if it does not exist.
.PARAMETER BackendConfig
  A hashtable containing backend configuration values to be passed to Terraform. These will be passed as '-backend-config=key=value' arguments. (Optional)
.PARAMETER TfvarsFiles
  An array of .tfvars files to pass to Terraform. (Optional)
.PARAMETER TerraformVersion
  The version of Terraform to use. If not specified, the version of Terraform on the path will be used. (Optional)
.PARAMETER TerraformPath
  The path to the Terraform binary. If not specified, the Terraform binary on the path or the version specified in TerraformVersion will be used.
  If the NeoTerraformPath environment variable is set, it will be used as the default for this parameter. (Optional)
.PARAMETER Target
  When running an apply, this parameter can be used to target a specific resource.
.PARAMETER ResourceAddress
  When using the 'import' action, this is the Address of the resource to import.
.PARAMETER ResourceId
  When using the 'import' action, this is the ID of the resource to import.
.PARAMETER PlanFile
  The name used if a plan file is generated. It is recommended that this be left as the default value so that plans file do not get accidentally
  committed to source control. (Default: tfplan)
.PARAMETER PlanMode
  The mode to use when running the 'plan' operation
    apply        : Default mode. Generates a plan file to be used in an apply operation.
    refresh-only : Generates a plan file that only refreshes the state.
    destroy      : Generates a plan file that destroys all resources.
.PARAMETER LockId
  The Lock Id to release. Mandatory when running the 'force-unlock' action.
.PARAMETER AutoApprove
  Enable to auto approve the action. For use in automation scenarios.
.PARAMETER UsePlanFile
  Enable if you want to generate a plan file during the 'plan' action, or use a plan file during the 'apply' or 'destroy' actions.
.PARAMETER SkipStateCleanup
  Enable to skip state clean up after the action is run.
.PARAMETER ClearCache
  Enable to remove the .terraform folder after the module has run.
.PARAMETER GenerateTimestamp
  Enable to generate a timestamp and push it into any tfvars file(s) containing a 'current_timestamp' variable.
#>
[CmdletBinding()]
Param(
  [Parameter(Position = 0)][ValidateSet("plan", "show-plan", "apply", "upgrade", "destroy", "list-state", "output", "force-unlock", "import")][string]$Action = "apply",
  [Parameter(Position = 1, Mandatory)][string]$ModulePath,
  [Parameter(Position = 2, Mandatory)][string]$Workspace,
  [hashtable]$BackendConfig = @{},
  [string[]]$TfvarsFiles = @(),
  [string]$TerraformVersion,
  [string]$TerraformPath = $Env:NeoTerraformPath,
  [string]$Target = "",
  [string]$ResourceAddress = "",
  [string]$ResourceId = "",
  [string]$PlanFile = "tfplan",
  [ValidateSet("apply", "refresh-only", "destroy")][string]$PlanMode = "apply",
  [string]$LockId,
  [switch]$AutoApprove,
  [switch]$UsePlanFile,
  [switch]$SkipStateCleanup,
  [switch]$ClearCache,
  [switch]$GenerateTimestamp
)

# Locate the scripts root path and load libraries
$scriptsPath = $PSScriptRoot; while ($scriptsPath -and (Split-Path $scriptsPath -Leaf) -ne "scripts") { $scriptsPath = Split-Path $scriptsPath }
if (!$scriptsPath) { throw "Could not locate the scripts root folder. Please ensure this script is nested beneath a 'scripts' parent folder." }
. (Join-Path $scriptsPath "libraries/common.ps1")

# Configure context for the runtime environment if a script is present.
$contextScript = "$scriptsPath/context/configure-runtime-context.ps1"
if (Test-Path $contextScript) {
  & $contextScript
}

# We cannot just use $ErrorActionPreference = "Stop" because Powershell then kills the terraform run immediately when an error occurs,
# which then doesn't allow terraform to exit cleanly, i.e. Release state locks.
$ErrorActionPreference = "Continue"
$InformationPreference = "Continue"

Write-Logs @(
  "`n",
  "▀█▀ █▀▀ █▀▄ █▀▄ █▀█ █▀▀ █▀█ █▀▄ █▄█   █▀▄ █ █ █▀█ █▀█ █▀▀ █▀▄",
  " █  █▀▀ █▀▄ █▀▄ █▀█ █▀▀ █ █ █▀▄ █ █   █▀▄ █ █ █ █ █ █ █▀▀ █▀▄",
  " ▀  ▀▀▀ ▀ ▀ ▀ ▀ ▀ ▀ ▀   ▀▀▀ ▀ ▀ ▀ ▀   ▀ ▀ ▀▀▀ ▀ ▀ ▀ ▀ ▀▀▀ ▀ ▀",
  ""
) $Global:LogStyles.Heading1Colour

# Linux vs Windows defaults for Terraform path and binary
# (On linux, if we specify 'terraform.exe' as the binary, it causes problems with provider installation and path generation)
$terraformBasePath = $IsLinux ? "~/apps/terraform" : "C:\Apps\Terraform"
$terraformBinary = $IsLinux ? "terraform" : "terraform.exe"

# Ensure the correct terraform binary is available
Write-Log "`nTerraform version" $Global:LogStyles.Heading1Colour

# If not Terraform path is specified, automatically resolve one, and install the correct Terraform version if required.
if ([string]::IsNullOrEmpty($TerraformPath)) {
  $terraformPath = $terraformBinary
  if (![string]::IsNullOrEmpty($TerraformVersion)) {
    $terraformPath = (Join-Path $terraformBasePath $TerraformVersion $terraformBinary)
    if (!(Test-Path $terraformPath)) {
      $terraformFile = $IsLinux ? "terraform_$($TerraformVersion)_linux_amd64.zip" : "terraform_$($TerraformVersion)_windows_amd64.zip"
      $terraformUrl = "https://releases.hashicorp.com/terraform/$TerraformVersion/$terraformFile"

      $terraformVersionPath = (Join-Path $terraformBasePath $TerraformVersion)

      Write-Log "Required terraform version not found. Downloading v$TerraformVersion..."
      New-Item $terraformVersionPath -ItemType Directory -Force | Out-Null
      Invoke-WebRequest $terraformUrl -OutFile "$terraformVersionPath/terraform.zip"
      Expand-Archive "$terraformVersionPath/terraform.zip" -DestinationPath $terraformVersionPath -Force

      $terraformPath = (Join-Path $terraformVersionPath $terraformBinary)
    }
  }
}

$terraformVersionInfo = (& $terraformPath --version -json) | ConvertFrom-Json
Write-Log "Installed version is $($terraformVersionInfo.terraform_version)"

# Prepare Terraform arguments
Write-Log "`nValidate Inputs" $Global:LogStyles.Heading1Colour

# Module Path
Write-Log "Verify terraform module path"
if (!(Test-Path $ModulePath)) {
  throw "Module path not found: $ModulePath"
}

# Force-Unlock action
if (($Action -eq "force-unlock") -and ([string]::IsNullOrEmpty($LockId))) {
  throw "LockId is required when running the 'force-unlock' action"
}

# Backend Arguments
Write-Log "Preparing backend arguments"
$backendArguments = $BackendConfig.GetEnumerator() |
  ForEach-Object { "-backend-config=$($_.Name)=$($_.Value)" } |
  Join-String -Separator " "

Write-Debug "backendArguments: $backendArguments"

# Tfvars Files
Write-Log "Validating .tfvars files"
$tfvarsArguments = $TfvarsFiles | ForEach-Object {
  if (!(Test-Path "$(Join-Path $ModulePath $_)")) { throw "Tfvars file not found: $_" }
  return "-var-file=$_"
} | Join-String -Separator " "
Write-Debug "tfvarsArguments: $tfvarsArguments"

# Output Run Configuration
Write-Log "`nRun Configuration" $Global:LogStyles.Heading1Colour
Write-Logs (Format-KeyValues ([ordered]@{
      "Module"             = (Split-Path $ModulePath -Leaf)
      "Action"             = $Action
      "Plan Mode"          = $PlanMode
      "Workspace"          = $Workspace
      "Auto Approve"       = ($AutoApprove ? "Yes" : "No")
      "Use Plan File"      = ($UsePlanFile ? "Yes ($PlanFile)" : "No")
      "Skip State Cleanup" = ($SkipStateCleanup ? 'Yes' : 'No')
      "Clear Cache"        = ($ClearCache ? 'Yes' : 'No')
      "Generate Timestamp" = ($GenerateTimestamp ? 'Yes' : 'No')
      "TerraformVersion"   = $TerraformVersion
      "TerraformPath"      = $terraformPath
      "ModulePath"         = $ModulePath
    })) $Global:LogStyles.EmphasisColour

Write-Log "`nBackend Config $($Global:LogStyles.SymbolColour)= {" $Global:LogStyles.EmphasisColour
$BackendConfig.GetEnumerator() | ForEach-Object { Write-Log (Format-KeyValue "  $($_.Name)" $_.Value 23) }
Write-Log "}" $Global:LogStyles.SymbolColour

Write-Log "`nTerraform Vars files $($Global:LogStyles.SymbolColour)= [" $Global:LogStyles.EmphasisColour
$TfvarsFiles.Trim() -Split " " | ForEach-Object { Write-Log "  $($Global:LogStyles.TextColour)$_" }
Write-Log "]" $Global:LogStyles.SymbolColour

# Get confirmation from the user if we are running against a production environment
$productionEnvironments = @("prd", "dr")
$isProductionEnvironment = $false
foreach ($productionEnvironment in $productionEnvironments) {
  if (($Workspace.ToLower() -eq $productionEnvironment) -or ($Workspace.ToLower().EndsWith("-$productionEnvironment"))) {
    $isProductionEnvironment = $true
  }
}

if (($isProductionEnvironment) -and ($AutoApprove -eq $false)) {
  if (!(Show-ConfirmationMessage "`nYou are about to run against a production environment.")) {
    Write-Log "Exiting..."
    exit
  }
}

try {
  # Ensure that the working directory is the module path
  Push-Location $ModulePath

  # Initialise Terraform
  Write-Log "`nTerraform Module Initialisation" $Global:LogStyles.Heading1Colour

  # Check if the selected workspace matches the environment. If not, clear cached state
  $statePath = (Join-Path $ModulePath ".terraform")
  if ($true -eq (Test-Path $statePath)) {
    if (Test-Path (Join-Path $statePath "environment")) {
      $currentWorkspace = (Get-Content (Join-Path $statePath "environment") -Raw).Trim()
      if ($currentWorkspace -ne $Workspace) {
        Write-Log "Selected workspace '$Workspace' doesn't match state workspace '$currentWorkspace'. Clearing cached state."
        Remove-Item -Force (Join-Path $statePath "environment")
        Remove-Item -Force (Join-Path $statePath "terraform.tfstate")

        if (Test-Path $PlanFile) {
          Write-Log "Removing plan file from previous run: $PlanFile"
          Remove-Item $PlanFile
        }
      }
    }
  }

  # Generate a timestamp from the current date and time, then push it into the tfvars file(s)
  # (Terraform doesn't support generation of a 'now' timestamp as an input variable's default)
  if ($GenerateTimestamp) {
    # Assume the first tfvars file is the base file
    $baseTfVarsFile = $TfvarsFiles[0]

    Write-Log "`nSetting 'current_timestamp' in tfvars file to current date and time"
    $CurrentTimestamp = (Get-Date).ToString("yyyy-MM-ddThh:mm:ss.fffZ")
    Write-Log "Timestamp: $CurrentTimestamp"
    $regex = "current_timestamp =[ a-zA-Z0-9`"-:]+"
    $config = (Get-Content $baseTfVarsFile) -replace $regex, "current_timestamp = `"$CurrentTimestamp`""
    $config | Set-Content $baseTfVarsFile
  }

  # Upgrade action must run prior to an init
  if ($Action -eq "upgrade") {
    # Initialise and upgrade Terraform providers
    # (Ensure the lock file has references to both windows and linux providers)
    $upgradeArguments = "init -upgrade$(![string]::IsNullOrEmpty($backendArguments) ? " $backendArguments" : '') -migrate-state"
    & $terraformPath ($upgradeArguments.Split(" "))

    & $terraformPath providers lock -platform=windows_amd64 -platform=linux_amd64 -platform=darwin_amd64

    # Run an init to ensure all providers are installed
    Write-Log "Installing providers..."
    & $terraformPath init
    exit
  }

  # Terraform init command
  Write-Log "`nTerraform init" $Global:LogStyles.Heading1Colour
  $initArguments = "init -reconfigure $backendArguments".Trim()
  Write-Debug "Terraform init command: & $terraformPath $($initArguments.Split(" "))"
  & $terraformPath ($initArguments.Split(" "))

  if (!$?) {
    throw "Failed to initialise Terraform."
  }

  # Ensure that required workspace exists, and is selected
  Write-Log "`nSet Workspace" $Global:LogStyles.Heading1Colour
  Write-Log "Selecting workspace '$Workspace'"
  & $terraformPath ("workspace select $Workspace".Split(" "))
  if (!$?) {
    & $terraformPath ("workspace new $Workspace".Split(" "))

    if (!$?) {
      throw "Failed to create or select workspace '$Workspace'"
    }
  }

  # Make sure we're now on the correct workspace
  $currentWorkspace = (& $terraformPath ("workspace show".Split(" ")))
  if ($currentWorkspace -ne $Workspace) {
    throw "Failed to set workspace correctly. Expected '$Workspace', but got '$currentWorksapce'"
  }

  # Execute the Terraform action
  $terraformArguments = ""
  switch ($Action) {
    "plan" {
      Write-Log "`nRun Terraform Plan" $Global:LogStyles.Heading1Colour
      $terraformArguments = "plan"

      if ($PlanMode -eq "refresh-only") { $terraformArguments += " -refresh-only" }
      if ($PlanMode -eq "destroy") { $terraformArguments += " -destroy" }
      if ($UsePlanFile -and ![string]::IsNullOrEmpty($PlanFile)) {
        if (Test-Path $PlanFile) {
          Write-Log "Removing plan file from previous run: $PlanFile"
          Remove-Item $PlanFile
        }

        $terraformArguments += " -out=$PlanFile"
      }

      $terraformArguments += (![string]::IsNullOrEmpty($tfvarsArguments) ? " $tfvarsArguments" : "")
    }

    "show-plan" {
      Write-Log "`nShow Terraform Plan" $Global:LogStyles.Heading1Colour
      if (!(Test-Path $PlanFile)) { throw "No plan file found with name '$PlanFile'" }
      $terraformArguments = "show $PlanFile"
    }

    "apply" {
      Write-Log "`nRun Terraform Apply" $Global:LogStyles.Heading1Colour
      $terraformArguments = "apply"

      if (![string]::IsNullOrEmpty($Target)) {
        $terraformArguments += " -target=$Target"
      }

      if ($UsePlanFile) {
        if (!(Test-Path $PlanFile)) { throw "No plan file found with name '$PlanFile'" }
        $terraformArguments += " $PlanFile"
      } else {
        # When using a plan file, auto approve is implied, and the tfvars files are
        # planning options, so we don't need to pass those either.
        $terraformArguments += ($AutoApprove ? " -auto-approve" : "")
        $terraformArguments += (![string]::IsNullOrEmpty($tfvarsArguments) ? " $tfvarsArguments" : "")
      }
    }

    "destroy" {
      Write-Log "`nRun Terraform Destroy" $Global:LogStyles.Heading1Colour
      $terraformArguments = "destroy"
      $terraformArguments += ($AutoApprove ? " -auto-approve" : "")
      $terraformArguments += (![string]::IsNullOrEmpty($tfvarsArguments) ? " $tfvarsArguments" : "")
    }

    "list-state" {
      Write-Log "`nList Terraform State" $Global:LogStyles.Heading1Colour
      $terraformArguments = "state list"
    }

    "output" {
      Write-Log "`nShow Terraform Output" $Global:LogStyles.Heading1Colour
      $terraformArguments = "output"
    }

    "force-unlock" {
      Write-Log "`nTerraform Force Unlock" $Global:LogStyles.Heading1Colour
      $terraformArguments = "force-unlock $LockId"
    }

    "import" {
      Write-Log "`nTerraform Import" $Global:LogStyles.Heading1Colour
      if ([string]::IsNullOrEmpty($ResourceAddress)) {
        throw "ResourceAddress is required when running the 'import' action"
      }

      if ([string]::IsNullOrEmpty($ResourceId)) {
        throw "ResourceId is required when running the 'import' action"
      }

      $terraformArguments = "import"
      $terraformArguments += (![string]::IsNullOrEmpty($tfvarsArguments) ? " $tfvarsArguments" : "")
      $terraformArguments += " $ResourceAddress $ResourceId"
    }

    default {
      throw "Action $Action not implemented"
    }
  }

  Write-Debug "Terraform Arguments: $terraformArguments"
  & $terraformPath $terraformArguments.Split(" ")

  # Using $? does not work as expected with terraform apply, so we need to use LastExitCode instead
  if ($LASTEXITCODE -ne 0) {
    throw "Error running Terraform action '$Action'. Scroll up to the terraform output for more information."
  }
} catch { 
  Write-Log "`nAn error occurred, aborting..." $Global:LogStyles.ErrorColour
  $errorMessage = $_.Exception.Message
} finally {
  Pop-Location

  if ($true -eq $SkipStateCleanup) {
    Write-Log "`nSkipping state cleanup"
  } else {
    Write-Log "`nCleanup State" $Global:LogStyles.Heading1Colour

    $tfLocalStatePath = (Join-Path $ModulePath "terraform.tfstate.d")
    if ($true -eq (Test-Path $tfLocalStatePath)) {
      $confirmation = Read-Host "A 'terraform.tfstate.d' state file was found. This is a local state file, and may be the only copy. Are you sure you want to delete it? [Y/n]"
      if ($confirmation -eq 'Y') {
        Write-Log "Removing local state..."
        Remove-Item -Recurse -Force $tfLocalStatePath
      }
    }

    $tfWorkspaceFile = (Join-Path $ModulePath ".terraform" "environment")
    if ($true -eq (Test-Path $tfWorkspaceFile)) {
      Write-Log "Removing workspace file..."
      Remove-Item -Force $tfWorkspaceFile
    }

    $tfRemoteStatePath = (Join-Path $ModulePath ".terraform" "terraform.tfstate")
    if ($true -eq (Test-Path $tfRemoteStatePath)) {
      Write-Log "Removing local copy of backend state..."
      Remove-Item -Recurse -Force $tfRemoteStatePath
    }

    Write-Log "State cleanup completed" $Global:LogStyles.EmphasisColour
  }

  if ($true -eq $ClearCache) {
    Write-Log "`nClear Cache" $Global:LogStyles.Heading1Colour
    $cacheFolder = (Join-Path $ModulePath ".terraform")
    if ($true -eq (Test-Path $cacheFolder)) {
      Write-Log "Removing cache folder..."
      Remove-Item -Recurse -Force $cacheFolder
    }

    Write-Log "Cache folder cleanup completed" $Global:LogStyles.EmphasisColour
  }

  # If we caught an error, display it here
  if (![string]::IsNullOrEmpty($errorMessage)) {
    Write-Log "`nAn error occurred:"
    Write-Log $errorMessage $Global:LogStyles.ErrorColour
    Write-Log ""

    # We need to re-throw an error here to ensure that the pipeline fails in the CI/CD system
    throw "Terraform provisioning failed."
  } else {
    Write-Log "`nTerraform run completed`n" $Global:LogStyles.SuccessColour
  }
}

