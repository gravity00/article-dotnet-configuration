using ArticleDotNetConfiguration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<AppOptions>(
    builder.Configuration.GetSection("App")
);

using var host = builder.Build();

var logger = host.Services.GetRequiredService<ILogger<Program>>();

logger.LogInformation("Application started...");

var appOptions = host.Services.GetRequiredService<IOptions<AppOptions>>().Value;

logger.LogInformation("Application says: '{Message}'", appOptions.Message);