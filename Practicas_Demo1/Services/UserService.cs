using Microsoft.Data.SqlClient;
using Practicas_Demo1.Models;

namespace Practicas_Demo1.Services
{
    public class UserService
    {
        private readonly string _connStr;
        private readonly string _app;

        public UserService(IConfiguration config)
        {
            _connStr = config.GetConnectionString("MainDB");
            _app = config.GetConnectionString("SecondDB");
        }

        public async Task<UsuarioModel?> ObtenerUsuarioAsync(string badgeNumber)
        {
            using var conn = new SqlConnection(_connStr);
            string query = @"SELECT TOP 1 Account, Name, FISGroup FROM Account_TMP 
                             WHERE RIGHT(Account, 6) = RIGHT(@badge, 6)";

            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@badge", badgeNumber);

            await conn.OpenAsync();
            var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new UsuarioModel
                {
                    BadgeNum = reader["Account"].ToString(),
                    Name = reader["Name"].ToString(),
                    Area = reader["FISGroup"].ToString()
                };
            }

            return null;
        }

        public async Task<UsuarioModel?> VerificarUsuarioAsync(string badgeNumber, string password)
        {
            using var conn = new SqlConnection(_app);

            try
            {
                await conn.OpenAsync();

                string query = "[sp_ESTADIAS_TOOLCRIB] @param1, @param2, @param3, @param4, @param5, @param6, @param7, @param8, @param9";

                using var cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@param1", "14");               
                cmd.Parameters.AddWithValue("@param2", "");        
                cmd.Parameters.AddWithValue("@param3", badgeNumber);                 
                cmd.Parameters.AddWithValue("@param4", "");
                cmd.Parameters.AddWithValue("@param5", "");
                cmd.Parameters.AddWithValue("@param6", "");
                cmd.Parameters.AddWithValue("@param7", "");
                cmd.Parameters.AddWithValue("@param8", "");
                cmd.Parameters.AddWithValue("@param9", password);           

                using var reader = await cmd.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    var area = reader["FisGroup"].ToString();

                    return new UsuarioModel
                    {
                        Area = area
                    };
                }
            }
            catch (SqlException ex)
            {
                throw new Exception("Error al verificar usuario", ex);
            }

            return null;
        }

    }
}
