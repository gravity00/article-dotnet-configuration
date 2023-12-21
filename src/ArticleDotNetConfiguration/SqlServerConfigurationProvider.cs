using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using System.Diagnostics;

namespace ArticleDotNetConfiguration;

public class SqlServerConfigurationProvider(
    SqlServerConfigurationSource source
) : ConfigurationProvider, IDisposable
{
    private CancellationTokenSource _cts;
    private Task _refreshWorker;

    #region IDisposable

    ~SqlServerConfigurationProvider() => Dispose(false);

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
                    Debug.WriteLine($"Unhandled exception when waiting for the worker to stop: {e}");
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
                await Task.Delay(15_000, ct);
                try
                {
                    await LoadAsync(ct);
                }
                catch (Exception e)
                {
                    Debug.WriteLine($"Unhandled exception when refreshing database settings: {e}");
                }
            } while (!ct.IsCancellationRequested);
        }, ct);
    }

    private async Task LoadAsync(CancellationToken ct)
    {
        Dictionary<string, string> currentData;
        await using (var connection = new SqlConnection(source.ConnectionString))
        {
            await connection.OpenAsync(ct);

            currentData = (await connection.QueryAsync<(string Key, string Value)>(new CommandDefinition(@"
select 
    [Key],
    [Value]
from ApplicationSettings", cancellationToken: ct))).ToDictionary(e => e.Key, e => e.Value);
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