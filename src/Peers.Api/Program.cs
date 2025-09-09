using Peers.Modules.Kernel;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.ConfigureKestrel(p => p.AddServerHeader = false);

builder.Services.Configure<RouteHandlerOptions>(p => p.ThrowOnBadRequest = true);
builder.Services.AddPeers(builder.Configuration, builder.Environment);

var app = builder.Build();
app.UsePeers();

await app.RunAsync();

public partial class Program { }
