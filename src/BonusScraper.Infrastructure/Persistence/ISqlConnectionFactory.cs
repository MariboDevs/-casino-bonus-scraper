using Microsoft.Data.SqlClient;

namespace BonusScraper.Infrastructure.Persistence;

internal interface ISqlConnectionFactory
{
    Task<SqlConnection> OpenAsync(CancellationToken cancellationToken);
}
