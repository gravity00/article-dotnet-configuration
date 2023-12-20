using Microsoft.Extensions.Configuration;

namespace ArticleDotNetConfiguration;

public static class SqlServerConfigurationBuilderExtensions
{
    public static IConfigurationBuilder AddSqlServer(this IConfigurationBuilder builder, string connectionString)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(connectionString);

        return builder.Add<SqlServerConfigurationSource>(source =>
        {
            source.ConnectionString = connectionString;
        });
    }
}