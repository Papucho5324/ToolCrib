using Practicas_Demo1.Models;

public interface IFolioService
{
    Task<List<FolioModel>> ObtenerFoliosAsync(string usuarioId);
    Task<string> ObtenerHeaderEmpleadoAsync(string usuarioId);
    Task ActualizarEstadoFolioAsync(string folio, string badge, string estado, string comentario);
    Task<List<FolioDetalleModel>> ObtenerDetalleFolioAsync(string folio, string badgeNo);

}
