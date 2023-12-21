using ArticleDotNetConfiguration;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

var builder = Host.CreateApplicationBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("ArticleDotNetConfiguration");
builder.Configuration.AddSql(() => new SqlConnection(connectionString));

builder.Services.Configure<ExampleOptions>(
    builder.Configuration.GetSection("Example")
);

using var host = builder.Build();

var appOptions = host.Services.GetRequiredService<IOptionsMonitor<ExampleOptions>>();

await Task.Run(async () =>
{
    do
    {
        var options = appOptions.CurrentValue;

        Console.WriteLine(options.Message);
        await Task.Delay(5_000);
    } while (true);
});
