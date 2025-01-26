using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.IO;
using System.Collections.Generic;
using Npgsql;
using System.Data;
using System.Threading.Tasks;

namespace Slovo_Filter_DAL
{
    public class dbContext
    {
        private readonly string _connectionString = "Server=slovofilter.postgres.database.azure.com;Database=postgres;Port=5432;User Id=postgres;Password=SlovoFilter2025;Ssl Mode=Require;";


        public async Task<DataTable> ExecuteQueryAsync(string query, Dictionary<string, object> parameters = null)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            using var command = new NpgsqlCommand(query, connection);

            if (parameters != null)
            {
                foreach (var p in parameters)
                {
                    command.Parameters.AddWithValue(p.Key, p.Value ?? DBNull.Value);
                }
            }

            var dataTable = new DataTable();
            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();
            dataTable.Load(reader);

            return dataTable;
        }

        public async Task<int> ExecuteNonQueryAsync(string query, Dictionary<string, object> parameters = null)
        {
            using var connection = new NpgsqlConnection(_connectionString);
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
}
