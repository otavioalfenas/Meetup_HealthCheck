using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
namespace Monitramento.Db
{
    public abstract class DbConnectionHealthCheck : Microsoft.Extensions.Diagnostics.HealthChecks.IHealthCheck
    {
        protected DbConnectionHealthCheck(string connectionString)
            : this(connectionString, testQuery: null)
        {
        }

        protected DbConnectionHealthCheck(string connectionString, string testQuery)
        {
            ConnectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            TestQuery = testQuery;
        }

        protected string ConnectionString { get; }

        // This sample supports specifying a query to run as a boolean test of whether the database
        // is responding. It is important to choose a query that will return quickly or you risk
        // overloading the database.
        //
        // In most cases this is not necessary, but if you find it necessary, choose a simple query such as 'SELECT 1'.
        protected string TestQuery { get; }

        protected abstract System.Data.Common.DbConnection CreateConnection(string connectionString);

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            using (var connection = CreateConnection(ConnectionString))
            {
                try
                {
                    await connection.OpenAsync(cancellationToken);

                    if (TestQuery != null)
                    {
                        var command = connection.CreateCommand();
                        command.CommandText = TestQuery;

                        await command.ExecuteNonQueryAsync(cancellationToken);
                    }
                }
                catch (System.Data.Common.DbException ex)
                {
                    return new HealthCheckResult(status: context.Registration.FailureStatus, description:ex.Message, exception: ex);
                }
            }

            return HealthCheckResult.Healthy();
        }
    }
}
