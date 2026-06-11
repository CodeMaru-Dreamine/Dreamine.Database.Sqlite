using Dapper;
using Dreamine.Database.Abstractions;
using Dreamine.Database.Core.Mapping;
using Dreamine.Database.Core.Providers;
using Microsoft.Data.Sqlite;
using System.Data;

namespace Dreamine.Database.Sqlite;

/// <summary>
/// Provides a SQLite database provider implementation.
/// </summary>
public sealed class SqliteDatabaseProvider : DatabaseProviderBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SqliteDatabaseProvider"/> class.
    /// </summary>
    /// <param name="connectionString">The SQLite connection string.</param>
    public SqliteDatabaseProvider(string connectionString)
        : base(connectionString)
    {
    }

    public override DatabaseProviderKind Kind => DatabaseProviderKind.Sqlite;

    public override bool IsTableExists(string tableName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tableName);

        const string sql = "SELECT COUNT(1) FROM sqlite_master WHERE type = 'table' AND name = @TableName";
        return ExecuteScalar<long>(sql, new { TableName = tableName }) > 0;
    }

    public override async Task<bool> IsTableExistsAsync(
        string tableName,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tableName);

        const string sql = "SELECT COUNT(1) FROM sqlite_master WHERE type = 'table' AND name = @TableName";
        var count = await ExecuteScalarAsync<long>(sql, new { TableName = tableName }, cancellationToken)
            .ConfigureAwait(false);
        return count > 0;
    }

    protected override IDbConnection CreateConnection()
    {
        return new SqliteConnection(ConnectionString);
    }

    protected override string QuoteIdentifier(string identifier)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(identifier);
        return "\"" + identifier.Replace("\"", "\"\"", StringComparison.Ordinal) + "\"";
    }

    protected override string GetSqlType(DatabasePropertyMap property)
    {
        var type = property.PropertyType;

        if (type == typeof(bool) ||
            type == typeof(byte) ||
            type == typeof(short) ||
            type == typeof(int) ||
            type == typeof(long))
        {
            return "INTEGER";
        }

        if (type == typeof(float) ||
            type == typeof(double) ||
            type == typeof(decimal))
        {
            return "REAL";
        }

        if (type == typeof(byte[]))
        {
            return "BLOB";
        }

        return "TEXT";
    }

    protected override string BuildPrimaryKeySql(DatabasePropertyMap property)
    {
        return property.IsGenerated ? " PRIMARY KEY AUTOINCREMENT" : " PRIMARY KEY";
    }
}
