class ScribanTemplateService {
  ScribanTemplateService() {

  }

  [Scriban.TemplateContext] CreateTemplateContext() {
    $templateContext = [Scriban.TemplateContext]::new()
    $templateContext.NewLine = [System.Environment]::NewLine
    $templateContext.AutoIndent = $false
    return $templateContext
  }

  [Scriban.Runtime.ScriptObject] CreateScriptObject([System.Dynamic.ExpandoObject]$Expando) {
    $expandoDictionary = [System.Collections.Generic.Dictionary[string, object]]$Expando
    $scriptObject = [Scriban.Runtime.ScriptObject]::new()

    foreach ($item in $expandoDictionary.GetEnumerator()) {
      if ($item.Value -is [System.Dynamic.ExpandoObject]) {
        $scriptObject.Add($item.Key, $this.CreateScriptObject($item.Value))
      } else {
        $scriptObject.Add($item.Key, $item.Value)
      }
    }

    return $scriptObject
  }

  [Scriban.Template] ParseTemplate([string]$Template) {
    $parsedTemplate = [Scriban.Template]::Parse($Template)

    if ($parsedTemplate.HasErrors) {
      $errorMessage = "Error parsing template: $($parsedTemplate.ErrorMessages)"
      throw $errorMessage
    }

    return $parsedTemplate
  }

  [string] RenderFromJson([string]$Template, [string]$Json) {
    $templateContext = $this.CreateTemplateContext()

    $expando = [Newtonsoft.Json.JsonConvert]::DeserializeObject[System.Dynamic.ExpandoObject]($Json)
    $scriptObject = $this.CreateScriptObject($expando)
    $templateContext.PushGlobal($scriptObject)

    $parsedTemplate = $this.ParseTemplate($Template)
    return $parsedTemplate.Render($templateContext)
  }

  [string] RenderFromPSObject([string]$Template, [PSObject]$Model) {
    $json = (ConvertTo-Json $Model -Depth 12)
    return $this.RenderFromJson($Template, $json)
  }
}

