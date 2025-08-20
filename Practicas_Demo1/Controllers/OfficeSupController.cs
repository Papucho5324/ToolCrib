using Microsoft.AspNetCore.Mvc;
using Practicas_Demo1.Services;
using Practicas_Demo1.Models;

namespace Practicas_Demo1.Controllers
{
    public class OfficeSupController : Controller
    {
        private readonly OfficeSupService _officeSupService;

        public OfficeSupController(OfficeSupService officeSupService)
        {
            _officeSupService = officeSupService;
        }
        

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var folio = await _officeSupService.ObtenerFolioAsync();
            ViewBag.Folio = folio;
            return View();

        }

        



        [HttpGet]
        public async Task<IActionResult> ObtenerMateriales()
        {
            try
            {
                var materiales = await _officeSupService.ObtenerMaterialesAsync();
                return Json(materiales);
            }
            catch (Exception ex)
            {
                return Json(new { error = "Error al cargar materiales: " + ex.Message });
            }
        }


        [HttpGet]
        public async Task<IActionResult> ObtenerInfoProducto(string descripcion)
        {
            try
            {
                var producto = await _officeSupService.ObtenerInfoProductoAsync(descripcion);

                if (producto == null)
                    return NotFound(new { error = "Producto no encontrado" });

                return Json(producto);
            }
            catch (Exception ex)
            {
                return Json(new { error = "Error al obtener información del producto: " + ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> VerFolio(string folio, string tipo)
        {
            var productos = await _officeSupService.ObtenerProductosPorFolioAsync(folio, tipo);
            return View(productos);
        }


        [HttpPost]
        public async Task<IActionResult> EnviarYFinalizarSolicitud([FromBody] SolicitudModel model)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(model.Codigo) || model.Cantidad <= 0)
                {
                    return BadRequest(new { error = "Datos incompletos." });
                }

                model.BadgeNum = HttpContext.Session.GetString("BadgeNumber");

                var insercion = await _officeSupService.InsertarSolicitudAsync(model);
                var finalizacion = await _officeSupService.FinalizarSolicitudAsync(model.Folio!, model.BadgeNum!, model.Codigo!);

                return Ok(new { mensaje = $"{insercion}. {finalizacion}" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Error en el proceso: " + ex.Message });
            }
        }


        [HttpPost]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }



    }
}
