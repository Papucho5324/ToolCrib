using Microsoft.Data.SqlClient;
using Practicas_Demo1.Models;
using System.Data;

public class DashboardService
{
    private readonly string _app;

    public DashboardService(IConfiguration config)
    {
        _app = config.GetConnectionString("SecondDB");
    }

    public async Task<DashboardViewModel> GetDashboardData(DateTime? fromDate, DateTime? toDate, string materialType = null, string fisGroup = null)
    {
        var viewModel = new DashboardViewModel
        {
            RequestStatusData = new List<RequestStatusData>(),
            GroupConsumptionData = new List<GroupConsumptionData>(),
            DailyRequestsData = new List<DailyRequestsData>()
        };

        using (var connection = new SqlConnection(_app))
        {
            await connection.OpenAsync();

            // Lógica para obtener los datos del gráfico de estados (Type '1')
            using (var cmd = new SqlCommand("sp_IMX_TOOLCRIB_DASHBOARD_JP", connection))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Type", "1");
                cmd.Parameters.AddWithValue("@From", fromDate?.ToString("yyyy-MM-dd") ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@To", toDate?.ToString("yyyy-MM-dd") ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@PartNo", (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@MaterialType", (object)materialType ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@FISGroup", (object)fisGroup ?? DBNull.Value);

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        viewModel.RequestStatusData.Add(new RequestStatusData
                        {
                            Status = reader["Status"].ToString(),
                            TotalRequests = Convert.ToInt32(reader["TotalRequests"])
                        });
                    }
                }
            }

            // Lógica para obtener los datos del gráfico de consumo por grupo (Type '2')
            using (var cmd = new SqlCommand("sp_IMX_TOOLCRIB_DASHBOARD_JP", connection))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Type", "2");
                cmd.Parameters.AddWithValue("@From", fromDate?.ToString("yyyy-MM-dd") ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@To", toDate?.ToString("yyyy-MM-dd") ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@PartNo", (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@MaterialType", (object)materialType ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@FISGroup", (object)fisGroup ?? DBNull.Value);

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        viewModel.GroupConsumptionData.Add(new GroupConsumptionData
                        {
                            FisGroup = reader["FISGroup"].ToString(),
                            TotalAmount = Convert.ToDecimal(reader["TotalAmount"])
                        });
                    }
                }
            }

            // Lógica para obtener los datos del gráfico de tendencias (Type '4')
            using (var cmd = new SqlCommand("sp_IMX_TOOLCRIB_DASHBOARD_JP", connection))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Type", "4");
                cmd.Parameters.AddWithValue("@From", fromDate?.ToString("yyyy-MM-dd") ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@To", toDate?.ToString("yyyy-MM-dd") ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@PartNo", (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@MaterialType", (object)materialType ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@FISGroup", (object)fisGroup ?? DBNull.Value);

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        viewModel.DailyRequestsData.Add(new DailyRequestsData
                        {
                            RequestDate = Convert.ToDateTime(reader["RequestDate"]),
                            TotalRequests = Convert.ToInt32(reader["TotalRequests"])
                        });
                    }
                }
            }

            // Lógica para obtener la sumatoria total (Type '5')
            using (var cmd = new SqlCommand("sp_IMX_TOOLCRIB_DASHBOARD_JP", connection))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Type", "5");
                cmd.Parameters.AddWithValue("@From", fromDate?.ToString("yyyy-MM-dd") ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@To", toDate?.ToString("yyyy-MM-dd") ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@PartNo", (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@MaterialType", (object)materialType ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@FISGroup", (object)fisGroup ?? DBNull.Value);

                var result = await cmd.ExecuteScalarAsync();
                viewModel.GrandTotalAmount = result != DBNull.Value ? Convert.ToDecimal(result) : 0;
            }
        }

        return viewModel;
    }

    // Servicio para obtener los detalles de las solicitudes en dataTables no TOCAR!
    public async Task<List<RequestDetailsData>> GetDetailedRequests(string from, string to, string materialType, string fisGroup)
    {
        var requests = new List<RequestDetailsData>();

        using (var conn = new SqlConnection(_app))
        {
            using (var cmd = new SqlCommand("sp_IMX_TOOLCRIB_DASHBOARD_JP", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Type", "7");
                cmd.Parameters.AddWithValue("@From", (object?)from ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@To", (object?)to ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@MaterialType", (object?)materialType ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@FISGroup", (object?)fisGroup ?? DBNull.Value);

                await conn.OpenAsync();
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        requests.Add(new RequestDetailsData
                        {
                            NoFolio = reader["NoFolio"].ToString(),
                            BadgeNo = reader["BadgeNo"].ToString(),
                            FISGroup = reader["FISGroup"].ToString(),
                            CodeTc = reader["CodeTc"].ToString(),
                            Description = reader["Description"].ToString(),
                            UM = reader["UM"].ToString(),
                            Qty = Convert.ToInt32(reader["Qty"]),
                            Remark = reader["Remark"].ToString(),
                            Status = reader["Status"].ToString(),
                            Approver = reader["Approver"].ToString(),
                            ApproverRemark = reader["ApproverRemark"].ToString(),
                            Udt = Convert.ToDateTime(reader["Udt"])
                        });
                    }
                }
            }
        }
        return requests;
    }

    // Método para obtener la lista de grupos FIS disponibles
    public async Task<List<string>> GetAvailableFISGroups()
    {
        var groups = new List<string>();

        using (var conn = new SqlConnection(_app))
        {
            await conn.OpenAsync();
            using (var cmd = new SqlCommand(@"
                SELECT DISTINCT b.FISGroup 
                FROM ESTADIAS_TCRIB_REQUEST a
                INNER JOIN (SELECT Account, FISGroup FROM [IMXC-OT-DBSH1].CTO.dbo.Account_TMP GROUP BY Account, FISGroup) b
                    ON a.BadgeNo = b.Account
                WHERE b.FISGroup NOT IN ('ADMIN', 'Default')
                ORDER BY b.FISGroup", conn))
            {
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        groups.Add(reader["FISGroup"].ToString());
                    }
                }
            }
        }
        return groups;
    }
}