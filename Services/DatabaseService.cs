
using Npgsql;

namespace CampusEats.Api.Services;

public class DatabaseService
{
    private readonly string _connectionString;

    public DatabaseService(IConfiguration configuration)
    {
        var connectionString =
            configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException(
                "ConnectionStrings:Default is not configured.");

        if (connectionString.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase)
            || connectionString.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase))
        {
            var databaseUrl = new Uri(connectionString);
            var userInfo = databaseUrl.UserInfo.Split(':', 2);

            var builder = new NpgsqlConnectionStringBuilder
            {
                Host = databaseUrl.Host,
                Port = databaseUrl.IsDefaultPort ? 5432 : databaseUrl.Port,
                Database = databaseUrl.AbsolutePath.TrimStart('/'),
                Username = Uri.UnescapeDataString(userInfo[0]),
                Password = userInfo.Length > 1
                    ? Uri.UnescapeDataString(userInfo[1])
                    : "",
                SslMode = SslMode.Require
            };

            _connectionString = builder.ConnectionString;
        }
        else
        {
            _connectionString = connectionString;
        }
    }

    public NpgsqlConnection CreateConnection()
    {
        return new NpgsqlConnection(_connectionString);
    }
}
