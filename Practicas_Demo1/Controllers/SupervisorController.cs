using Microsoft.AspNetCore.Mvc;
using Practicas_Demo1.Models;
using Practicas_Demo1.Services;

namespace Practicas_Demo1.Controllers
{
    public class SupervisorController : Controller
    {
        private readonly IFolioService _folioService;

        public SupervisorController(IFolioService folioService)
        {
            _folioService = folioService;
        }

        public async Task<IActionResult> Index()
        {
            var id = HttpContext.Session.GetString("BadgeNumber");

            if (string.IsNullOrEmpty(id))
                return RedirectToAction("Login", "Employee");

            // Await the task to get the result before applying LINQ methods
            var foliosList = await _folioService.ObtenerFoliosAsync(id);
            var folios = foliosList.Where(f => f.Status != "TERMINADO").ToList();

            var header = await _folioService.ObtenerHeaderEmpleadoAsync(id);

            ViewBag.Header = header;
            return View(folios);
        }

        [HttpGet]
        public async Task<IActionResult> DetallesFolio(string folio)
        {
            var badge = HttpContext.Session.GetString("BadgeNumber");
            if (string.IsNullOrEmpty(folio) || string.IsNullOrEmpty(badge))
                return BadRequest();

            var detalles = await _folioService.ObtenerDetalleFolioAsync(folio, badge);
            return Json(detalles);
        }


        [HttpPost]
        public async Task<IActionResult> ProcesarDecision(FolioDecisionViewModel model)
        {
            if (string.IsNullOrEmpty(model.Folio) || string.IsNullOrEmpty(model.Estado))
                return BadRequest("Folio and Estado cannot be empty.");

            await _folioService.ActualizarEstadoFolioAsync(model.Folio, model.BadgeNumber, model.Estado, model.Comentario);

            TempData["Message"] = model.Estado == "Aprobado" 
                ? "Folio aprobado exitosamente." 
                : "Folio rechazado exitosamente.";

            return RedirectToAction("Index");
        }

    }
}
