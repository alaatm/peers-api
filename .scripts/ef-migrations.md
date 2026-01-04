dotnet ef migrations add Initial -s ./src/Peers.Api -p ./src/Peers.Modules
dotnet ef migrations add ProductTypeLineageFunc -s ./src/Peers.Api -p ./src/Peers.Modules
dotnet ef dbcontext optimize -s ./src/Peers.Api -p ./src/Peers.Modules -o ./Kernel/CompiledModel --suffix ".g"

dotnet ef migrations has-pending-model-changes -s ./src/Peers.Api -p ./src/Peers.Modules

dotnet cake --target MigrationsBundle --configuration Release
az webapp deploy -g peers -n peers-api --src-path ./web.zip --type zip

./efbundle.exe --connection "Server=tcp:peers-sqlserver.database.windows.net,1433;Initial Catalog=peers-db;Persist Security Info=False;User ID=alaa;Password='Voilent987@Road;';MultipleActiveResultSets=True;Encrypt=False;Connection Timeout=30;encrypt=false;"


=================================

mkdir .efbundles
dotnet ef migrations add Initial -s ./src/Peers.Api -p ./src/Peers.Modules
dotnet ef migrations bundle -s ./src/Peers.Api -p ./src/Peers.Modules --configuration github.com.dotnet.efcore.issues.25555 --force
dotnet ef migrations bundle -s ../src/Peers.Api -p ../src/Peers.Modules --configuration Release --self-contained -r win-x64 << Latest

copy ../src/Peers.Api/appsettings*.json .

./efbundle.exe --connection "Server=.;Database=XXXX;Trusted_Connection=True;MultipleActiveResultSets=true;encrypt=false;"

dotnet cake --target MigrationsBundle --configuration Release
az webapp deploy -g peers -n peers-api --src-path ./web.zip --type zip

=====================================

 dotnet ef database update RegNumToOwnerId -s ./src/Peers.Api -p ./src/Peers.Modules
