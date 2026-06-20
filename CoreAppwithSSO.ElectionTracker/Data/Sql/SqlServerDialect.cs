using CoreAppwithSSO.ElectionTracker.Models;

namespace CoreAppwithSSO.ElectionTracker.Data.Sql
{
    public sealed class SqlServerDialect : ISqlDialect
    {
        public DbProvider Provider => DbProvider.SqlServer;
        public string QuoteName(string identifier) =>
            string.Join(".", identifier.Split('.').Select(part => $"[{part}]"));

        public string BuildInsertReturning(string tableName, IReadOnlyList<string> columns)
        {
            string columnList = string.Join(", ", columns.Select(QuoteName));
            string valueList = string.Join(", ", columns.Select(c => "@" + c));
            return $"INSERT INTO {QuoteName(tableName)} ({columnList}) OUTPUT INSERTED.* VALUES ({valueList});";
        }

        public string BuildUpdateReturning(string tableName, IReadOnlyList<string> setColumns, string keyColumn)
        {
            string setClause = string.Join(", ", setColumns.Select(c => $"{QuoteName(c)} = @{c}"));
            return $"UPDATE {QuoteName(tableName)} SET {setClause} OUTPUT INSERTED.* WHERE {QuoteName(keyColumn)} = @{keyColumn};";
        }
    }
}
