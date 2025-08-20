using Microsoft.Data.SqlClient;
using Practicas_Demo1.Models;

namespace Practicas_Demo1.Services
{
    public class ToolsService
    {
        private readonly string _connStr;

        public ToolsService(IConfiguration config)
        {
            _connStr = config.GetConnectionString("SecondDB");
        }

        public async Task<List<Models.ToolModel>> GetToolsAsync()
        {
            List<Models.ToolModel> tools = new List<Models.ToolModel>();
            using (SqlConnection conn = new SqlConnection(_connStr))
            {
                await conn.OpenAsync();
                using (SqlCommand cmd = new SqlCommand(" SELECT Id, NoPart, Description, Category, UM, PurshDate, Price, TCStatus, ProductStatus  FROM ESTADIAS_TCRIB_TOOLS", conn))
                {
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            tools.Add(new Models.ToolModel
                            {
                                Id = reader.GetInt32(0),
                                NoPart = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                                Description = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                                Category = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                                UM = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                                PurshDate = reader.IsDBNull(5) ? DateTime.MinValue : reader.GetDateTime(5),
                                Price = reader.IsDBNull(6) ? 0 : Convert.ToDecimal(reader.GetDouble(6)),
                                TCStatus = reader.IsDBNull(7) ? string.Empty : reader.GetString(7),
                                ProductStatus = reader.IsDBNull(8) ? string.Empty : reader.GetString(8)
                            });
                        }
                    }
                }
            }

            return tools;
        }

        public ToolModel? GetToolById(int id)
        {
            using (var conn = new SqlConnection(_connStr))
            {
                string query = "SELECT * FROM ESTADIAS_TCRIB_TOOLS WHERE Id = @Id";
                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    conn.Open();

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new ToolModel
                            {
                                Id = (int)reader["Id"],
                                NoPart = reader["NoPart"]?.ToString() ?? "",
                                Description = reader["Description"]?.ToString() ?? "",
                                Category = reader["Category"]?.ToString() ?? "",
                                UM = reader["UM"]?.ToString() ?? "",
                                PurshDate = reader["PurshDate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["PurshDate"]),
                                Price = reader.IsDBNull(6) ? 0 : (decimal)reader.GetDouble(6),
                                TCStatus = reader["TCStatus"]?.ToString() ?? "",
                                ProductStatus = reader["ProductStatus"]?.ToString() ?? ""
                            };
                        }
                    }
                }
            }

            return null;
        }

        public bool UpdateTool(ToolModel tool)
        {
            using (var conn = new SqlConnection(_connStr))
            {
                string query = @"
                    UPDATE ESTADIAS_TCRIB_TOOLS SET 
                        NoPart = @NoPart,
                        Description = @Description,
                        Category = @Category,
                        UM = @UM,
                        PurshDate = @PurshDate,
                        Price = @Price,
                        TCStatus = @TCStatus,
                        ProductStatus = @ProductStatus
                    WHERE Id = @Id";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@NoPart", tool.NoPart);
                    cmd.Parameters.AddWithValue("@Description", tool.Description);
                    cmd.Parameters.AddWithValue("@Category", tool.Category);
                    cmd.Parameters.AddWithValue("@UM", tool.UM);
                    cmd.Parameters.AddWithValue("@PurshDate", tool.PurshDate);
                    cmd.Parameters.AddWithValue("@Price", tool.Price);
                    cmd.Parameters.AddWithValue("@TCStatus", tool.TCStatus);
                    cmd.Parameters.AddWithValue("@ProductStatus", tool.ProductStatus);
                    cmd.Parameters.AddWithValue("@Id", tool.Id);

                    conn.Open();
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        public bool AddTool(ToolModel tool)
        {
            using (SqlConnection conn = new SqlConnection(_connStr))
            {
                string query = @"
                    INSERT INTO ESTADIAS_TCRIB_TOOLS 
                        (NoPart, Description, Category, UM, PurshDate, Price, TCStatus, ProductStatus)
                    VALUES 
                        (@NoPart, @Description, @Category, @UM, @PurshDate, @Price, @TCStatus, @ProductStatus)";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@NoPart", tool.NoPart);
                    cmd.Parameters.AddWithValue("@Description", tool.Description);
                    cmd.Parameters.AddWithValue("@Category", tool.Category);
                    cmd.Parameters.AddWithValue("@UM", tool.UM);
                    cmd.Parameters.AddWithValue("@PurshDate", tool.PurshDate);
                    cmd.Parameters.AddWithValue("@Price", tool.Price);
                    cmd.Parameters.AddWithValue("@TCStatus", tool.TCStatus);
                    cmd.Parameters.AddWithValue("@ProductStatus", tool.ProductStatus);

                    conn.Open();
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        public async Task DeleteToolAsync(int id)
        {
            using (SqlConnection conn = new SqlConnection(_connStr))
            {
                await conn.OpenAsync();
                using (SqlCommand cmd = new SqlCommand("DELETE FROM ESTADIAS_TCRIB_TOOLS WHERE Id = @Id", conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }
    }
}
