using Microsoft.AspNetCore.Mvc;
using Practicas_Demo1.Models;
using Practicas_Demo1.Services;

namespace Practicas_Demo1.Controllers
{
    public class FolioStatusController : Controller
    {
        private readonly FolioVerificacionService _folioVerificacionService;

        public FolioStatusController(FolioVerificacionService folioVerificacionService)
        {
            _folioVerificacionService = folioVerificacionService;
        }
        public async Task<IActionResult> Index()
        {
            var id = HttpContext.Session.GetString("BadgeNumber");

            if (string.IsNullOrEmpty(id))
                return RedirectToAction("Login", "Employee");

            var header = await _folioVerificacionService.ObtenerHeaderEmpleadoAsync(id);
            var folios = await _folioVerificacionService.ObtenerFoliosAsync(id);

            ViewBag.Header = header;
            return View(folios);
        }

        [HttpGet]
        public async Task<IActionResult> DetallesFolio(string folio)
        {
            if (string.IsNullOrEmpty(folio))
            {
                return BadRequest("El folio no puede estar vacío.");
            }

            try
            {
                var detalles = await _folioVerificacionService.ObtenerDetalleFolioAsync(folio);
                return Json(detalles); 
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error al obtener los detalles del folio: " + ex.Message);
            }
        }


        [HttpPost]
        public async Task<IActionResult> ProcesarDecision(FolioDecisionViewModel model)
        {
            if (string.IsNullOrEmpty(model.Folio) || string.IsNullOrEmpty(model.Estado))
            {
                TempData["Mensaje"] = "Error: Faltan datos esenciales para procesar la decisión.";
                return RedirectToAction("Index");
            }

            var comentario = model.Estado == "RECHAZADO" ? model.Comentario : "";

            var mensaje = await _folioVerificacionService.ProcesarDecisionAsync(model.Folio, model.BadgeNumber, model.Estado, comentario);

            TempData["Mensaje"] = mensaje;
            return RedirectToAction("Index"); 
        }
    }

}
