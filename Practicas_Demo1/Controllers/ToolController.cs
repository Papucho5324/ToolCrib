using Microsoft.AspNetCore.Mvc;
using Practicas_Demo1.Models;
using Practicas_Demo1.Services;

namespace Practicas_Demo1.Controllers
{
    public class ToolController : Controller
    {

        private readonly ToolsService _toolsService;

        public ToolController(ToolsService toolsService)
        {
            _toolsService = toolsService;
        }
        public async Task<IActionResult> Index()
        {
            // Este método devuelve la vista, no los datos
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetTools()
        {
            try
            {
                var tools = await _toolsService.GetToolsAsync();
                return Json(new { data = tools }); // Envuelve los datos en un objeto con la clave 'data'
            }
            catch (Exception ex)
            {
                // En caso de error, devuelve un objeto JSON con un mensaje de error
                return StatusCode(500, new { error = "Error al obtener las herramientas: " + ex.Message });
            }
        }



        [HttpPost]
        public IActionResult Crear(ToolModel model)
        {
            if (!ModelState.IsValid)
            {
                // Si el modelo no es válido, se regresa a la vista 'Add' con los datos
                // para que el usuario pueda corregir.
                return View("Add", model);
            }

            bool success = _toolsService.AddTool(model);

            if (success)
            {
                TempData["SuccessMessage"] = "Herramienta agregado correctamente.";
                // Redirige a la vista de índice para mostrar la lista de materiales.
                // Esto evita que al recargar la página se vuelva a enviar el formulario.
                return RedirectToAction("Index");
            }
            else
            {
                TempData["ErrorMessage"] = "Error al agregar la herramienta.";
                return View("Add", model);
            }
        }

        // Asegúrate de que la acción 'Add' sin parámetros limpie el TempData si se accede directamente.
        [HttpGet]
        public IActionResult Add()
        {
            // Limpia cualquier mensaje de éxito o error que pudiera quedar de una operación anterior.
            TempData.Remove("SuccessMessage");
            TempData.Remove("ErrorMessage");
            return View();
        }


        public IActionResult Edit()
        {
            return View();
        }
        public async Task<IActionResult> GetMaterialsAsync()
        {
            var service = new Services.MaterialsService(HttpContext.RequestServices.GetService<IConfiguration>());
            var materials = await service.GetMaterialsAsync();
            return Json(materials);
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var material = _toolsService.GetToolById(id);
            if (material == null)
            {
                return NotFound();
            }
            return View(material);
        }

        [HttpPost]
        public IActionResult Edit(ToolModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            bool updated = _toolsService.UpdateTool(model);
            if (updated)
            {
                TempData["SuccessMessage"] = "Herramienta actualizada correctamente.";
                return RedirectToAction("Index");
            }

            TempData["ErrorMessage"] = "Error al actualizar la herramienta.";
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var service = new Services.MaterialsService(HttpContext.RequestServices.GetService<IConfiguration>());
            await service.DeleteMaterialAsync(id);
            return Ok();
        }

    }
}

