using ArticleDotNetConfiguration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

var builder = Host.CreateApplicationBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("ArticleDotNetConfiguration");
builder.Configuration.AddSqlServer(connectionString);

builder.Services.Configure<AppOptions>(
    builder.Configuration.GetSection("App")
);

using var host = builder.Build();

var appOptions = host.Services.GetRequiredService<IOptionsMonitor<AppOptions>>();

await Task.Run(async () =>
{
    do
    {
        var options = appOptions.CurrentValue;

        Console.WriteLine("Application says: '{0}'", options.Message);
        await Task.Delay(5_000);
    } while (true);
});
