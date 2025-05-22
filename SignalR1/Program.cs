using Microsoft.AspNetCore.SignalR;
using SignalR1;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();
builder.Services.AddSingleton<RelaySessionManager>();
builder.Services.AddSingleton<IBackendChannelFactory, DummyBackendChannelFactory>();

var app = builder.Build();

app.UseDefaultFiles();  // index.html ‚ðŽ©“®“I‚É•\Ž¦
app.UseStaticFiles();

app.MapHub<RelayHub>( "/relay" );
app.MapGet( "/api/sessions/dump", ( RelaySessionManager manager ) =>
{
    var json = manager.DumpSessionsAsJson();
    return Results.Content( json, "application/json" );
} );

app.Run();
