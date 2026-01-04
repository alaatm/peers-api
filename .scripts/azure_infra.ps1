$ErrorActionPreference = 'Stop'                           # make PowerShell cmdlet errors stop the script
$PSNativeCommandUseErrorActionPreference = $true          # make non-zero exit codes from native tools (like az) throw
$VerbosePreference = 'Continue'                           # so -Verbose actually prints

$RG = "shiftlee"
$LOCATION = "germanywestcentral"
$PLAN = "$RG-plan"
$REGISTRY_APP = "$RG-registry"
$API_APP = "$RG-api"
$APPINSIGHTS_NAME = $API_APP
$MONITORING_WORKSPACE = "$RG-workspace"
$REGISTRY_SQL_SVR = "$REGISTRY_APP-sqlsvr"
$REGISTRY_SQL_SVR_ADMIN = "registry_admin"
$REGISTRY_SQL_SVR_PWD = "ofX8iA5n84Xq"
$REGISTRY_DB = "$REGISTRY_APP-db"
$API_SQL_SVR = "$API_APP-sqlsvr"
$API_SQL_SVR_ADMIN = "api_admin"
$API_SQL_SVR_PWD = "iU719GhX3VYo"
$API_STORAGE = ($API_APP -replace "_", "").ToLower() + "storage"

$REGISTRY_DB_CONN = "Server=tcp:$REGISTRY_SQL_SVR.database.windows.net,1433;Initial Catalog=$REGISTRY_DB;Persist Security Info=False;User ID=$REGISTRY_SQL_SVR_ADMIN;Password=$REGISTRY_SQL_SVR_PWD;MultipleActiveResultSets=True;Encrypt=False;TrustServerCertificate=False;Connection Timeout=30;"
$API_DB_CONN = "Server=tcp:$API_SQL_SVR.database.windows.net,1433;Initial Catalog=ShiftleeDev;Persist Security Info=False;User ID=$API_SQL_SVR_ADMIN;Password=$API_SQL_SVR_PWD;MultipleActiveResultSets=True;Encrypt=False;TrustServerCertificate=False;Connection Timeout=30;"

az group create -n $RG -l $LOCATION
az appservice plan create -g $RG -n $PLAN --sku p0v3 -l $LOCATION

az webapp create -g $RG -p $PLAN -n $REGISTRY_APP --runtime "dotnet:10"
az webapp update -g $RG -n $REGISTRY_APP `
    --set `
    clientAffinityEnabled=false `
    siteConfig.alwaysOn=true `
    siteConfig.use32BitWorkerProcess=false
$REGISTRY_APP_HOSTNAME = "https://" + (az webapp show -g $RG -n $REGISTRY_APP --query defaultHostName -o tsv)

az webapp create -g $RG -p $PLAN -n $API_APP --runtime "dotnet:10"
az webapp update -g $RG -n $API_APP `
    --set `
    clientAffinityEnabled=false `
    siteConfig.alwaysOn=true `
    siteConfig.webSocketsEnabled=true `
    siteConfig.use32BitWorkerProcess=false
$API_APP_HOSTNAME = "https://" + (az webapp show -g $RG -n $API_APP --query defaultHostName -o tsv)

#---- Registry database server & database ----
az sql server create -g $RG -n $REGISTRY_SQL_SVR -l $LOCATION -u $REGISTRY_SQL_SVR_ADMIN -p $REGISTRY_SQL_SVR_PWD
az sql server firewall-rule create -g $RG -s $REGISTRY_SQL_SVR -n AllowAzureServices --start-ip-address 0.0.0.0 --end-ip-address 0.0.0.0

$MY_IP = (Invoke-RestMethod -Uri "https://api.ipify.org").Trim()
az sql server firewall-rule create `
    -g $RG `
    -s $REGISTRY_SQL_SVR `
    -n "ClientIP_$($MY_IP -replace '\.','_')" `
    --start-ip-address $MY_IP `
    --end-ip-address $MY_IP
az sql db create -g $RG -s $REGISTRY_SQL_SVR -n $REGISTRY_DB --edition Basic --service-objective Basic

./efbundle.exe --connection "$REGISTRY_DB_CONN"
#---------------------------------------------

#---- API database server only ----
az sql server create -g $RG -n $API_SQL_SVR -l $LOCATION -u $API_SQL_SVR_ADMIN -p $API_SQL_SVR_PWD
az sql server firewall-rule create -g $RG -s $API_SQL_SVR -n AllowAzureServices --start-ip-address 0.0.0.0 --end-ip-address 0.0.0.0
#----------------------------------

#---- Allow Registry web app to create a database in the API database server ----
az webapp identity assign -g $RG -n $REGISTRY_APP
$PRINCIPAL_ID = az webapp identity show -g $RG -n $REGISTRY_APP --query principalId -o tsv
$SQL_SERVER_ID = az sql server show -g $RG -n $API_SQL_SVR --query id -o tsv
# Wait a moment for the managed identity's service principal to be visible
Start-Sleep -Seconds 10
az role assignment create --assignee-object-id $PRINCIPAL_ID --assignee-principal-type ServicePrincipal --role "SQL Server Contributor" --scope $SQL_SERVER_ID
#--------------------------------------------------------------------------------

#---- API Storage Account ----
az storage account create -n $API_STORAGE -g $RG -l $LOCATION --sku Standard_LRS --access-tier Hot --min-tls-version TLS1_2 --allow-shared-key-access true --allow-blob-public-access true
$STORAGE_CONN_STRING = az storage account show-connection-string -g $RG -n $API_STORAGE --query connectionString -o tsv
#-----------------------------

#---- Monitoring & App Insights ----
az monitor log-analytics workspace create `
    -g $RG `
    -n $MONITORING_WORKSPACE `
    -l $LOCATION `
    --sku PerGB2018

az monitor app-insights component create `
    -g $RG `
    -a $APPINSIGHTS_NAME `
    -l $LOCATION `
    --workspace $MONITORING_WORKSPACE `
    --application-type web `
    --kind web

$APPINSIGHTS_INSTRUMENTATION_KEY = az monitor app-insights component show -a $APPINSIGHTS_NAME -g $RG --query instrumentationKey -o tsv
$APPINSIGHTS_CONNECTION_STRING = az monitor app-insights component show -a $APPINSIGHTS_NAME -g $RG --query connectionString -o tsv
#--------------------------------

#---- Registry app settings ----
az webapp config connection-string set `
    -g $RG -n $REGISTRY_APP `
    --connection-string-type SQLAzure `
    --settings `
    Default="$REGISTRY_DB_CONN" `
    ShiftleeApi="$API_DB_CONN"
    
az webapp config appsettings set `
    -g $RG -n $REGISTRY_APP `
    --settings `
    azure:storageConnectionString="$STORAGE_CONN_STRING" `
    azureResource:SubscriptionId="509ae91c-1e41-451e-84fa-dd82d7696b67" `
    azureResource:ResourceGroupName="$RG" `
    azureResource:ServerName="$API_SQL_SVR" `
    azureResource:ElasticPoolName="none" `
    shiftlee:baseUrl="$API_APP_HOSTNAME" `
    shiftlee:clientId="Shiftlee.App" `
    shiftlee:clientSecret="Shiftlee.App.Password" `
    shiftlee:hostId="Shiftlee.Registry" `
    serviceAccount:key="8YhyTjhDFZzJYwHIBmVJJUWD6218rKWXLotgc21Nnu8=" `
    serviceAccount:clientId="Shiftlee.Registry" `
    serviceAccount:clientSecretHash="OEjzTypJgWDthzL/P87p1Z5lXN2auzLo15IwHjVLEwc=" `
    jwt:issuer="https://registry.shiftlee.co" `
    jwt:key="YTlLRlNoUHdpNWtMdlZhSDk5emU0REdDaDFxY0RMOVY=" `
    jwt:durationInMinutes="240" `
    sms:sender="Taqnyat.sa" `
    sms:key="89bb35824dcda139b0c2321e6bb5a979" `
    sms:enabled="false" `
    email:host="smtp.sendgrid.net" `
    email:username="apikey" `
    email:password="xxxxxx" `
    email:senderName="Shiftlee" `
    email:senderEmail="email@shiftlee.net" `
    email:port="465" `
    email:enableSsl="true" `
    email:enabled="false" `
    reverseProxy:Routes:ShiftleeProxy:ClusterId="ShiftleeCluster" `
    reverseProxy:Routes:ShiftleeProxy:Match:Path="/shiftlee_proxy/{**catchAll}" `
    reverseProxy:Routes:ShiftleeProxy:Transforms:0:PathRemovePrefix="/shiftlee_proxy" `
    reverseProxy:Clusters:ShiftleeCluster:Destinations:ShiftleeDestination:Address="$API_APP_HOSTNAME/"
#--------------------------------

#------- API app settings -------
az webapp config connection-string set `
    -g $RG -n $API_APP `
    --connection-string-type SQLAzure `
    --settings `
    Default="$API_DB_CONN"

az webapp config appsettings set `
    -g $RG -n $API_APP `
    --settings `
    APPINSIGHTS_INSTRUMENTATIONKEY="$APPINSIGHTS_INSTRUMENTATION_KEY" `
    APPLICATIONINSIGHTS_CONNECTION_STRING="$APPINSIGHTS_CONNECTION_STRING" `
    applicationInsights:connectionString="$APPINSIGHTS_CONNECTION_STRING" `
    azure:storageConnectionString="$STORAGE_CONN_STRING" `
    rateLimiting:perUserRateLimit:queueLimit="0" `
    rateLimiting:perUserRateLimit:tokenLimit="150" `
    rateLimiting:perUserRateLimit:tokensPerPeriod="150" `
    rateLimiting:perUserRateLimit:autoReplenishment="true" `
    rateLimiting:perUserRateLimit:replenishmentPeriod="60" `
    rateLimiting:anonRateLimit:queueLimit="0" `
    rateLimiting:anonRateLimit:tokenLimit="75" `
    rateLimiting:anonRateLimit:tokensPerPeriod="75" `
    rateLimiting:anonRateLimit:autoReplenishment="true" `
    rateLimiting:anonRateLimit:replenishmentPeriod="60" `
    rateLimiting:anonConcurrencyLimit:queueLimit="0" `
    rateLimiting:anonConcurrencyLimit:permitLimit="10" `
    jwt:issuer="https://shiftlee.co" `
    jwt:key="YTlLRlNoUHdpNWtMdlZhSDk3emU0REdDaDFxY0RMOVY=" `
    jwt:durationInMinutes="240" `
    health:privateMemory="768" `
    health:allocatedMemory="512" `
    health:workingSetMemory="1024" `
    registry:baseUrl="$REGISTRY_APP_HOSTNAME" `
    registry:clientId="Shiftlee.Registry" `
    registry:clientSecret="Shiftlee.Registry.Password" `
    registry:hostId="Shiftlee.Registry" `
    serviceAccount:key="3ja+sZ4U/HfK6RuJXimboWg9bvf3glXLqa1P4qQwrQk=" `
    serviceAccount:clientId="Shiftlee.App" `
    serviceAccount:clientSecretHash="NSSTLY3UD0pqDGNgkerrHcHzYKe6c7+ObyxIn5xcw2o="
#--------------------------------

# ---- Deploy web apps ----
az webapp deploy -g $RG -n $REGISTRY_APP --src-path ./Shiftlee.Registry.Server.zip --type zip
# give the Registry a moment to boot
Start-Sleep -Seconds 45
az webapp deploy -g $RG -n $API_APP --src-path ./Shiftlee.Api.zip --type zip
# -------------------------


# dotnet restore .\src\Shiftlee.Registry.Server\Shiftlee.Registry.Server.csproj -r win-x64
# dotnet publish .\src\Shiftlee.Registry.Server\Shiftlee.Registry.Server.csproj -c Release -r win-x64 --self-contained true -o .\artifacts\publish\win-x64
# dotnet restore .\src\Shiftlee.Api\Shiftlee.Api.csproj -r win-x64
# dotnet publish .\src\Shiftlee.Api\Shiftlee.Api.csproj -c Release -r win-x64 --self-contained true -o .\artifacts\publish\win-x64

# mkdir .efbundles
# cd .efbundles
# copy both appsettings.json and appsettings.Development.json files here from registry modules project
# dotnet ef migrations bundle -s ../src/Shiftlee.Registry.Server -p ../src/Shiftlee.Registry.Modules --context RegistryContext --configuration Release --self-contained -r win-x64
# ./efbundle.exe --connection "$REGISTRY_DB_CONN"