using Microsoft.Data.SqlClient;
using Practicas_Demo1.Models;

namespace Practicas_Demo1.Services
{
    public class MaterialsService
    {

        private readonly string _app;
        private readonly string _connStr;

        public MaterialsService(IConfiguration config)
        {
            _app = config.GetConnectionString("SecondDB");

        }

        public async Task<List<Models.MaterialsModel>> GetMaterialsAsync()
        {
            List<Models.MaterialsModel> materials = new List<Models.MaterialsModel>();
            using (SqlConnection conn = new SqlConnection(_app))
            {
                await conn.OpenAsync();
                using (SqlCommand cmd = new SqlCommand("SELECT [Id], [NoPart], [Description], [UM], [Brand], [Specification], [Price],[Location], [Stock] FROM [ESTADIAS_TCRIB_MATERIAL] where [NoPart]!='' ", conn))
                {
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            // Lógica para manejar valores nulos en el DataReader
                            materials.Add(new Models.MaterialsModel
                            {
                                Id = reader.GetInt32(0),
                                NoPart = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                                Description = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                                UM = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                                Brand = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                                Specification = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
                                Price = reader.IsDBNull(6) ? 0 : (decimal)reader.GetDouble(6),
                                Location = reader.IsDBNull(7) ? string.Empty : reader.GetString(7),
                                Stock = reader.IsDBNull(8) ? string.Empty : reader.GetString(8)
                            });

                        }
                    }
                }
            }
            return materials;
        }
        public MaterialsModel GetMaterialById(int id)
        {
            using (var conn = new SqlConnection(_app))
            {
                string query = "SELECT * FROM ESTADIAS_TCRIB_MATERIAL WHERE Id = @Id";
                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new MaterialsModel
                            {
                                Id = (int)reader["Id"],
                                NoPart = reader["NoPart"].ToString(),
                                Description = reader["Description"].ToString(),
                                UM = reader["UM"].ToString(),
                                Brand = reader["Brand"].ToString(),
                                Specification = reader["Specification"].ToString(),
                                Price = Convert.ToDecimal(reader["Price"]),
                                Location = reader["Location"].ToString(),
                                Stock = reader["Stock"].ToString() // Fix: Convert Stock to string
                            };
                        }
                    }
                }
            }
            return null;
        }

        public bool UpdateMaterial(MaterialsModel material)
        {
            using (var conn = new SqlConnection(_app))
            {
                string query = @"
            UPDATE ESTADIAS_TCRIB_MATERIAL SET 
                NoPart = @NoPart,
                Description = @Description,
                UM = @UM,
                Brand = @Brand,
                Specification = @Specification,
                Price = @Price,
                Location = @Location,
                Stock = @Stock,
                Udt = GETDATE()
            WHERE Id = @Id";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@NoPart", material.NoPart);
                    cmd.Parameters.AddWithValue("@Description", material.Description);
                    cmd.Parameters.AddWithValue("@UM", material.UM);
                    cmd.Parameters.AddWithValue("@Brand", material.Brand);
                    cmd.Parameters.AddWithValue("@Specification", material.Specification);
                    cmd.Parameters.AddWithValue("@Price", material.Price);
                    cmd.Parameters.AddWithValue("@Location", material.Location);
                    cmd.Parameters.AddWithValue("@Stock", material.Stock);
                    cmd.Parameters.AddWithValue("@Id", material.Id);

                    conn.Open();
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }



        public bool AddMaterial(MaterialsModel material)
        {
            using (SqlConnection conn = new SqlConnection(_app))
            {
                string query = @"
                    INSERT INTO ESTADIAS_TCRIB_MATERIAL 
                    (NoPart, Description, UM, Brand, Specification, Status, Cdt, Udt, Price, Location, Stock)
                    VALUES (@NoPart, @Description, @UM, @Brand, @Specification, 1, GETDATE(), GETDATE(), @Price, @Location, @Stock)";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@NoPart", material.NoPart);
                    cmd.Parameters.AddWithValue("@Description", material.Description);
                    cmd.Parameters.AddWithValue("@UM", material.UM);
                    cmd.Parameters.AddWithValue("@Brand", material.Brand);
                    cmd.Parameters.AddWithValue("@Specification", material.Specification);
                    cmd.Parameters.AddWithValue("@Price", material.Price);
                    cmd.Parameters.AddWithValue("@Location", material.Location);
                    cmd.Parameters.AddWithValue("@Stock", material.Stock);

                    conn.Open();
                    int rows = cmd.ExecuteNonQuery();
                    return rows > 0;
                }
            }
        }


        public async Task DeleteMaterialAsync(int id)
        {
            using (SqlConnection conn = new SqlConnection(_app))
            {
                await conn.OpenAsync();
                using (SqlCommand cmd = new SqlCommand("DELETE FROM [ESTADIAS_TCRIB_MATERIAL] WHERE Id = @Id", conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

    }
}
