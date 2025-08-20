using Microsoft.Data.SqlClient;
using Practicas_Demo1.Models;
using System.Data;

public class FolioService : IFolioService
{
    private readonly string _app;
    private readonly string _connStr;


    public FolioService(IConfiguration config)
    {
        _app = config.GetConnectionString("SecondDB");
        _connStr = config.GetConnectionString("MainDB");

    }

    public async Task<List<FolioModel>> ObtenerFoliosAsync(string usuarioId)
    {
        var folios = new List<FolioModel>();

        using var con = new SqlConnection(_app);
        
        await con.OpenAsync();

        var cmd = new SqlCommand($"[sp_ESTADIAS_TOOLCRIB] '13','','{usuarioId}','','','','','',''", con);
        using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            folios.Add(new FolioModel
            {
                Folio = reader[0]?.ToString(),
                Nombre = reader[1]?.ToString(),
                Status = reader[2]?.ToString(),
                ReqSubmitted = reader[3]?.ToString(),
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


    public async Task<List<FolioDetalleModel>> ObtenerDetalleFolioAsync(string folio, string badgeNo)
    {
        var detalles = new List<FolioDetalleModel>();
        using var con = new SqlConnection(_app);
        await con.OpenAsync();

        var cmd = new SqlCommand($"[sp_ESTADIAS_TOOLCRIB] '9','{folio}','{badgeNo}','','','','','',''", con);
        using var reader = await cmd.ExecuteReaderAsync();

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

        return detalles;
    }
    public async Task ActualizarEstadoFolioAsync(string folio, string badgeNo, string estado, string comentario)
    {
        using var con = new SqlConnection(_app);
        await con.OpenAsync();

        // Replicar exactamente la llamada del código antiguo
        string query = $"[sp_ESTADIAS_TOOLCRIB] '7','{folio}','{badgeNo}','','{estado}','','','','{comentario ?? ""}'";
        using var cmd = new SqlCommand(query, con);

        await cmd.ExecuteNonQueryAsync();
    }




}
