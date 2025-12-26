using System.Data.Common;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Diagnostics;
using NaturalSort.Extension;
using Reaparr.Domain;

namespace Reaparr.Data.Contracts;

/// <summary>
/// Interceptor that registers the NATURALSORT collation on SQLite connections when they are opened.
/// This is required because collations are per-connection settings in SQLite.
/// </summary>
public class NaturalSortCollationInterceptor : DbConnectionInterceptor
{
    private static readonly NaturalSortComparer Comparer = new(StringComparison.InvariantCultureIgnoreCase);

    public override void ConnectionOpened(DbConnection connection, ConnectionEndEventData eventData)
    {
        RegisterCollation(connection);
        base.ConnectionOpened(connection, eventData);
    }

    public override async Task ConnectionOpenedAsync(
        DbConnection connection,
        ConnectionEndEventData eventData,
        CancellationToken cancellationToken = default
    )
    {
        RegisterCollation(connection);
        await base.ConnectionOpenedAsync(connection, eventData, cancellationToken);
    }

    private static void RegisterCollation(DbConnection connection)
    {
        if (connection is SqliteConnection sqliteConnection)
        {
            sqliteConnection.CreateCollation(OrderByNaturalExtensions.CollationName, (x, y) => Comparer.Compare(x, y));
        }
    }
}
