using Dapper;
using Microsoft.Extensions.Configuration;

namespace ArticleDotNetConfiguration;

public class SqlConfigurationProvider(
    SqlConfigurationSource source,
    Action<SqlExceptionContext> exceptionHandler
) : ConfigurationProvider, IDisposable
{
    private CancellationTokenSource _cts;
    private Task _refreshWorker;

    #region IDisposable

    ~SqlConfigurationProvider() => Dispose(false);

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _cts?.Cancel();

            if (_refreshWorker is not null)
            {
                try
                {
                    _refreshWorker.ConfigureAwait(false)
                        .GetAwaiter().GetResult();
                }
                catch (OperationCanceledException)
                {
                    // expected exception due to cancellation
                }
                catch (Exception e)
                {
                    exceptionHandler(new SqlExceptionContext
                    {
                        Exception = e,
                        Provider = this
                    });
                }
            }

            _cts?.Dispose();
        }

        _refreshWorker = null;
        _cts = null;
    }

    #endregion

    public override void Load()
    {
        LoadAsync(CancellationToken.None).ConfigureAwait(false)
            .GetAwaiter().GetResult();

        if (_cts is not null)
            return;

        _cts = new CancellationTokenSource();

        var ct = _cts.Token;
        _refreshWorker ??= Task.Run(async () =>
        {
            do
            {
                await Task.Delay(source.RefreshInterval, ct);
                try
                {
                    await LoadAsync(ct);
                }
                catch (Exception e)
                {
                    exceptionHandler(new SqlExceptionContext
                    {
                        Exception = e,
                        Provider = this
                    });
                }
            } while (!ct.IsCancellationRequested);
        }, ct);
    }

    private async Task LoadAsync(CancellationToken ct)
    {
        Dictionary<string, string> currentData;
        await using (var connection = source.ConnectionFactory())
        {
            await connection.OpenAsync(ct);

            currentData = (await connection.QueryAsync<(string Key, string Value)>(
                new CommandDefinition(source.Sql, cancellationToken: ct)
            )).ToDictionary(e => e.Key, e => e.Value);
        }

        if (HasSameData(currentData))
            return;

        Data = currentData;

        OnReload();
    }

    private bool HasSameData(Dictionary<string, string> currentData)
    {
        if (Data.Count != currentData.Count)
            return false;

        foreach (var (key, value) in currentData)
        {
            if (!Data.TryGetValue(key, out var previousValue) || previousValue != value)
                return false;
        }

        return true;
    }
}