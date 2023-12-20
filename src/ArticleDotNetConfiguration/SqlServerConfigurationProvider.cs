using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;

namespace ArticleDotNetConfiguration;

public class SqlServerConfigurationProvider(
    SqlServerConfigurationSource source
) : ConfigurationProvider
{
    public override void Load()
    {
        Dictionary<string, string> applicationSettings;
        using (var connection = new SqlConnection(source.ConnectionString))
        {
            connection.Open();

            applicationSettings = connection.Query<(string Key, string Value)>(@"
select 
    [Key],
    [Value]
from ApplicationSettings").ToDictionary(e => e.Key, e => e.Value);
        }

        if(HasSameData(applicationSettings))
            return;

        Data = applicationSettings;

        OnReload();
    }

    private bool HasSameData(Dictionary<string, string> applicationSettings)
    {
        if (Data.Count != applicationSettings.Count) 
            return false;

        foreach (var (key, value) in applicationSettings)
        {
            if (!Data.TryGetValue(key, out var previousValue) || previousValue != value)
                return false;
        }

        return true;
    }
}