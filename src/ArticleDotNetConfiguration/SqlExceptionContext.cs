namespace ArticleDotNetConfiguration;

public class SqlExceptionContext
{
    public Exception Exception { get; set; }

    public SqlConfigurationProvider Provider { get; set; }
}