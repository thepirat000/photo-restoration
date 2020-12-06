# Set-ExecutionPolicy -Scope CurrentUser -ExecutionPolicy Bypass -Force;
Set-ExecutionPolicy Bypass -Scope Process -Force; 

# Download dotnet core runtime & hosting bundle
Write-Host "Download dotnet core runtime & hosting bundle", $PSScriptRoot -foregroundcolor "green";
$url = 'https://download.visualstudio.microsoft.com/download/pr/ff658e5a-c017-4a63-9ffe-e53865963848/15875eef1f0b8e25974846e4a4518135/dotnet-hosting-3.1.3-win.exe';
$file = $PSScriptRoot + '\dotnet-hosting-3.1.3-win.exe';
Start-BitsTransfer -Source $url -Destination $file

# Install dotnet core runtime & hosting bundle
Write-Host "Install dotnet core runtime & hosting bundle", $file -foregroundcolor "green";
& $file /install /passive 

#Enable IIS
Write-Host "Installing IIS features" -foregroundcolor "green";
Enable-WindowsOptionalFeature -Online -norestart -FeatureName IIS-WebServerRole
Enable-WindowsOptionalFeature -Online -norestart -FeatureName IIS-WebServer
Enable-WindowsOptionalFeature -Online -norestart -FeatureName IIS-CommonHttpFeatures
Enable-WindowsOptionalFeature -Online -norestart -FeatureName IIS-HttpErrors
Enable-WindowsOptionalFeature -Online -norestart -FeatureName IIS-HttpRedirect
Enable-WindowsOptionalFeature -Online -norestart -FeatureName IIS-ApplicationDevelopment
Enable-WindowsOptionalFeature -online -norestart -FeatureName NetFx4Extended-ASPNET45
Enable-WindowsOptionalFeature -Online -norestart -FeatureName IIS-NetFxExtensibility45
Enable-WindowsOptionalFeature -Online -norestart -FeatureName IIS-HealthAndDiagnostics
Enable-WindowsOptionalFeature -Online -norestart -FeatureName IIS-HttpLogging
Enable-WindowsOptionalFeature -Online -norestart -FeatureName IIS-LoggingLibraries
Enable-WindowsOptionalFeature -Online -norestart -FeatureName IIS-RequestMonitor
Enable-WindowsOptionalFeature -Online -norestart -FeatureName IIS-HttpTracing
Enable-WindowsOptionalFeature -Online -norestart -FeatureName IIS-Security
Enable-WindowsOptionalFeature -Online -norestart -FeatureName IIS-RequestFiltering
Enable-WindowsOptionalFeature -Online -norestart -FeatureName IIS-Performance
Enable-WindowsOptionalFeature -Online -norestart -FeatureName IIS-WebServerManagementTools
Enable-WindowsOptionalFeature -Online -norestart -FeatureName IIS-IIS6ManagementCompatibility
Enable-WindowsOptionalFeature -Online -norestart -FeatureName IIS-Metabase
Enable-WindowsOptionalFeature -Online -norestart -FeatureName IIS-ManagementConsole
Enable-WindowsOptionalFeature -Online -norestart -FeatureName IIS-BasicAuthentication
Enable-WindowsOptionalFeature -Online -norestart -FeatureName IIS-WindowsAuthentication
Enable-WindowsOptionalFeature -Online -norestart -FeatureName IIS-StaticContent
Enable-WindowsOptionalFeature -Online -norestart -FeatureName IIS-DefaultDocument
Enable-WindowsOptionalFeature -Online -norestart -FeatureName IIS-WebSockets
Enable-WindowsOptionalFeature -Online -norestart -FeatureName IIS-ApplicationInit
Enable-WindowsOptionalFeature -Online -norestart -FeatureName IIS-ISAPIExtensions
Enable-WindowsOptionalFeature -Online -norestart -FeatureName IIS-ISAPIFilter
Enable-WindowsOptionalFeature -Online -norestart -FeatureName IIS-HttpCompressionStatic
Enable-WindowsOptionalFeature -Online -norestart -FeatureName IIS-ASPNET45
Enable-WindowsOptionalFeature -Online -norestart -FeatureName IIS-ManagementService
net start WMSvc
choco install webdeploy -y --no-progress
choco install urlrewrite -y --no-progress

# Install dotnet core SDK
Write-Host "Install dotnet core SDK", $PSScriptRoot -foregroundcolor "green";
$url  = 'https://download.visualstudio.microsoft.com/download/pr/2e20b6fb-d05b-4dbc-bda1-5be5cba9e759/32bbf60d86a169acd5864b856e977ede/dotnet-sdk-3.1.103-win-x64.exe';
$file = $PSScriptRoot + '\dotnet-sdk-3.1.103-win-x64.exe';
Start-BitsTransfer -Source $url -Destination $file
& $file /install /passive
