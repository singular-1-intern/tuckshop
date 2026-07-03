# ┌───────────────────────────────────────────────────────────────────────────┐
# │ Templating Library                                                        │
# ├───────────────────────────────────────────────────────────────────────────┤
# │ Dependencies                                                              │
# │   Libraries : common.ps1                                                  │
# │   Tools     : none                                                        │
# |   Loads templating assemblies hosted at files.singular-cloud.com          │
# └───────────────────────────────────────────────────────────────────────────┘

$Global:TemplatingAssembliesLoaded = $false

<#
.SYNOPSIS
  Loads the templating assemblies from their remote location.
#>
function Import-TemplatingAssemblies() {
  [CmdletBinding()]
  param()

  if ($Global:TemplatingAssembliesLoaded) {
    return
  }

  $cacheFolder = "templating"
  $context = Get-GlobalContext
  if ($null -ne $context.Runtime -and $context.Runtime.CiCdAgentId -ne "default") {
    $cacheFolder = (Join-Path $cacheFolder $context.Runtime.CiCdAgentId)
  }

  Import-RemoteAssemblies @("Scriban.Signed.dll", "Newtonsoft.Json.dll") `
    -RemoteFolder "/binaries/templating" `
    -CacheFolder $cacheFolder

  $Global:TemplatingAssembliesLoaded = $true
}

<#
.SYNOPSIS
  Render a Scriban template using a model in PSObject format.
.PARAMETER Template
  The Scriban template to render.
.PARAMETER Model
  The model (in PSObject format) to use for rendering the template.
#>
function Build-Template() {
  [CmdletBinding()]
  [OutputType([string])]
  param(
    [Parameter(Mandatory, Position = 0)][string]$Template,
    [Parameter(Mandatory, Position = 1)][PSObject]$Model
  )

  # We defer the loading of assemblies until this function is called to allow for runtime context
  # to be set which may affect where the assemblies should be cached.
  Import-TemplatingAssemblies

  # The ScribanTemplateService class must be loaded after the templating assemblies, as it references
  # types from these assemblies.
  $scribanClassLoaded = ($null -ne ('ScribanTemplateService' -as [type]))
  if (-not $scribanClassLoaded) {
    . $PSScriptRoot/classes/scriban-template-service.ps1
  }

  $service = [ScribanTemplateService]::new()
  return $service.RenderFromPSObject($Template, $Model)
}

<#
.SYNOPSIS
  Render a Scriban template using a model in JSON format.
.PARAMETER Template
  The Scriban template to render.
.PARAMETER Json
  The model (in JSON string format) to use for rendering the template.
#>
function Build-TemplateFromJson() {
  [CmdletBinding()]
  [OutputType([string])]
  param(
    [Parameter(Mandatory, Position = 0)][string]$Template,
    [Parameter(Mandatory, Position = 1)][string]$Json
  )

  # We defer the loading of assemblies until this function is called to allow for runtime context
  # to be set which may affect where the assemblies should be cached.
  Import-TemplatingAssemblies

  # The ScribanTemplateService class must be loaded after the templating assemblies, as it references
  # types from these assemblies.
  $scribanClassLoaded = ($null -ne ('ScribanTemplateService' -as [type]))
  if (-not $scribanClassLoaded) {
    Write-Host "Loading ScribanTemplateService class."
    . $PSScriptRoot/classes/scriban-template-service.ps1
  }

  $service = [ScribanTemplateService]::new()
  return $service.RenderFromJson($Template, $Json)
}


