using Microsoft.Extensions.Configuration;

namespace ArticleDotNetConfiguration;

public class SqlServerConfigurationSource : IConfigurationSource
{
    public string ConnectionString { get; set; }

    public IConfigurationProvider Build(IConfigurationBuilder builder) => new SqlServerConfigurationProvider(this);
}