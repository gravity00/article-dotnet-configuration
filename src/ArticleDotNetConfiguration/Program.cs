using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);

using var host = builder.Build();

var logger = host.Services.GetRequiredService<ILogger<Program>>();

logger.LogInformation("Application started...");

logger.LogInformation("Application terminated. Press <enter> to exit...");
Console.ReadLine();
