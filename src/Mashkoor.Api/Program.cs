
using Mashkoor.Modules.Kernel;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.ConfigureKestrel(p => p.AddServerHeader = false);

builder.Services.Configure<RouteHandlerOptions>(p => p.ThrowOnBadRequest = true);
builder.Services.AddMashkoor(builder.Configuration, builder.Environment);

var app = builder.Build();
app.UseMashkoor();

await app.RunAsync();

public partial class Program { }
