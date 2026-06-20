using CoreAppwithSSO.ElectionTracker.Models;

namespace CoreAppwithSSO.ElectionTracker.Data.Sql
{
    public static class SqlDialectFactory
    {
        private static readonly ISqlDialect SqlServer = new SqlServerDialect();
        private static readonly ISqlDialect PostgreSql = new PostgreSqlDialect();

        public static ISqlDialect For(DbProvider provider) => provider switch
        {
            DbProvider.PostgreSql => PostgreSql,
            _ => SqlServer
        };
    }
}
