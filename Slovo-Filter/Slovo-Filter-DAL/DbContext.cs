using Microsoft.EntityFrameworkCore;

namespace Slovo_Filter_DAL;
using Npgsql;
using System.Data;

public class dbContext
{
    private const string connectionString =
        "Server=slovofilter.postgres.database.azure.com;Database=test_db;Port=5432;User Id=postgres;Password=SlovoFilter2025;Ssl Mode=Require;";

    public async Task<DataTable> ExecuteQueryAsync(string query, Dictionary<string, object> parameters = null)
    {
        using var connection = new NpgsqlConnection(connectionString);
        using var command = new NpgsqlCommand(query, connection);

        if (parameters != null)
        {
            foreach (var p in parameters)
            {
                command.Parameters.AddWithValue(p.Key, p.Value ?? DBNull.Value);
            }
        }
        var DataTable = new DataTable();
        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();
        DataTable.Load(reader);
        
        return DataTable;
    }

    public async Task<int> ExecuteNonQueryAsync(string query, Dictionary<string, object> parameters = null)
    {
        using var connection = new NpgsqlConnection(connectionString);
        using var command = new NpgsqlCommand(query, connection);

        if (parameters != null)
        {
            foreach (var param in parameters)
            {
                command.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
            }
        }
        
        await connection.OpenAsync();
        return await command.ExecuteNonQueryAsync();
        
    }
    
    
}