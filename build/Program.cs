using Build;
using Cake.DotNetLocalTools.Module;
using Cake.Frosting;

return new CakeHost()
    .UseContext<BuildContext>()
    .UseModule<LocalToolsModule>()
    .InstallToolsFromManifest(".config/dotnet-tools.json")
    .Run(args);
