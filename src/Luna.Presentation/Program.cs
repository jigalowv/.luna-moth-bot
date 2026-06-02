using Luna.Application;
using Luna.Infrastructure;
using Luna.Presentation;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddApplicationServices();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddPresentation(builder.Configuration);

var host = builder.Build();

await host.RunAsync();
