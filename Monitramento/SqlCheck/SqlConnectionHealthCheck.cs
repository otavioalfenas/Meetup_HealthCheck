using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data.SqlClient;
namespace Monitramento.SqlCheck
{
    public class SqlConnectionHealthCheck : Db.DbConnectionHealthCheck
    {
        private static readonly string DefaultTestQuery = "Select 1";
        public SqlConnectionHealthCheck(string connectionString)
                : this(connectionString, testQuery: DefaultTestQuery)
        {
        }
        public SqlConnectionHealthCheck(string connectionString, string testQuery)
                : base(connectionString, testQuery ?? DefaultTestQuery)
        {
        }
        protected override System.Data.Common.DbConnection CreateConnection(string connectionString)
        {
            return new SqlConnection(connectionString);
        }
    }
}
