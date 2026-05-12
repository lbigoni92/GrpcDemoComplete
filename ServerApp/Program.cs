using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddGrpc();
var app = builder.Build();
app.MapGrpcService<GreeterService>();
app.MapGet("/", () => "gRPC server is running...");
// Return 204 for favicon requests to avoid repeated 404 log entries
app.MapGet("/favicon.ico", () => Results.NoContent());
app.Run();