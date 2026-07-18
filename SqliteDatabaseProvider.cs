using Dapper;
using Dreamine.Database.Abstractions;
using Dreamine.Database.Core.Mapping;
using Dreamine.Database.Core.Providers;
using Microsoft.Data.Sqlite;
using System.Data;

namespace Dreamine.Database.Sqlite;

/// <summary>
/// \if KO
/// <para>SQLite용 Dreamine 데이터베이스 공급자 구현을 제공합니다.</para>
/// \endif
/// \if EN
/// <para>Provides a Dreamine database-provider implementation for SQLite.</para>
/// \endif
/// </summary>
public sealed class SqliteDatabaseProvider : DatabaseProviderBase
{
    /// <summary>
    /// \if KO
    /// <para>지정한 연결 문자열로 <see cref="SqliteDatabaseProvider"/>의 새 인스턴스를 초기화합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Initializes a new <see cref="SqliteDatabaseProvider"/> instance with the specified connection string.</para>
    /// \endif
    /// </summary>
    /// <param name="connectionString">
    /// \if KO
    /// <para>SQLite 연결 문자열입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The SQLite connection string.</para>
    /// \endif
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// \if KO
    /// <para><paramref name="connectionString"/>이 <see langword="null"/>인 경우 발생합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Thrown when <paramref name="connectionString"/> is <see langword="null"/>.</para>
    /// \endif
    /// </exception>
    /// <exception cref="ArgumentException">
    /// \if KO
    /// <para><paramref name="connectionString"/>이 비어 있거나 공백인 경우 발생합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Thrown when <paramref name="connectionString"/> is empty or white space.</para>
    /// \endif
    /// </exception>
    public SqliteDatabaseProvider(string connectionString)
        : base(connectionString)
    {
    }

    /// <summary>
    /// \if KO
    /// <para>SQLite 공급자 종류를 가져옵니다.</para>
    /// \endif
    /// \if EN
    /// <para>Gets the SQLite provider kind.</para>
    /// \endif
    /// </summary>
    public override DatabaseProviderKind Kind => DatabaseProviderKind.Sqlite;

    /// <summary>
    /// \if KO
    /// <para>현재 SQLite 데이터베이스에 지정한 테이블이 존재하는지 확인합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Determines whether the specified table exists in the current SQLite database.</para>
    /// \endif
    /// </summary>
    /// <param name="tableName">
    /// \if KO
    /// <para>확인할 테이블 이름입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The table name to inspect.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>테이블 존재 여부입니다.</para>
    /// \endif
    /// \if EN
    /// <para>Whether the table exists.</para>
    /// \endif
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// \if KO
    /// <para><paramref name="tableName"/>이 <see langword="null"/>인 경우 발생합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Thrown when <paramref name="tableName"/> is <see langword="null"/>.</para>
    /// \endif
    /// </exception>
    /// <exception cref="ArgumentException">
    /// \if KO
    /// <para><paramref name="tableName"/>이 비어 있거나 공백인 경우 발생합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Thrown when <paramref name="tableName"/> is empty or white space.</para>
    /// \endif
    /// </exception>
    public override bool IsTableExists(string tableName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tableName);

        const string sql = "SELECT COUNT(1) FROM sqlite_master WHERE type = 'table' AND name = @TableName";
        return ExecuteScalar<long>(sql, new { TableName = tableName }) > 0;
    }

    /// <summary>
    /// \if KO
    /// <para>현재 SQLite 데이터베이스에 지정한 테이블이 존재하는지 비동기적으로 확인합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Asynchronously determines whether the specified table exists in the current SQLite database.</para>
    /// \endif
    /// </summary>
    /// <param name="tableName">
    /// \if KO
    /// <para>확인할 테이블 이름입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The table name to inspect.</para>
    /// \endif
    /// </param>
    /// <param name="cancellationToken">
    /// \if KO
    /// <para>조회 취소 토큰입니다.</para>
    /// \endif
    /// \if EN
    /// <para>A token used to cancel the query.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>테이블 존재 여부를 결과로 제공하는 작업입니다.</para>
    /// \endif
    /// \if EN
    /// <para>A task whose result indicates whether the table exists.</para>
    /// \endif
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// \if KO
    /// <para><paramref name="tableName"/>이 <see langword="null"/>인 경우 발생합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Thrown when <paramref name="tableName"/> is <see langword="null"/>.</para>
    /// \endif
    /// </exception>
    /// <exception cref="ArgumentException">
    /// \if KO
    /// <para><paramref name="tableName"/>이 비어 있거나 공백인 경우 발생합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Thrown when <paramref name="tableName"/> is empty or white space.</para>
    /// \endif
    /// </exception>
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

    /// <summary>
    /// \if KO
    /// <para>구성된 연결 문자열을 사용하는 새 SQLite 연결을 만듭니다.</para>
    /// \endif
    /// \if EN
    /// <para>Creates a new SQLite connection using the configured connection string.</para>
    /// \endif
    /// </summary>
    /// <returns>
    /// \if KO
    /// <para>닫힌 SQLite 연결입니다.</para>
    /// \endif
    /// \if EN
    /// <para>A closed SQLite connection.</para>
    /// \endif
    /// </returns>
    protected override IDbConnection CreateConnection()
    {
        return new SqliteConnection(ConnectionString);
    }

    /// <summary>
    /// \if KO
    /// <para>SQLite 큰따옴표 문법으로 식별자를 안전하게 인용합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Safely quotes an identifier using SQLite double-quote syntax.</para>
    /// \endif
    /// </summary>
    /// <param name="identifier">
    /// \if KO
    /// <para>인용할 식별자입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The identifier to quote.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>이스케이프하고 인용한 식별자입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The escaped and quoted identifier.</para>
    /// \endif
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// \if KO
    /// <para><paramref name="identifier"/>가 <see langword="null"/>인 경우 발생합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Thrown when <paramref name="identifier"/> is <see langword="null"/>.</para>
    /// \endif
    /// </exception>
    /// <exception cref="ArgumentException">
    /// \if KO
    /// <para><paramref name="identifier"/>가 비어 있거나 공백인 경우 발생합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Thrown when <paramref name="identifier"/> is empty or white space.</para>
    /// \endif
    /// </exception>
    protected override string QuoteIdentifier(string identifier)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(identifier);
        return "\"" + identifier.Replace("\"", "\"\"", StringComparison.Ordinal) + "\"";
    }

    /// <summary>
    /// \if KO
    /// <para>CLR 속성 형식을 대응하는 SQLite 저장소 형식으로 변환합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Converts a CLR property type to its corresponding SQLite storage type.</para>
    /// \endif
    /// </summary>
    /// <param name="property">
    /// \if KO
    /// <para>변환할 속성 매핑입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The property mapping to convert.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>SQLite 저장소 형식 선언입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The SQLite storage-type declaration.</para>
    /// \endif
    /// </returns>
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

    /// <summary>
    /// \if KO
    /// <para>생성 키의 AUTOINCREMENT를 포함한 SQLite 기본 키 SQL을 만듭니다.</para>
    /// \endif
    /// \if EN
    /// <para>Builds SQLite primary-key SQL, including AUTOINCREMENT for generated keys.</para>
    /// \endif
    /// </summary>
    /// <param name="property">
    /// \if KO
    /// <para>기본 키 속성 매핑입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The primary-key property mapping.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>SQLite 기본 키 SQL 조각입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The SQLite primary-key SQL fragment.</para>
    /// \endif
    /// </returns>
    protected override string BuildPrimaryKeySql(DatabasePropertyMap property)
    {
        return property.IsGenerated ? " PRIMARY KEY AUTOINCREMENT" : " PRIMARY KEY";
    }
}
