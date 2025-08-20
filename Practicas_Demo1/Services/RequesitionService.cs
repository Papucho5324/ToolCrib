using Microsoft.Data.SqlClient;
using Practicas_Demo1.Models;

namespace Practicas_Demo1.Services
{
    public class RequesitionService
    {
        private readonly string _app;

        public RequesitionService(IConfiguration config)
        {
            _app = config.GetConnectionString("SecondDB");
        }

        public async Task<string?> ObtenerFolioAsync()
        {
            string? folio = null;

            using var conn = new SqlConnection(_app);
            string query = @"[sp_ESTADIAS_TOOLCRIB] '3','','','','','','','',''";

            using var cmd = new SqlCommand(query, conn);

            try
            {
                await conn.OpenAsync();
                var reader = await cmd.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    folio = reader["NoFolio"].ToString();
                }
            }
            catch (SqlException ex)
            {
                throw new Exception("Error al obtener el folio", ex);
            }

            return folio;
        }

        public async Task<List<MaterialModel>> ObtenerMaterialesAsync()
        {
            var materiales = new List<MaterialModel>();

            using var conn = new SqlConnection(_app);
            string query = "SELECT Description FROM ESTADIAS_TCRIB_MATERIAL WHERE LEFT(NoPart, 3) IN ('TOP')";

            using var cmd = new SqlCommand(query, conn);

            try
            {
                await conn.OpenAsync();
                var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    var material = new MaterialModel
                    {
                        Description = reader["Description"].ToString()
                    };

                    materiales.Add(material);
                }
            }
            catch (SqlException ex)
            {
                throw new Exception("Error al obtener materiales", ex);
            }

            return materiales;
        }

        public async Task<List<MaterialModel>> ObtenerHerramientasAsync()
        {
            var herramientas = new List<MaterialModel>();
            using var conn = new SqlConnection(_app);
            string query = "SELECT Description FROM IMXAPP..ESTADIAS_TCRIB_TOOLS WHERE LEFT(NoPart, 3) IN ('TOP') AND ProductStatus = 'ACTIVO';";
            using var cmd = new SqlCommand(query, conn);

            try
            {
                await conn.OpenAsync();
                var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    var herramienta = new MaterialModel
                    {
                        Description = reader["Description"].ToString()
                    };
                    herramientas.Add(herramienta);
                }
            }
            catch (SqlException ex)
            {
                throw new Exception("Error al obtener herramientas", ex);
            }
            return herramientas;
        }

        public async Task<MaterialModel?> ObtenerInfoProductoAsync(string descripcion, string tipo)
        {
            using var conn = new SqlConnection(_app);

            try
            {
                await conn.OpenAsync();

                string query = "[sp_ESTADIAS_TOOLCRIB] @param1, @param2, @param3, @param4, @param5, @param6, @param7, @param8, @param9";

                using var cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@param1", "6");
                cmd.Parameters.AddWithValue("@param2", "");
                cmd.Parameters.AddWithValue("@param3", "");
                cmd.Parameters.AddWithValue("@param4", "");
                cmd.Parameters.AddWithValue("@param5", descripcion ?? "");
                cmd.Parameters.AddWithValue("@param6", "");
                cmd.Parameters.AddWithValue("@param7", "");
                cmd.Parameters.AddWithValue("@param8", "");
                cmd.Parameters.AddWithValue("@param9", tipo ?? "");

                using var reader = await cmd.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    var producto = new MaterialModel
                    {
                        NoPart = reader["NoPart"]?.ToString(),
                        UM = reader["UM"]?.ToString(),
                        Stock = reader["Stock"]?.ToString()
                    };
                    return producto;
                }
            }
            catch (SqlException ex)
            {
                throw new Exception("Error al obtener información del producto", ex);
            }

            return null;
        }

        public async Task<List<MaterialModel>> ObtenerProductosPorFolioAsync(string folio, string badgeNum, string tipo)
        {
            var productos = new List<MaterialModel>();

            using var conn = new SqlConnection(_app);
            string query = "[sp_ESTADIAS_TOOLCRIB] @param1, @param2, @param3, @param4, @param5, @param6, @param7, @param8, @param9";

            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@param1", "2"); // Acción: obtener productos por folio
            cmd.Parameters.AddWithValue("@param2", folio);
            cmd.Parameters.AddWithValue("@param3", badgeNum);
            cmd.Parameters.AddWithValue("@param4", "");
            cmd.Parameters.AddWithValue("@param5", "");
            cmd.Parameters.AddWithValue("@param6", "");
            cmd.Parameters.AddWithValue("@param7", "");
            cmd.Parameters.AddWithValue("@param8", "");
            cmd.Parameters.AddWithValue("@param9", tipo);

            try
            {
                await conn.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    var producto = new MaterialModel
                    {
                        NoPart = reader["NoPart"]?.ToString(),
                        Description = reader["Description"]?.ToString(),
                        UM = reader["UM"]?.ToString(),
                        Stock = reader["Stock"]?.ToString()
                        // agrega más campos si los necesitas
                    };

                    productos.Add(producto);
                }
            }
            catch (SqlException ex)
            {
                throw new Exception("Error al obtener productos del folio", ex);
            }

            return productos;
        }


        // Convertir a async para consistencia
        public async Task<string> InsertarSolicitudAsync(SolicitudModel model)
        {
            string mensaje = "";

            using var conn = new SqlConnection(_app);

            try
            {
                await conn.OpenAsync(); 

                string query = "[sp_ESTADIAS_TOOLCRIB] @param1, @param2, @param3, @param4, @param5, @param6, @param7, @param8, @param9";

                using var cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@param1", "1");
                cmd.Parameters.AddWithValue("@param2", model.Folio ?? "");
                cmd.Parameters.AddWithValue("@param3", model.BadgeNum ?? "");
                cmd.Parameters.AddWithValue("@param4", model.Codigo ?? "");
                cmd.Parameters.AddWithValue("@param5", model.Descripcion ?? "");
                cmd.Parameters.AddWithValue("@param6", "");
                cmd.Parameters.AddWithValue("@param7", model.UM ?? "");
                cmd.Parameters.AddWithValue("@param8", model.Cantidad.ToString());
                cmd.Parameters.AddWithValue("@param9", model.Comentario ?? "");

                using var reader = await cmd.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    mensaje = reader["Msj"]?.ToString() ?? "";
                }
            }
            catch (SqlException ex)
            {
                throw new Exception("Error al insertar solicitud", ex);
            }

            return mensaje;
        }

        // Método síncrono para compatibilidad con el controller actual
        public string InsertarSolicitud(SolicitudModel model)
        {
            return InsertarSolicitudAsync(model).GetAwaiter().GetResult();
        }
        public async Task<string> FinalizarSolicitudAsync(string folio, string badgeNum, string codigo)
        {
            using var conn = new SqlConnection(_app);
            string mensaje = "";

            string query = "[sp_ESTADIAS_TOOLCRIB] @param1, @param2, @param3, @param4, @param5, @param6, @param7, @param8, @param9";

            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@param1", "8");
            cmd.Parameters.AddWithValue("@param2", folio);
            cmd.Parameters.AddWithValue("@param3", badgeNum);
            cmd.Parameters.AddWithValue("@param4", codigo);
            cmd.Parameters.AddWithValue("@param5", "");
            cmd.Parameters.AddWithValue("@param6", "");
            cmd.Parameters.AddWithValue("@param7", "");
            cmd.Parameters.AddWithValue("@param8", "");
            cmd.Parameters.AddWithValue("@param9", "");

            try
            {
                await conn.OpenAsync();
                var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    mensaje = reader["Msj"]?.ToString() ?? "";
                }
            }
            catch (SqlException ex)
            {
                throw new Exception("Error al finalizar solicitud", ex);
            }

            return mensaje;
        }


    }
}