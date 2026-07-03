# Import 'localhost' certificates
# =============================================================================
$PfxFilePath = "$PSScriptRoot\neo-localhost.pfx"
$CerFilePath = "$PSScriptRoot\neo-localhost.cer"
$CertificatePassword = ConvertTo-SecureString -String "P@ssw0rd" -Force -AsPlainText

# Import the certificates
Import-PfxCertificate -filepath $PfxFilePath cert:\localmachine\my -password $CertificatePassword -exportable
Import-Certificate -FilePath $CerFilePath -CertStoreLocation Cert:\CurrentUser\Root

# Import 'neo-docker' certificates
# =============================================================================
$PfxFilePath = "$PSScriptRoot\neo-docker.pfx"
$CerFilePath = "$PSScriptRoot\neo-docker.cer"

# Import the certificates
Import-PfxCertificate -filepath $PfxFilePath cert:\localmachine\my -password $CertificatePassword -exportable
Import-Certificate -FilePath $CerFilePath -CertStoreLocation Cert:\CurrentUser\Root
