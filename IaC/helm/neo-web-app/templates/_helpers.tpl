{{/* Standard Helm Chart Templates */}}

{{/*
Expand the name of the chart.
*/}}
{{- define "neo-web-app.name" -}}
{{- default .Chart.Name .Values.nameOverride | trunc 63 | trimSuffix "-" }}
{{- end }}

{{/*
Create a default fully qualified app name.
We truncate at 63 chars because some Kubernetes name fields are limited to this (by the DNS naming spec).
If release name contains chart name it will be used as a full name.
*/}}
{{- define "neo-web-app.fullname" -}}
{{- if .Values.fullnameOverride }}
{{- .Values.fullnameOverride | trunc 63 | trimSuffix "-" }}
{{- else }}
{{- $name := default .Chart.Name .Values.nameOverride }}
{{- if contains $name .Release.Name }}
{{- .Release.Name | trunc 63 | trimSuffix "-" }}
{{- else }}
{{- printf "%s-%s" .Release.Name $name | trunc 63 | trimSuffix "-" }}
{{- end }}
{{- end }}
{{- end }}

{{/*
Create chart name and version as used by the chart label.
*/}}
{{- define "neo-web-app.chart" -}}
{{- printf "%s-%s" .Chart.Name .Chart.Version | replace "+" "_" | trunc 63 | trimSuffix "-" }}
{{- end }}

{{/*
Common labels
*/}}
{{- define "neo-web-app.labels" -}}
helm.sh/chart: {{ include "neo-web-app.chart" . }}
{{ include "neo-web-app.selectorLabels" . }}
{{- if .Chart.AppVersion }}
app.kubernetes.io/version: {{ .Chart.AppVersion | quote }}
{{- end }}
app.kubernetes.io/managed-by: {{ .Release.Service }}
{{- end }}

{{/*
Selector labels
*/}}
{{- define "neo-web-app.selectorLabels" -}}
app.kubernetes.io/name: {{ include "neo-web-app.name" . }}
app.kubernetes.io/instance: {{ .Release.Name }}
app.singular.co.za/app-type: webapi
neo-app-type: webapi
{{- end }}

{{/* Custom Templates */}}


{{- define "neo-web-app.image" -}}
{{ printf "%s/%s:%s" .Values.deployment.image.registry .Values.deployment.image.repository (.Values.deployment.image.tag | default .Chart.AppVersion) }}
{{- end -}}

{{- define "neo-web-app.serviceAccountName" -}}
{{- if .Values.serviceAccount.name }}
{{- .Values.serviceAccount.name }}
{{- else }}
{{- printf "%s-%s" (include "neo-web-app.fullname" .) "service-account" }}
{{- end }}
{{- end -}}