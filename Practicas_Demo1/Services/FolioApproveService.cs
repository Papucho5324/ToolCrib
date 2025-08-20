using Microsoft.Data.SqlClient;
using Practicas_Demo1.Models;
using System.Data;

namespace Practicas_Demo1.Services
{
    public class FolioApproveService
    {

        private readonly string _app;
        private readonly string _connStr;

        public FolioApproveService(IConfiguration config)
        {
            _app = config.GetConnectionString("SecondDB");
            _connStr = config.GetConnectionString("MainDB");
        }


        public async Task<List<FolioApproveModel>> GetFolioDetailsAsync()
        {

            var data = new List<FolioApproveModel>();

        
            string query = "[sp_ESTADIAS_TOOLCRIB]";

            using (SqlConnection conDataBase = new SqlConnection(_app))
            {
                using (SqlCommand command = new SqlCommand(query, conDataBase))
                {
                    command.CommandType = CommandType.StoredProcedure;


                    command.Parameters.AddWithValue("@Type", "16"); 
                    command.Parameters.AddWithValue("@param2", DBNull.Value);
                    command.Parameters.AddWithValue("@param3", DBNull.Value);
                    command.Parameters.AddWithValue("@param4", DBNull.Value);
                    command.Parameters.AddWithValue("@param5", DBNull.Value);
                    command.Parameters.AddWithValue("@param6", DBNull.Value);
                    command.Parameters.AddWithValue("@param7", DBNull.Value);
                    command.Parameters.AddWithValue("@param8", DBNull.Value);
                    command.Parameters.AddWithValue("@param9", "TOOLCRIB-MATERIAL");


                    await conDataBase.OpenAsync();
                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            data.Add(new FolioApproveModel
                            {
                                NoFolio = reader["NoFolio"].ToString(),
                                Name = reader["Name"].ToString(),
                                Status = reader["Status"].ToString(),
                                Tipo = reader["Tipo"].ToString(), 
                                Udt = reader.GetDateTime(reader.GetOrdinal("Udt")) 
                            });
                        }
                    }
                }
            }
            return data;
        }
    }
}
