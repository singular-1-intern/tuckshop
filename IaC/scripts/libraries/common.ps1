# ┌───────────────────────────────────────────────────────────────────────────┐
# │ Common Library                                                            │
# ├───────────────────────────────────────────────────────────────────────────┤
# │ Dependencies                                                              │
# │   Libraries : none                                                        │
# │   Tools     : none                                                        │
# └───────────────────────────────────────────────────────────────────────────┘

# Globals
$Global:LogColours = $PSStyle.Foreground
$Global:DefaultLogType = "host"

$Global:LogStyles = @{
  Heading1Colour   = $Global:LogColours.BrightCyan
  Heading2Colour   = $Global:LogColours.Cyan
  TextColour       = $Global:LogColours.White
  EmphasisColour   = $Global:LogColours.BrightWhite
  SuppressedColour = $Global:LogColours.BrightBlack
  SymbolColour     = $Global:LogColours.BrightBlack
  WarningColour    = $Global:LogColours.BrightYellow
  ErrorColour      = $Global:LogColours.BrightRed
  SuccessColour    = $Global:LogColours.BrightGreen
}

<#
.SYNOPSIS
  Output a message to the terminal using more widely compatible colour output (I.e ANSI control codes)
.PARAMETER Message
  The message to output.
.PARAMETER Colour
  The colour to output the message in. (Optional, Defaults to $PSStyle.Foreground.White)
.PARAMETER NoNewLine
  Do not include a new line after the output. (Only supported with "host" log type)
#>
function Write-Log {
  Param(
    [Parameter(Position = 0)][string]$Message = "",
    [Parameter(Position = 1)][string]$Colour = $PSStyle.Foreground.White,
    [switch]$NoNewLine
  )

  # This fixes a bug where the GoCD console renderer doesn't handle newlines correctly if they are adjacent to an ASCII control code.
  # (If the GO_SERVER_URL environment variable has a value, it means we're running under a GoCD pipeline)
  if (![string]::IsNullOrEmpty($Env:GO_SERVER_URL)) {
    if ($Message.StartsWith("`n") -or $Message -eq "") { $Message = " " + $Message }
    if ($Message.EndsWith("`n")) { $Message += " " }
  }

  if ($Global:DefaultLogType -eq "info") {
    Write-Information "$($Colour)$($Message)$($PSStyle.Reset)"
  } else {
    Write-Host "$($Colour)$($Message)$($PSStyle.Reset)" -NoNewLine:$NoNewLine
  }
}

<#
.SYNOPSIS
  Outputs multiple log messages using the Write-Log function
.PARAMETER Messages
  The messages to output. (Required)
.PARAMETER Colour
  The colour to output the messages in. (Optional, Defaults to $PSStyle.Foreground.White)
#>
function Write-Logs {
  Param(
    [Parameter(Position = 0)][string[]]$Messages,
    [Parameter(Position = 1)][string]$Colour = $PSStyle.Foreground.White
  )

  foreach ($message in $Messages) {
    Write-Log $message $Colour
  }
}

<#
.SYNOPSIS
  Outputs a formatted key value pair with different colour styling of the Key, Separator and Value.
.PARAMETER Key
  The key name (Required)
.PARAMETER Value
  The value (Required)
.PARAMETER KeyColumnWidth
  The width of the key column. (Optional, Defaults to 25)
.PARAMETER Separator
  The separator character to use between the key and value. (Optional, Defaults to ":")
.PARAMETER ValueIfNull
  The value to use if the value is null. (Optional, Defaults to "[NULL]")
.OUTPUTS
  A styled and formatted key value pair.
#>
function Format-KeyValue {
  Param(
    [Parameter(Mandatory, Position = 0)][string]$Key,
    [Parameter(Position = 1)][string]$Value = $null,
    [Parameter(Position = 2)][int]$KeyColumnWidth = 25,
    [Parameter(Position = 3)][string]$Separator = ":",
    [string]$ValueIfNull = "[NULL]"
  )
  $Value = ($null -eq $Value) ? $ValueIfNull : $Value
  return "$($Key.PadRight($KeyColumnWidth))$($Global:LogStyles.SymbolColour)$($Separator) $($Global:LogStyles.TextColour)$Value"
}

<#
.SYNOPSIS
  Outputs a formatted collection of key value pairs with different colour styling of the Keys, Separators and Values.
.PARAMETER KeyValues
  The key value pairs to output. This must be provided as an ordered hashtable, the syntax is: [ordered]@{ ... } (Required)
.PARAMETER Separator
  The separator character to use between the key and value. (Optional, Defaults to ":")
.PARAMETER KeyColumnPadding
  The padding to add to the key column width (Width is auto calculated based on the longest key length). (Optional, Defaults to 2)
.PARAMETER ValueIfNull
  The value to use if the value is null. (Optional, Defaults to "[NULL]")
.OUTPUTS
  An array of styled and formatted key value pairs.
#>
function Format-KeyValues {
  Param(
    [Parameter(Mandatory, Position = 0)][System.Collections.Specialized.OrderedDictionary]$KeyValues,
    [Parameter(Position = 1)][string]$Separator = ":",
    [Parameter(Position = 2)][int]$KeyColumnPadding = 2,
    [string]$ValueIfNull = "[NULL]"
  )

  # Determine the key column width based on the length of the longest key
  $longestKey = 0
  $KeyValues.Keys | ForEach-Object { if ($_.Length -gt $longestKey) { $longestKey = $_.Length } }
  $keyColumnWidth = $longestKey + $KeyColumnPadding

  $formattedKeyValues = @()
  foreach ($key in $KeyValues.Keys) {
    $formattedKeyValues += Format-KeyValue -Key $key -Value $KeyValues[$key] -KeyColumnWidth $KeyColumnWidth -Separator $Separator -ValueIfNull $ValueIfNull
  }

  return $formattedKeyValues
}

<#
.SYNOPSIS
  Shows a confirmation message and waits for the user to confirm the action
.PARAMETER ActionMessage
  The message which explains what action is about to be taken. This is shown in front of the confirmation message. (Optional)
.PARAMETER ConfirmationMessage
  The message to display to the user to prompt for confirmation. (Optional, Defaults to 'Are you sure you want to continue?')
.OUTPUTS
  A boolean indicating whether the user confirmed the action or not.
#>
function Show-ConfirmationMessage {
  [CmdletBinding()]
  [OutputType([bool])]
  Param(
    [Parameter(Position = 0)][string]$ActionMessage = "",
    [Parameter(Position = 1)][string]$ConfirmationMessage = "Are you sure you want to continue?"
  )

  if (![string]::IsNullOrWhiteSpace($Message)) {
    Write-Log "$message" $Global:LogStyles.EmphasisColour
  }

  if (![string]::IsNullOrWhiteSpace($ActionMessage)) {
    $ActionMessage += " "
  }

  Write-Log "$($Global:LogStyles.WarningColour)$ActionMessage$ConfirmationMessage [Y/n]: $($PSStyle.Reset)" -NoNewLine

  $confirmation = Read-Host
  if ($confirmation -cne 'Y') {
    return $false
  }

  return $true
}

<#
.SYNOPSIS
  Asserts that a string value is not null or empty and throws an exception if it is.
.PARAMETER Value
  The value to check. (Required)
.PARAMETER ErrorMessage
  The error message to throw if the value is null or empty. (Required)
#>
function Assert-HasValue {
  Param(
    [Parameter(Position = 0)][string]$Value,
    [Parameter(Position = 1)][string]$ErrorMessage
  )

  if ([string]::IsNullOrWhiteSpace($Value)) {
    throw $ErrorMessage
  }
}

<#
.SYNOPSIS
  Downloads assemblies hosted online, saves them to an assemblies cache and then loads them into the current AppDomain.
.PARAMETER AssemblyNames
  The assemblies to load (Required)
.PARAMETER RemoteUrl
  The Base URL to download the assemblies from. (Optional, Defaults to https://files.singular-devops.com)
.PARAMETER RemoteFolder
  The Folder under the remote URL to download the assemblies from. (Optional, Defaults to 'binaries')
.PARAMETER CacheFolder
  The Folder in the cache under which to save the assemblies. (Optional, Defaults to "", I.e. The base of the cache folder)
.PARAMETER HoursToCache
  The number of hours to cache the assemblies for before checking for updates again. (Optional, Defaults to 24)
.PARAMETER SkipMinimumVersionCheck
  If specified, skips the minimum version check when loading assemblies. (Optional, Defaults to false)
  Use this if your logic is tolerant of potentially using an older version which may have been loaded into the AppDomain already.
#>
function Import-RemoteAssemblies {
  [CmdletBinding()]
  param(
    [Parameter(Position = 0)][string[]]$AssemblyNames,
    [string]$RemoteUrl = "https://files.singular-devops.com",
    [string]$RemoteFolder = "binaries",
    [string]$CacheFolder = "",
    [int]$HoursToCache = 24,
    [switch]$SkipMinimumVersionCheck
  )

  $baseUrl = "$($RemoteUrl.TrimEnd('/'))/$($RemoteFolder.TrimStart("/"))"

  # Settings
  $culture = [System.Globalization.CultureInfo]::InvariantCulture
  $dateFormat = "yyyy-MM-dd HH:mm:ss"

  # Ensure the Assemblies Cache folder exists
  $assembliesCachePath = (Join-Path $HOME ".neo" "assembly-cache" $CacheFolder)
  if (!(Test-Path $assembliesCachePath)) {
    New-Item -Path $assembliesCachePath -ItemType Directory | Out-Null
  }

  # Initialise the metadata cache
  $cacheMetadataPath = (Join-Path $assembliesCachePath "cache-metadata.json")
  if (Test-Path $cacheMetadataPath) {
    # Load the cache metadata and then convert the ETags PSObject back to a hashtable
    $cacheMetadata = Get-Content -Path $cacheMetadataPath -Raw | ConvertFrom-Json
    $eTagsCache = @{}
    $cacheMetadata.ETags.PSObject.Properties | ForEach-Object { $eTagsCache[$_.Name] = $_.Value }
    $cacheMetadata.ETags = $eTagsCache
  } else {
    $cacheMetadata = @{
      LastUpdated = (Get-Date).ToString($dateFormat)
      ETags       = @{}
    }
  }

  # Check if we should force an update of the assemblies (We do one update check per day)
  $now = (Get-Date)
  $lastUpdated = [datetime]::ParseExact($cacheMetadata.LastUpdated, $dateFormat, $culture)
  $timeSpan = New-TimeSpan -Start $lastUpdated -End $now
  $runUpdate = ($timeSpan.TotalHours -ge $HoursToCache)

  if ($runUpdate) {
    Write-Log "Loading Remote Assemblies" $Global:LogStyles.EmphasisColour
  } else {
    Write-Verbose "Loading Remote Assemblies"
  }

  foreach ($assembly in $AssemblyNames) {
    $assemblyUrl = "$baseUrl/$assembly"

    # ETags are only checked if we're running an update
    $eTagsChanged = $false
    if ($runUpdate) {
      # Before downloading, check if the assembly has changed by checking the ETag
      $headersResponse = Invoke-WebRequest -Uri $assemblyUrl -Method Head

      # In some scenarios, it's possible for more than one ETag to be returned, so we normalise the list.
      $eTags = ($headersResponse.Headers["ETag"] | ForEach-Object { $_.Trim().Trim("`"") } | Sort-Object -Unique) -join ","

      # Get the cached ETag for the assembly and then update it to the latest ETag returned by the server
      $eTagsFromCache = $cacheMetadata.ETags[$assembly]
      $cacheMetadata.ETags[$assembly] = $eTags

      $eTagsChanged = ($eTags -ne $eTagsFromCache)
      if ($eTagsChanged) {
        Write-Log "eTag Changed: $eTagsFromCache -> $eTags" $Global:LogStyles.SuppressedColour
      }

      # Even if an ETag hasn't changed, we still consider this as having run an update.
      $cacheMetadata.LastUpdated = (Get-Date).ToString($dateFormat)
    }

    $assemblyPath = (Join-Path $assembliesCachePath $assembly)
    $assemblyExists = (Test-Path $assemblyPath)
    if ((!$assemblyExists) -or ($eTagsChanged)) {
      Write-Log "Downloading assembly to cache: $(Join-Path $CacheFolder $assembly)" $Global:LogStyles.SuppressedColour
      Invoke-WebRequest -Uri $assemblyUrl -OutFile $assemblyPath
      $cacheMetadata.LastUpdated = (Get-Date).ToString($dateFormat)
    } else {
      Write-Verbose "Assembly $assembly is up to date."
    }

    # Load the Assembly into the current AppDomain if it hasn't been loaded already
    $assemblyName = (Split-Path $assemblyPath.ToLower() -Leaf)
    $loadedAssemblies = [AppDomain]::CurrentDomain.GetAssemblies() | Where-Object { ([string]::IsNullOrWhiteSpace($_.Location) ? "" : (Split-Path $_.Location.ToLower() -Leaf)) -eq $assemblyName }
    if ($loadedAssemblies.Count -eq 0) {
      Write-Verbose "Loading Assembly: $assemblyPath"
      Add-Type -Path $assemblyPath
    } else {
      if (!$SkipMinimumVersionCheck) {
        $loadedAssembly = $loadedAssemblies[0]
        # Is the loaded assembly's location different from the one we want to load?
        if ($loadedAssembly.Location.ToLower() -ne $assemblyPath.ToLower()) {
          # If so, check if the loaded assembly's version is lower than the version of the assembly we want to load.
          $loadedAssemblyVersion = $loadedAssembly.GetName().Version
          $assemblyVersion = [System.Reflection.AssemblyName]::GetAssemblyName($assemblyPath).Version
          if ($loadedAssemblyVersion -lt $assemblyVersion) {
            throw "Import-RemoteAssemblies Error: An instance of the '$($loadedAssembly.FullName)' assembly is already loaded into the AppDomain from location '$($loadedAssembly.Location)', but is a lower version ($loadedAssemblyVersion) than the version of the assembly being loaded by this script ($assemblyVersion). Since an assembly cannot be unloaded from the AppDomain, you will need to close this PowerShell session, start a new one, and then re-run this command."
          }
        }
      }

      Write-Verbose "Assembly already loaded: $assemblyPath"
    }
  }

  # Save the updated metadata cache to disk
  $cacheMetadata | ConvertTo-Json -Depth 12 | Set-Content -Path $cacheMetadataPath -Force
}

<#
.SYNOPSIS
  Create a context object with an optional cleanup method, and the ability to run the cleanup methods of any child contexts.
.PARAMETER Properties
  The properties to add to the context. (Optional)
.PARAMETER Cleanup
  The cleanup method for the context. (Optional)
#>
function New-Context {
  [CmdletBinding()]
  [OutputType([PSCustomObject])]
  param (
    [hashtable]$Properties = @{},
    [scriptblock]$Cleanup = $null
  )

  $context = [PSCustomObject]@{}

  # Add provided properties to the context
  foreach ($key in $Properties.Keys) {
    $context | Add-Member -MemberType NoteProperty -Name $key -Value $Properties[$key]
  }

  # Method: Add($name, $value, $replace)
  # Used to add additional properties to the context dynamically (including child contexts)
  $context | Add-Member -MemberType ScriptMethod -Name Add -Value {
    param($Name, $Value, $Replace = $false)

    # # Prevent overwrite of existing properties
    # if ($this.PSObject.Properties[$Name] -and !$Replace) {
    #   throw "Property '$Name' already exists on this context."
    # }

    $this | Add-Member -MemberType NoteProperty -Name $Name -Value $Value -Force
  }

  # Method: Cleanup()
  # Invoke the cleanup methods of any child contexts, and if defined, run the context's own cleanup method
  # (Debug output is handled explicitly because dynamic methods can't have CmdletBinding attributes)
  $context | Add-Member -MemberType ScriptMethod -Name Cleanup -Value {
    param($Debug = $false)
    if ($Debug) { Write-Log "== Cleaning up context ==" $Global:LogStyles.EmphasisColour }

    foreach ($property in $this.PSObject.Properties) {
      if ($property.Name -in @("Add", "Cleanup")) { continue }

      $child = $property.Value
      if ($child -is [psobject] -and $child.PSObject.Methods['Cleanup']) {
        if ($Debug) { Write-Log "Cleanup: '$($property.Name)'" $Global:LogStyles.SuppressedColour }
        $child.Cleanup($Debug)
      }
    }

    # Call this context’s own cleanup if defined
    if ($this.PSObject.Methods['OnCleanup']) {
      $this.OnCleanup()
    }
  }

  # Method: OnCleanup()
  # Optional cleanup method for the current context
  if ($Cleanup) {
    $context | Add-Member -MemberType ScriptMethod -Name OnCleanup -Value $Cleanup
  }

  return $context
}

<#
.SYNOPSIS
  Gets the global context object. If one doesn't exist, it is created.
.PARAMETER New
    If specified, the existing global context is replaced with a new one. (Optional, Defaults to false)
#>
function Get-GlobalContext {
  [CmdletBinding()]
  [OutputType([PSCustomObject])]
  param (
    [Parameter(Position = 0)][switch]$New
  )

  if ($Global:Context -and !$New.IsPresent) {
    Write-Verbose "Returning existing global context"
    return $Global:Context
  }

  Write-Verbose "Creating new global context"
  $Global:Context = New-Context @{} -Cleanup {
    $Global:Context = $null
    # Caller scripts might modify these Preference Variables, we just set them back to their PowerShell default values.
    $VerbosePreference = "SilentlyContinue"
    $DebugPreference = "SilentlyContinue"
    $InformationPreference = "SilentlyContinue"
    $ErrorActionPreference = "Continue"
  }
  return $Global:Context
}

<#
.SYNOPSIS
  Invokes a REST GET request to a specified URL and returns the response.
.PARAMETER Uri
  The URI to send the GET request to. (Required)
.PARAMETER Headers
  A hashtable of headers to include in the request. (Optional)
#>
function Invoke-GetRequest {
  [CmdletBinding()]
  param(
    [Parameter(Position = 0)][string]$Uri,
    [Parameter(Position = 1)][hashtable]$Headers = @{}
  )

  # Prepare the result object
  $result = @{
    success      = $true
    responseCode = 200
    body         = ""
  }

  try {
    $response = (Invoke-RestMethod -Uri $Uri -Headers $Headers -Method Get -ErrorAction Stop)
    $result.responseCode = $response.PSObject.Properties["statusCode"].Value ?? "200"
    $result.body = $response
  } catch {
    $result.success = $false
    $result.responseCode = [int]$_.Exception.Response.StatusCode
    $result.body = $_.Exception.Message
  }

  return $result
}

<#
.SYNOPSIS
  Extracts configuration values from terraform tfvars files.
.DESCRIPTION
  Scans the provided tfvars files, looking for the values of the provided keys.
  Later files override earlier values, so the list is processed in reverse order.
  Throws if a value for any of the keys cannot be found.
.PARAMETER TfvarsFiles
  The tfvars files to scan, in order of precedence (later files override earlier ones). (Required)
.PARAMETER BasePath
  The base path used to resolve relative tfvars paths. (Optional, defaults to the current location)
.PARAMETER Keys
  The keys to look for in the tfvars files. Nested properties can be accessed using dot notation
  (E.g. "azure.tenant_id"). (Required)
.OUTPUTS
  A hashtable containing the keys and their values.
#>

function Get-TerraformConfigValuesFromTfvars {
  Param(
    [Parameter(Mandatory, Position = 0)][string[]]$TfvarsFiles,
    [Parameter(Position = 1)][string]$BasePath = (Get-Location).Path,
    [Parameter(Mandatory, Position = 2)][string[]]$Keys
  )

  $missingValues = @()
  $results = @{}

  # The Latter Tfvars files override the values of preceding files, so we need to reverse the order of the array.
  $tfvarsFilesToScan = $TfvarsFiles.Clone()
  [array]::Reverse($TfvarsFilesToScan)


  # Support generic search for nested properties (E.g. property1.property2.property3).
  # Regex pattern: property1\s*=\s*\{[^}]*property2\s*=\s*\{[^}]*property3\s*=\s*"([^"]+)"
  foreach ($key in $Keys) {
    $valueFound = $false

    # Regex breakdown:
    #   $key\s*=\s*\{     - Matches '$key = {' with optional whitespace
    #   [^}]*             - Matches any content within the azure block up to the next '}'
    #   "([^"]+)"         - Captures the quoted value (group 1)
    # NOTE: This regex can get the wrong value  if a property with the same name is nested within a block above the target property.
    #       Solving this requires more sophisticated parsing, so we'll only implement this if we hit a scenario where this is an issue.
    $pattern = ($key.Split('.') | ForEach-Object { [regex]::Escape($_) }) -join '\s*=\s*\{[^}]*\s+'
    $pattern += '\s*=\s*"([^"]+)"'
    foreach ($tfvarsFile in $TfvarsFilesToScan) {
      $fullTfvarsPath = Join-Path $BasePath $tfvarsFile

      if (Test-Path $fullTfvarsPath) {
        $tfvarsContent = Get-Content $fullTfvarsPath -Raw
        if ($tfvarsContent -match $pattern) {
          $value = $Matches[1]
          $results[$key] = $value
          $valueFound = $true
          break
        }
      }
    }

    if (-not $valueFound) {
      $missingValues += $key
    }
  }

  if ($missingValues.Count -gt 0) {
    $searchedList = ($TfvarsFiles | ForEach-Object { "$_" }) -join ", "
    throw "Could not find the following values in tfvars files: $($missingValues -join ', ') (Searched:`n$searchedList)"
  }

  return $results
}

<#
.SYNOPSIS
  DEPRECATED FUNCTION - Use Get-TerraformConfigValuesFromTfvars instead.
  Extracts Azure tenant and subscription IDs from terraform tfvars files.
.DESCRIPTION
  Scans the provided tfvars files, looking for azure.tenant_id and azure.subscription_id values inside
  the azure block. Later files override earlier values, so the list is processed in reverse order.
  Throws if either value cannot be found.
.PARAMETER TfvarsFiles
  The tfvars files to scan, in order of precedence (later files override earlier ones). (Required)
.PARAMETER BasePath
  The base path used to resolve relative tfvars paths. (Optional, defaults to the current location)
.OUTPUTS
  PSCustomObject with TenantId and SubscriptionId properties.
#>
function Get-TerraformAzureIdsFromTfvars {
  [CmdletBinding()]
  [OutputType([PSCustomObject])]
  Param(
    [Parameter(Mandatory, Position = 0)][string[]]$TfvarsFiles,
    [Parameter(Position = 1)][string]$BasePath = (Get-Location).Path
  )

  $values = Get-TerraformConfigValuesFromTfvars $TfvarsFiles $BasePath @("azure.tenant_id", "azure.subscription_id")

  return [PSCustomObject]@{
    TenantId       = $values["azure.tenant_id"]
    SubscriptionId = $values["azure.subscription_id"]
  }
}

