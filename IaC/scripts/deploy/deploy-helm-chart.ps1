<#
.SYNOPSIS
  Deploys a helm chart to a Kubernetes Cluster
.PARAMETER ProjectPrefix
  The project prefix. This is used to generate the Kubernetes namespace and release name.
  (Defaults to the value of the `ProjectPrefix` environment variable)
.PARAMETER LocationPrefix
  The location prefix. This is used when generating infrastructure resource names.
  (Defaults to the value of the `LocationPrefix` environment variable)
.PARAMETER HostPrefix
  The host environment prefix. This is used when generating infrastructure resource names.
  (Defaults to the value of the `HostPrefix` environment variable)
.PARAMETER Environment
  The environment prefix. This is used when generating infrastructure resource names, as well
  as the namespace and release names.
  (Defaults to the value of the `EnvironmentPrefix` environment variable)
.PARAMETER AppName
  The name of the app. This is used when generating the release name, and in locating the
  deployment configuration files.
.PARAMETER Chart
  The name of the helm chart to deploy
  (Defaults to "./IaC/helm/neo-web-app")
.PARAMETER Namespace
  The Kubernetes namespace to deploy to
  (If not specified, defaults to "{ProjectPrefix}-{AppName}")
.PARAMETER Release
  The name of the release
  (If not specified, defaults to "{ProjectPrefix}-{Environment}-{AppName}")
.PARAMETER ValueFiles
  A comma (,) separated list of value file paths to pass to the chart. The script also checks
  for (and prefers) Scriban template (.sbn) versions of the files specified. (E.g. If you specify
  "app-config.yaml", the script will first check for "app-config.yaml.sbn" and use that if it exists.)

  If not specified, config files are search for as follows:
  - If folder exists: /IaC/config/apps/{$AppName}
    - /IaC/config/apps/{$AppName}/{$AppName}{.yaml|.yaml.sbn}
    - /IaC/config/apps/{$AppName}/{$AppName}.{$Environment}{.yaml|.yaml.sbn}
  - Otherwise:
    - /IaC/config/apps/{$AppName}{.yaml|.yaml.sbn}
    - /IaC/config/apps/{$AppName}.{$Environment}{.yaml|.yaml.sbn}

  Any .sbn files will be processed as Scriban templates, allowing you to inject IaC state, if
  there is any available.
.PARAMETER ImageTag
  The tag of the image to deploy (Optional. E.g. v5)
.PARAMETER GitHubToken
  The GitHub token to use to retrieve the profiles.json file directly from the source repository.
  This is used to retrieve the application resource configuration at deployment time instead of the copy that was packaged with the build.
  (Defaults to the value of the `GitHubToken` environment variable)
.PARAMETER GitHubRepository
  The GitHub repository to use to retrieve the profiles.json file.
  This is used to retrieve the application resource configuration at deployment time instead of the copy that was packaged with the build.
  (Defaults to the value of the `GitHubRepository` environment variable)
.PARAMETER GitHubRepositoryBranch
  The branch of the GitHub repository to use to retrieve the profiles.json file.
  This is used to retrieve the application resource configuration at deployment time instead of the copy that was packaged with the build.
  (Defaults to the value of the `GitHubRepositoryDeployBranch` environment variable, or "main" if not set)
.PARAMETER UseDefaultKubeConfig
  Use the default kube config. Use this for local testing and non-concurrent scenarios.
  Credentials for the cluster must be present in your kube config file.
.PARAMETER ShowTimings
  Show timings for each step of the script execution.
  (Optional. Defaults to false)
.PARAMETER SkipDeployment
  Skip the deployment of the helm chart.
  This is useful for testing your app deployment templates without actually deploying the chart.
  (Optional. Defaults to false)
.PARAMETER DryRun
  Perform a dry run where the helm command is executed with the `--dry-run` flag.
.PARAMETER HelmDebug
  Enable debug logging in helm.
  (Optional. Defaults to false)
.PARAMETER PreserveContext
  Preserve the global context after the script has completed. This is useful for debugging and testing.
  If this is set, the script will not clean up the global context at the end of execution.
  (Optional. Defaults to false)
#>
[CmdletBinding()]
Param(
  [string]$ProjectPrefix = $Env:ProjectPrefix,
  [string]$LocationPrefix = $Env:LocationPrefix,
  [string]$HostPrefix = $Env:HostPrefix,
  [string]$Environment = $Env:EnvironmentPrefix,
  [Parameter(Mandatory)][string]$AppName,
  [string]$Chart = "./IaC/helm/neo-web-app",
  [string]$Namespace,
  [string]$Release,
  [string]$ValueFiles = "",
  [string]$ImageTag = "",
  [string]$GitHubToken = $Env:GitHubToken,
  [string]$GitHubRepository = $Env:GitHubRepository,
  [string]$GitHubRepositoryDeployBranch = ($Env:GitHubRepositoryDeployBranch ?? "main"),
  [switch]$UseDefaultKubeConfig,
  [switch]$ShowTimings,
  [switch]$SkipDeployment,
  [switch]$DryRun,
  [switch]$HelmDebug,
  [switch]$PreserveContext
)

$ErrorActionPreference = "Stop"
$InformationPreference = "Continue"

# Locate the IaC root path and load libraries
$iacPath = $PSScriptRoot; while ($iacPath -and (Split-Path $iacPath -Leaf) -ne "IaC") { $iacPath = Split-Path $iacPath }
if (!$iacPath) { throw "Could not locate the IaC root folder. Please ensure this script is nested beneath an 'IaC' parent folder." }
$scriptsPath = (Join-Path $iacPath "scripts")
. (Join-Path $scriptsPath "libraries/common.ps1")
. (Join-Path $scriptsPath "libraries/templating.ps1")

Write-Logs @(
  "`n",
  " █▀▄ █▀▀ █▀█ █   █▀█ █ █   █ █ █▀▀ █   █▄█   █▀▀ █ █ █▀█ █▀▄ ▀█▀",
  " █ █ █▀▀ █▀▀ █   █ █  █    █▀█ █▀▀ █   █ █   █   █▀█ █▀█ █▀▄  █ ",
  " ▀▀  ▀▀▀ ▀   ▀▀▀ ▀▀▀  ▀    ▀ ▀ ▀▀▀ ▀▀▀ ▀ ▀   ▀▀▀ ▀ ▀ ▀ ▀ ▀ ▀  ▀ "
) $Global:LogStyles.Heading1Colour

$context = Get-GlobalContext -New:$(!$PreserveContext)

# If PreserveContext is enabled, and the context has been populated previously, check if the environment
# has changed. If it has, force a re-create of the context
if ($PreserveContext -and ($null -ne $context.Environment)) {
  if ($context.Environment.ProjectPrefix -ne $ProjectPrefix -or
    $context.Environment.LocationPrefix -ne $LocationPrefix -or
    $context.Environment.HostPrefix -ne $HostPrefix -or
    $context.Environment.Environment -ne $Environment) {
    Write-Log "`nEnvironment has changed. Re-creating context." $Global:LogStyles.WarningColour
    $context = Get-GlobalContext -New
  } else {
    Write-Log "`nEnvironment has not changed. Reusing context from a previous run." $Global:LogStyles.SuppressedColour
  }
}

try {
  # Set default values
  if ([string]::IsNullOrEmpty($Namespace)) { $Namespace = "$($ProjectPrefix.ToLower())-$($Environment.ToLower())" }
  if ([string]::IsNullOrEmpty($Release)) { $Release = "$($ProjectPrefix.ToLower())-$($Environment.ToLower())-$($AppName.ToLower())" }
  if ([string]::IsNullOrEmpty($ValueFiles)) {
    # Check if an application specific configuration folder exists
    $appConfigPath = (Join-Path $iacPath "config/apps/$($AppName.ToLower())")
    if (Test-Path $appConfigPath) {
      $ValueFiles = "./IaC/config/apps/$AppName/$AppName.yaml,./IaC/config/apps/$AppName/$AppName.$Environment.yaml"
    } else {
      $appConfigPath = (Join-Path $iacPath "config/apps")
      $ValueFiles = "./IaC/config/apps/$AppName.yaml,./IaC/config/apps/$AppName.$Environment.yaml"
    }
  }

  $stopwatch = [system.diagnostics.stopwatch]::StartNew()

  $nugetConfig = $Env:NugetConfig
  if (![string]::IsNullOrEmpty($nugetConfig)) {
    # Output the NugetConfig to a file in the user folder
    $nugetConfigPath = (Join-Path $Env:HOME "~NuGet.Config")
    $nugetConfig | Out-File -FilePath $nugetConfigPath -Encoding UTF8
    Write-Log "NuGet.Config written to '$nugetConfigPath'." $Global:LogStyles.WarningColour
  }

  # Populate the context if PreserveContext is disabled, or if the context has not been populated yet
  if (!$PreserveContext -or ($null -eq $context.Environment)) {
    $context.Add("Environment", (New-Context @{
          ProjectPrefix  = $ProjectPrefix
          LocationPrefix = $LocationPrefix
          HostPrefix     = $HostPrefix
          Environment    = $Environment
        }))

    # Configure context for the runtime environment if a script is present.
    $runtimeContextScript = "$scriptsPath/context/configure-runtime-context.ps1"
    if (Test-Path $runtimeContextScript) {
      & $runtimeContextScript
    }
    $runtimeContextTime = $stopwatch.Elapsed.TotalMilliseconds

    # Configure cloud provider context if a script is present.
    $cloudContextScript = "$scriptsPath/context/configure-cloud-context.ps1"
    if (Test-Path $cloudContextScript) {
      & $cloudContextScript
    }
    $cloudContextTime = $stopwatch.Elapsed.TotalMilliseconds

    # Configure state context if a script is present.
    $stateContextScript = "$scriptsPath/context/configure-state-context.ps1"
    if (Test-Path $stateContextScript) {
      & $stateContextScript -ProjectPrefix $ProjectPrefix -LocationPrefix $LocationPrefix -HostPrefix $HostPrefix -Environment $Environment
    }
    $stateContextTime = $stopwatch.Elapsed.TotalMilliseconds

    # Acquire a kube config for the cluster
    $kubeConfigScript = "$scriptsPath/context/configure-kubernetes-context.ps1"
    if ((Test-Path $kubeConfigScript) -and !$UseDefaultKubeConfig) {
      & $kubeConfigScript
    }
    $kubeConfigTime = $stopwatch.Elapsed.TotalMilliseconds
  }

  # Get the Environment Profiles file and extract the application resource configuration
  Write-Log "`n[ Environment Profile ]" $Global:LogStyles.Heading1Colour
  $profilesFilePath = "$iacPath/config/profiles.json"
  if (![string]::IsNullOrEmpty($GitHubToken) -and ![string]::IsNullOrEmpty($GitHubRepository)) {
    $repofilePath = "IaC/config/profiles.json"
    $getRepoContentUri = "https://api.github.com/repos/$GitHubRepository/contents/$($repofilePath)?ref=$GitHubRepositoryDeployBranch"
    $headers = @{
      "Accept"               = "application/vnd.github+json"
      "Authorization"        = "Bearer $GitHubToken"
      "X-GitHub-Api-Version" = "2022-11-28"
    }

    $response = Invoke-GetRequest $getRepoContentUri $headers
    if ($response.success) {
      $profilesFilePath = "$iacPath/config/~profiles.json"
      $profilesContent = [System.Text.Encoding]::UTF8.GetString([System.Convert]::FromBase64String($response.body.content))
      $profilesContent | Out-File $profilesFilePath

      Write-Log "Downloaded 'profiles.json' from GitHub repository '$GitHubRepository'."
    } else {
      Write-Log "Error downloading 'profiles.json' from GitHub repository '$GitHubRepository'. Using local copy instead." $Global:LogStyles.WarningColour
      Write-Log "(Response Code: $($response.responseCode), Error: '$($response.body)') " $Global:LogStyles.WarningColour
    }
  } else {
    Write-Log "No GitHub token or repository specified. Using local profiles.json file."
  }

  Write-Log "Profiles path: $(Join-Path "IaC" ([System.IO.Path]::GetRelativePath($iacPath, $profilesFilePath)))" $Global:LogStyles.SuppressedColour
  if (!(Test-Path $profilesFilePath)) {
    throw "Profiles file not found at '$profilesFilePath'. Please ensure it exists."
  }

  $configProfiles = (Get-Content $profilesFilePath -Raw | ConvertFrom-Json)
  $configProfile = $configProfiles.profiles | Where-Object { $_.environments -contains $($Environment.ToLower()) }
  if ($null -eq $configProfile) {
    throw "Config Profile for environment '$Environment' not found in profiles.json."
  }

  $appResources = $configProfile.appResources.$AppName
  if ($null -eq $appResources) {
    throw "App resources for '$AppName' not found in profiles.json"
  }

  $cpuRequest = $appResources.cpuRequest ?? "10m"
  $cpuLimit = $appResources.cpuLimit ?? $null
  $memoryRequest = $appResources.memoryRequest ?? "64Mi"
  $memoryLimit = $appResources.memoryLimit ?? "64Mi"
  $minReplicas = $appResources.minReplicas ?? 1
  $maxReplicas = $appResources.maxReplicas ?? $minReplicas
  $autoScaling = ($maxReplicas -gt $minReplicas)

  Write-Log "`nResources for '$($AppName)' App:" $Global:LogStyles.EmphasisColour
  Write-Log "  CPU Request    : $cpuRequest"
  Write-Log "  CPU Limit      : $($cpuLimit ?? "None")"
  Write-Log "  Memory Request : $memoryRequest"
  Write-Log "  Memory Limit   : $memoryLimit"

  if ($autoScaling) {
    Write-Log "  Auto Scaling   : Enabled"
    Write-Log "  Min Replicas   : $minReplicas"
    Write-Log "  Max Replicas   : $maxReplicas"
  } else {
    Write-Log "  Replicas       : $minReplicas"
  }

  $resourceArguments = @()
  if (![string]::IsNullOrEmpty($cpuRequest)) { $resourceArguments += "--set deployment.resources.requests.cpu=$cpuRequest" }
  if (![string]::IsNullOrEmpty($cpuLimit)) { $resourceArguments += "--set deployment.resources.limits.cpu=$cpuLimit" }
  if (![string]::IsNullOrEmpty($memoryRequest)) { $resourceArguments += "--set deployment.resources.requests.memory=$memoryRequest" }
  if (![string]::IsNullOrEmpty($memoryLimit)) { $resourceArguments += "--set deployment.resources.limits.memory=$memoryLimit" }
  if ($autoScaling) {
    $resourceArguments += "--set autoscaling.enabled=true"
    $resourceArguments += "--set autoscaling.minReplicas=$minReplicas"
    $resourceArguments += "--set autoscaling.maxReplicas=$maxReplicas"
  } else {
    $resourceArguments += "--set deployment.replicaCount=$minReplicas"
  }

  Write-Log "`n[ Helm Deployment ]" $Global:LogStyles.Heading1Colour

  # Install or upgrade the helm chart
  Write-Log "`nInstall Helm Chart" $Global:LogStyles.Heading2Colour
  Write-Logs (Format-KeyValues ([ordered]@{
        "Chart"     = $Chart
        "Namespace" = $Namespace
        "Release"   = $Release
        "ImageTag"  = $ImageTag
      })) $Global:LogStyles.EmphasisColour

  # Generate the helm value file argument(s)
  Write-Log "`nValues Files" $Global:LogStyles.Heading2Colour
  $valuesArgument = ""
  $valuesFilePaths = @()
  if (![string]::IsNullOrEmpty(($ValueFiles))) {
    $filePaths = ($ValueFiles -split ",")

    foreach ($valuesFilePath in $filePaths) {
      # First, check for a Scriban template version of the file
      $templateFilePath = "$($valuesFilePath).sbn"
      if (Test-Path $templateFilePath) {
        # Values File Template
        # Render and save the template with a "~" prefix"
        $basePath = (Split-Path $templateFilePath -Parent)
        $templateFileName = (Split-Path $templateFilePath -Leaf)
        $renderedTemplateFilePath = (Join-Path $basePath "~$($templateFileName.replace(".sbn", [string]::Empty))")

        $templateContent = Get-Content $templateFilePath -Raw
        Build-Template $templateContent $context.State | Out-File -FilePath $renderedTemplateFilePath
        $valuesFilePaths += $renderedTemplateFilePath

        Write-Log "Rendered Values file: $(Split-Path $renderedTemplateFilePath -Leaf)"
      } elseif (Test-Path $valuesFilePath) {
        # Static Values File
        # Add the file as-is to the values file paths
        # The file path is recreated to ensure it's in the OS-native format (I.e. Windows uses backslashes, whereas Linux uses forward slashes)
        $basePath = (Split-Path $valuesFilePath -Parent)
        $valuesFileName = (Split-Path $valuesFilePath -Leaf)
        $valuesFilePath = (Join-Path $basePath $valuesFileName)
        $valuesFilePaths += $valuesFilePath

        Write-Log "Static Values file  : $(Split-Path $valuesFilePath -Leaf)"
      }
    }

    $valuesArgument = ($valuesFilePaths | ForEach-Object { "--values $_" }) -Join " "
  }

  # Generate the helm arguments list
  $arguments = "upgrade --install $Release $Chart --namespace $Namespace $valuesArgument"
  if (![string]::IsNullOrEmpty($ImageTag)) {
    $arguments = $arguments + " --set deployment.image.tag=$ImageTag"
  }

  if ($resourceArguments.Length -gt 0) {
    $arguments = $arguments + " " + ($resourceArguments -join " ")
  }

  if ($DryRun) {
    $arguments = $arguments + " --dry-run"
  }

  if ($HelmDebug) {
    $arguments = $arguments + " --debug"
  }

  Write-Log "`nHelm Command" $Global:LogStyles.Heading2Colour
  $arguments.Split("--") | ForEach-Object { Write-Log "$($_.Trim() -ne "upgrade" ? '--' : 'helm ')$($_.Trim())" }

  if (!$SkipDeployment) {
    Write-Log "`nDeploying Helm Chart" $Global:LogStyles.Heading2Colour
    & helm ($arguments.Split(" "))

    if (-not $?) {
      throw "Error deploying helm chart"
    }
  } else {
    Write-Log "`nDeployment is disabled. Skipping Helm Chart Deployment." $Global:LogStyles.WarningColour
  }

  $helmDeployTime = $stopwatch.Elapsed.TotalMilliseconds

} finally {
  if ($ShowTimings) {
    Write-Log "`nExecution Timings:" $Global:LogStyles.Heading2Colour
    Write-Log "Configure Runtime Context    : $([int]$runtimeContextTime)ms"
    Write-Log "Configure Cloud Context      : $([int]($cloudContextTime - $runtimeContextTime))ms"
    Write-Log "Configure State Context      : $([int]($stateContextTime - $cloudContextTime))ms"
    Write-Log "Configure Kubernetes Context : $([int]($kubeConfigTime - $stateContextTime))ms"
    Write-Log "Deploy Helm Chart            : $([int]($helmDeployTime - $kubeConfigTime))ms"

    Write-Log "`nTotal Time Elapsed           : $([int]($stopwatch.Elapsed.TotalMilliseconds))ms"
  }

  $stopwatch.Stop()

  # Ensure the global context cleanup is run before exiting
  Write-Log "`nCleaning up Context" $Global:LogStyles.SuppressedColour
  if (!$PreserveContext) {
    $context.Cleanup()
    $Global:Context = $null
  } else {
    Write-Log "Preserve Context enabled. Skipping cleanup." $Global:LogStyles.WarningColour
  }
}

Write-Log "`nDeployment Successful`n" $Global:LogStyles.SuccessColour

