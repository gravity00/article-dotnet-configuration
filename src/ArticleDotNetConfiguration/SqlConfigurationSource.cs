using Microsoft.Extensions.Configuration;
using System.Data.Common;
using System.Diagnostics;

namespace ArticleDotNetConfiguration;

public class SqlConfigurationSource : IConfigurationSource
{
    public Func<DbConnection> ConnectionFactory { get; set; }

    public string Sql { get; set; } = @"
select 
    Key,
    Value
from ApplicationSettings";

    public TimeSpan RefreshInterval { get; set; } = TimeSpan.FromSeconds(15);

    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        var exceptionHandler = builder.GetSqlExceptionHandler() ?? (ctx =>
        {
            Debug.WriteLine($"Unhandled SQL exception: {ctx.Exception}");
        });
        return new SqlConfigurationProvider(this, exceptionHandler);
    }
}