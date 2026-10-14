using Microsoft.Data.SqlClient;

namespace BonusScraper.Infrastructure.Persistence;

internal sealed class SqlConnectionFactory(string connectionString) : ISqlConnectionFactory
{
    public async Task<SqlConnection> OpenAsync(CancellationToken cancellationToken)
    {
        var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        return connection;
    }
}
