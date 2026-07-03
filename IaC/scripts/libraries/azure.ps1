# ┌───────────────────────────────────────────────────────────────────────────┐
# │ Azure Library                                                             │
# ├───────────────────────────────────────────────────────────────────────────┤
# │ Dependencies                                                              │
# │   Libraries : common.ps1                                                  │
# │   Tools     : Azure CLI                                                   │
# └───────────────────────────────────────────────────────────────────────────┘

<#
.SYNOPSIS
  Checks if there is a valid Azure CLI user login, and if so, returns a context object with the
  account, token, and user information.
.OUTPUTS
  If a user login is found, A hashtable with the azure context, otherwise $null.
  The context contains:
  - Account: The Azure account information (az account show)
  - Token: The Azure access token (az account get-access-token)
#>
function Get-AzureCliUserContext {
  [CmdletBinding()]
  [OutputType([hashtable])]

  $accountJson = (az account show --only-show-errors 2>$null)
  if ($?) {
    $account = ($accountJson | ConvertFrom-Json)
    $isUser = (($null -ne $account) -and ($account.user.type -eq "user"))
    if (!$isUser) { return $null }

    # Confirm the user has a valid, non-expired token
    $token = (az account get-access-token | ConvertFrom-Json)
    $tokenValid = ($null -ne $token -and [DateTime]$token.expiresOn -gt [DateTime]::Now)
    if (!$tokenValid) { return $null }

    return @{
      Account = $account
      Token   = $token
    }
  }
}

<#
.SYNOPSIS
  Checks for authenticated PowerShell Az contexts, and if so, returns an object containing information about the context(s).
  .PARAMETER SubscriptionId
    The Azure Subscription Id to filter contexts by. If not provided, the AZURE_SUBSCRIPTION_ID environment variable will be used.
.OUTPUTS
  If multiple contexts are found, an array of hashtables with the context information.
  If a single context is found, a single hashtable with the context information.
  If no contexts are found, $null.
#>
function Get-AzPowerShellContext {
  [CmdletBinding()]
  [OutputType([hashtable])]
  param(
    [Parameter(Position = 0)][string]$SubscriptionId = $Env:AZURE_SUBSCRIPTION_ID
  )

  if (-not $SubscriptionId) { throw "SubscriptionId parameter or AZURE_SUBSCRIPTION_ID environment variable must be set." }

  $azContexts = @(Get-AzContext)
  if (-not $?) { return $null }
  if (-not $azContexts) { return $null }

  [PSCustomObject]$azContext = $azContexts | Where-Object {
    $_.Subscription.Id -eq $SubscriptionId
  } | Select-Object -Property Account, Subscription, Tenant
    
  if (-not $azContext) { return $null }
  return $azContext
}

<#
.SYNOPSIS
  Retrieve the details of an Automation Account
  NOTE: At time of writing, this call was supported by Azure CLI, but the Automation extension was in preview, so we need to use the REST API directly.
.PARAMETER SubscriptionId
  The Azure Subscription Id
.PARAMETER ResourceGroup
  The Azure Resource Group name
.PARAMETER AutomationAccount
  The Azure Automation Account name
.PARAMETER AzToken
  The Azure Access Token to use for authentication with the Azure REST API
.OUTPUTS
  The Automation Account details as a PSCustomObject.
  If the Resource Group or Automation Account is not found, the function will return $null.
  If any other error occurs, the function will throw the error.
#>
function Get-AzureAutomationAccount {
  [CmdletBinding()]
  [OutputType([PSCustomObject])]
  Param(
    [string]$SubscriptionId,
    [string]$ResourceGroup,
    [string]$AutomationAccount,
    [PSCustomObject]$AzToken
  )

  # Prepare REST headers and URL
  $headers = @{
    "Authorization" = "Bearer $($azToken.accessToken)"
    "Content-Type"  = "application/json"
  }

  $url = "https://management.azure.com/subscriptions/$SubscriptionId/resourceGroups/$ResourceGroup/providers/Microsoft.Automation/automationAccounts/$($AutomationAccount)?api-version=2023-11-01"

  try {
    $response = Invoke-RestMethod -Uri $url -Method Get -Headers $headers
    Write-Host "response type: $($response.GetType())"
    return $response
  } catch {
    # The Status Code is an Enum, using the __ is a c# convention to get to its integer value
    $statusCode = $_.Exception.Response.StatusCode.Value__

    # Attempt to read the error response (We expect a JSON response, sometimes the API returns a plain text error message)
    $errorResponse = $null
    try {
      $errorResponse = ($_.ErrorDetails.Message | ConvertFrom-Json)
    } catch {
      # JSON parsing failed, we'll return the raw error details in the throw at the end of this function.
    }

    if ($statusCode -eq 404 -and $null -ne $errorResponse) {
      # This is done because the response isn't always consistently wrapped in an 'error' object
      $errorCode = $errorResponse.error.code ?? $errorResponse.code

      if ($errorCode -eq "ResourceGroupNotFound") {
        Write-Debug "Get-AzureAutomationAccount: Resource Group '$ResourceGroup' not found."
        return $null
      } elseif ($errorCode -eq "ResourceNotFound") {
        Write-Debug "Get-AzureAutomationAccount: Automation Account '$AutomationAccount' not found in Resource Group '$ResourceGroup'"
        return $null
      }
    }

    # If we couldn't identify the error, throw it with full details.
    $fullErrorMessage = @(
      "Error retrieving Automation Account '$AutomationAccount':",
      "Status Code: $statusCode",
      "Error Details: $($_.ErrorDetails.Message)",
      "Exception Message: $($_.Exception.Message)",
      "URL: $url"
    ) -join [System.Environment]::NewLine
    throw $fullErrorMessage
  }
}

<#
.SYNOPSIS
  Retrieve the value of an Automation Account variable
  NOTE: At time of writing, this call was unsupported by Azure CLI, so we need to use the REST API directly.
.PARAMETER SubscriptionId
  The Azure Subscription Id
.PARAMETER ResourceGroup
  The Azure Resource Group name
.PARAMETER AutomationAccount
  The Azure Automation Account name
.PARAMETER VariableName
  The name of the Automation Account variable to retrieve
.PARAMETER AzToken
  The Azure Access Token to use for authentication with the Azure REST API
.PARAMETER JsonDecode
  Decode the value of the variable as JSON. Use this if the variable's value is JSON encoded.
.OUTPUTS
  The Automation Account Variable as a PSCustomObject.
  If the Resource Group, Automation Account or Variable is not found, the function will return $null.
  If any other error occurs, the function will throw the error.
#>
function Get-AzureAutomationAccountVariable {
  [CmdletBinding()]
  [OutputType([PSCustomObject])]
  Param(
    [Parameter(Position = 0)][string]$SubscriptionId,
    [Parameter(Position = 1)][string]$ResourceGroup,
    [Parameter(Position = 2)][string]$AutomationAccount,
    [Parameter(Position = 3)][string]$VariableName,
    [Parameter(Position = 4)][PSCustomObject]$AzToken,
    [switch]$JsonDecode
  )

  # Prepare REST headers and URL
  $headers = @{
    "Authorization" = "Bearer $($azToken.accessToken)"
    "Content-Type"  = "application/json"
  }

  $url = "https://management.azure.com/subscriptions/$SubscriptionId/resourceGroups/$ResourceGroup/providers/Microsoft.Automation/automationAccounts/$AutomationAccount/variables/$($VariableName)?api-version=2023-11-01"

  try {
    $response = Invoke-RestMethod -Uri $url -Method Get -Headers $headers

    # The value property is JSON encoded by the API
    $variableValue = ($Response.properties.value | ConvertFrom-Json)

    # If the original value was also JSON encoded, we need to decode it again
    if ($JsonDecode) {
      $variableValue = ($variableValue | ConvertFrom-Json)
    }

    return $variableValue
  } catch {
    # The Status Code is an Enum, using the __ is a c# convention to get to its integer value
    $statusCode = $_.Exception.Response.StatusCode.Value__

    # Attempt to read the error response (We expect a JSON response, sometimes the API returns a plain text error message)
    $errorResponse = $null
    try {
      $errorResponse = ($_.ErrorDetails.Message | ConvertFrom-Json)
    } catch {
      # JSON parsing failed, we'll return the raw error details in the throw at the end of this function.
    }

    if ($statusCode -eq 404 -and $null -ne $errorResponse) {
      # This is done because the response isn't always consistently wrapped in an 'error' object
      $errorCode = $errorResponse.error.code ?? $errorResponse.code
      if ($errorCode -eq "ResourceGroupNotFound") {
        Write-Debug "Get-AzureAutomationAccountVariable: Resource Group '$ResourceGroup' not found."
        return $null
      } elseif ($errorCode -eq "ResourceNotFound") {
        Write-Debug "Get-AzureAutomationAccountVariable: Automation Account '$AutomationAccount' not found in Resource Group '$ResourceGroup'"
        return $null
      } elseif ($errorCode -eq "NotFound") {
        Write-Debug "Get-AzureAutomationAccountVariable: Variable '$VariableName' not found in Automation Account '$AutomationAccount'"
        return $null
      }
    }

    # If we couldn't identify the error, throw it with full details.
    $fullErrorMessage = @(
      "Error retrieving Automation Account '$AutomationAccount':",
      "Status Code: $statusCode",
      "Error Details: $($_.ErrorDetails.Message)",
      "Exception Message: $($_.Exception.Message)",
      "URL: $url"
    ) -join [System.Environment]::NewLine
    throw $fullErrorMessage
  }
}

