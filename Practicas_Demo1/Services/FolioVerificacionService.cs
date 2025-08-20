using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Practicas_Demo1.Models;

namespace Practicas_Demo1.Services
{
    public class FolioVerificacionService
    {
        private readonly string _app;
        private readonly string _connStr;

        public FolioVerificacionService(IConfiguration config)
        {
            _app = config.GetConnectionString("SecondDB");
            _connStr = config.GetConnectionString("MainDB");

        }

        public async Task<List<FolioVerificacionModel>> ObtenerFoliosAsync(string usuarioId)
        {
            var folios = new List<FolioVerificacionModel>();

            using var con = new SqlConnection(_app);

            await con.OpenAsync();

            var cmd = new SqlCommand($"[sp_ESTADIAS_TOOLCRIB] '16','','','','','','','','TOOLCRIB-MATERIAL'", con);
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                folios.Add(new FolioVerificacionModel
                {
                    Folio = reader[0]?.ToString(),
                    Nombre = reader[1]?.ToString(),
                    Status = reader[2]?.ToString(),
                    Tipo = reader[3]?.ToString(),
                    Fecha = reader[4]?.ToString()
                });
            }
            return folios;
        }


        public async Task<string> ObtenerHeaderEmpleadoAsync(string usuarioId)
        {
            using var con = new SqlConnection(_connStr);
            await con.OpenAsync();

            var cmd = new SqlCommand("SELECT Name, FISGroup FROM Account_TMP WHERE Account = @id", con);
            cmd.Parameters.AddWithValue("@id", usuarioId);

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                string name = reader["Name"].ToString();
                string group = reader["FISGroup"].ToString();
                return $"Nombre: {name} | Área: {group}";
            }

            return "Empleado no encontrado";
        }


        public async Task<string> ProcesarDecisionAsync(string folio, string badgeNumber, string estado, string comentario)
        {
            string mensaje = "";

            using var con = new SqlConnection(_app);
            
                await con.OpenAsync();

                string query = $@"[sp_ESTADIAS_TOOLCRIB] '7','{folio}','{badgeNumber}','','{estado}','','','','{comentario}'";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    SqlDataReader reader = await cmd.ExecuteReaderAsync();

                    if (await reader.ReadAsync())
                    {
                        mensaje = reader["Message"].ToString();
                    }

                    reader.Close();
                }

            return mensaje;
            }
        

        public async Task<List<FolioDetalleModel>> ObtenerDetalleFolioAsync(string folio)
        {
            var detalles = new List<FolioDetalleModel>();

            using var con = new SqlConnection(_app);
            
                await con.OpenAsync();

                string query = $@"[sp_ESTADIAS_TOOLCRIB] '2','{folio}','','','','','','',''";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    SqlDataReader reader = await cmd.ExecuteReaderAsync();

                    while (await reader.ReadAsync())
                    {
                        detalles.Add(new FolioDetalleModel
                        {
                            Nombre = reader["Nombre"]?.ToString(),
                            CodeTc = reader["CodeTc"]?.ToString(),
                            Descripcion = reader["Description"]?.ToString(),
                            UM = reader["UM"]?.ToString(),
                            Cantidad = reader["Qty"]?.ToString(),
                            Remark = reader["Remark"]?.ToString(),
                            Location = reader["Location"]?.ToString()
                        });
                    }

                    reader.Close();
                }
            

            return detalles;
        }
    }


}
