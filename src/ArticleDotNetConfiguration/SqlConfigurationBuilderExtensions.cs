using Microsoft.Extensions.Configuration;
using System.Data.Common;

namespace ArticleDotNetConfiguration;

public static class SqlConfigurationBuilderExtensions
{
    private const string SqlExceptionHandlerKey = "SqlExceptionHandler";

    public static IConfigurationBuilder AddSql(
        this IConfigurationBuilder builder,
        Func<DbConnection> connectionFactory,
        string sql = null,
        TimeSpan? refreshInterval = null
    )
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(connectionFactory);

        return builder.Add<SqlConfigurationSource>(source =>
        {
            source.ConnectionFactory = connectionFactory;

            if(sql is not null)
                source.Sql = sql;

            if (refreshInterval is not null)
                source.RefreshInterval = refreshInterval.Value;
        });
    }

    public static IConfigurationBuilder SetSqlExceptionHandler(
        this IConfigurationBuilder builder,
        Action<SqlExceptionContext> handler
    )
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Properties[SqlExceptionHandlerKey] = handler;

        return builder;
    }

    public static Action<SqlExceptionContext> GetSqlExceptionHandler(this IConfigurationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder.Properties.TryGetValue(SqlExceptionHandlerKey, out var value)
            ? value as Action<SqlExceptionContext>
            : null;
    }
}