using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace ProfAly.CMS.Infrastructure.Persistence.Interceptors;

/// <summary>
/// Applies the operational SQLite pragmas (doc 03 §7) on every opened connection:
/// WAL journaling, foreign-key enforcement, NORMAL synchronous, and a busy timeout.
/// WAL persists in the database file; the per-connection pragmas are re-applied here.
/// </summary>
public sealed class SqlitePragmaInterceptor : DbConnectionInterceptor
{
    private const string PragmaBatch =
        "PRAGMA journal_mode=WAL; PRAGMA foreign_keys=ON; PRAGMA synchronous=NORMAL; PRAGMA busy_timeout=5000;";

    public override void ConnectionOpened(DbConnection connection, ConnectionEndEventData eventData)
    {
        using var command = connection.CreateCommand();
        command.CommandText = PragmaBatch;
        command.ExecuteNonQuery();
    }

    public override async Task ConnectionOpenedAsync(
        DbConnection connection,
        ConnectionEndEventData eventData,
        CancellationToken cancellationToken = default)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = PragmaBatch;
        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
