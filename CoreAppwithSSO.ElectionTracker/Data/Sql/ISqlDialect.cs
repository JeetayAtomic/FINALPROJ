using CoreAppwithSSO.ElectionTracker.Models;

namespace CoreAppwithSSO.ElectionTracker.Data.Sql
{
    public interface ISqlDialect
    {
        DbProvider Provider { get; }

        /// <summary>
        /// Quotes a (possibly schema-qualified) identifier, e.g. "ATMDIS.Driver" becomes
        /// <c>[ATMDIS].[Driver]</c> on SQL Server or <c>"ATMDIS"."Driver"</c> on PostgreSQL.
        /// </summary>
        string QuoteName(string identifier);

        /// <summary>
        /// Builds an INSERT that returns the persisted row. SQL Server uses
        /// <c>OUTPUT INSERTED.*</c> before VALUES; PostgreSQL uses a trailing <c>RETURNING *</c>.
        /// </summary>
        string BuildInsertReturning(string tableName, IReadOnlyList<string> columns);

        /// <summary>
        /// Builds an UPDATE filtered by <paramref name="keyColumn"/> that returns the updated row.
        /// </summary>
        string BuildUpdateReturning(string tableName, IReadOnlyList<string> setColumns, string keyColumn);
    }
}
