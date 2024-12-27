using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Ycode.Storage;
using Ycode.Cosmos;

var builder = FunctionsApplication.CreateBuilder(args);

builder.Services.AddTransient<StorageExample>();
builder.Services.AddTransient<DataAccess>();

builder.ConfigureFunctionsWebApplication();

// Application Insights isn't enabled by default. See https://aka.ms/AAt8mw4.
// builder.Services
//     .AddApplicationInsightsTelemetryWorkerService()
//     .ConfigureFunctionsApplicationInsights();

builder.Build().Run();
