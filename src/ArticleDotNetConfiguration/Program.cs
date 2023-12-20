using ArticleDotNetConfiguration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

var builder = Host.CreateApplicationBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("ArticleDotNetConfiguration");
builder.Configuration.AddSqlServer(connectionString);

builder.Services.Configure<AppOptions>(
    builder.Configuration.GetSection("App")
);

using var host = builder.Build();

var logger = host.Services.GetRequiredService<ILogger<Program>>();

logger.LogInformation("Application started...");

var appOptions = host.Services.GetRequiredService<IOptionsMonitor<AppOptions>>();

await Task.Run(async () =>
{
    while (true)
    {
        var options = appOptions.CurrentValue;

        logger.LogInformation("Application says: '{Message}'", options.Message);
        await Task.Delay(5_000);
    }
});
