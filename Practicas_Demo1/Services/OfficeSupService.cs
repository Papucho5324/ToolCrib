using Microsoft.Data.SqlClient;
using Practicas_Demo1.Models;

namespace Practicas_Demo1.Services
{
    public class OfficeSupService
    {
        private readonly string _app;
        
        public OfficeSupService(IConfiguration config)
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
            string query = "SELECT Description FROM TEST..ESTADIAS_TCRIB_MATERIAL WHERE LEFT(NoPart,3) IN('PAP')";

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

        public async Task<MaterialModel?> ObtenerInfoProductoAsync(string descripcion)
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
                cmd.Parameters.AddWithValue("@param9", "");

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

        public async Task<string> FinalizarSolicitudAsync(string folio, string badgeNum, string codigo)
        {
            string mensaje = "";

            using var conn = new SqlConnection(_app);

            try
            {
                await conn.OpenAsync();

                string query = "[sp_ESTADIAS_TOOLCRIB] @Type, @Folio, @BadgeNo, @CodeTc, @Description, @Size, @UM, @Qty, @Remark";

                using var cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Type", "8");
                cmd.Parameters.AddWithValue("@Folio", folio);
                cmd.Parameters.AddWithValue("@BadgeNo", badgeNum);
                cmd.Parameters.AddWithValue("@CodeTc", codigo);
                cmd.Parameters.AddWithValue("@Description", "");
                cmd.Parameters.AddWithValue("@Size", "");
                cmd.Parameters.AddWithValue("@UM", "");
                cmd.Parameters.AddWithValue("@Qty", "");
                cmd.Parameters.AddWithValue("@Remark", "");

                using var reader = await cmd.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    mensaje = reader["Msj"]?.ToString() ?? "Solicitud enviada";
                }
            }
            catch (SqlException ex)
            {
                throw new Exception("Error al finalizar la solicitud", ex);
            }

            return mensaje;
        }


        public async Task<List<SolicitudModel>> ObtenerProductosPorFolioAsync(string folio, string tipo)
        {
            var productos = new List<SolicitudModel>();

            using var conn = new SqlConnection(_app);

            try
            {
                await conn.OpenAsync();

                string query = "[sp_ESTADIAS_TOOLCRIB] @Type, @Folio, @BadgeNo, @CodeTc, @Description, @Size, @UM, @Qty, @Remark";

                using var cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Type", "2");
                cmd.Parameters.AddWithValue("@Folio", folio);
                cmd.Parameters.AddWithValue("@BadgeNo", "");  // No se usa
                cmd.Parameters.AddWithValue("@CodeTc", "");   // No se usa
                cmd.Parameters.AddWithValue("@Description", ""); // No se usa
                cmd.Parameters.AddWithValue("@Size", "");
                cmd.Parameters.AddWithValue("@UM", "");
                cmd.Parameters.AddWithValue("@Qty", "");
                cmd.Parameters.AddWithValue("@Remark", tipo); // "Material" o "Tools"

                using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    productos.Add(new SolicitudModel
                    {
                        BadgeNum = reader["Nombre"]?.ToString(),
                        Codigo = reader["CodeTc"]?.ToString(),
                        Descripcion = reader["Description"]?.ToString(),
                        UM = reader["UM"]?.ToString(),
                        Cantidad = int.TryParse(reader["Qty"]?.ToString(), out var cant) ? cant : 0,
                        Comentario = reader["Remark"]?.ToString()
                        // Si es Material: puedes agregar reader["Location"]
                    });
                }
            }
            catch (SqlException ex)
            {
                throw new Exception("Error al obtener productos por folio", ex);
            }

            return productos;
        }



        // Método síncrono para compatibilidad con el controller actual
        public string InsertarSolicitud(SolicitudModel model)
        {
            return InsertarSolicitudAsync(model).GetAwaiter().GetResult();
        }

    }
}
