using Microsoft.AspNetCore.Mvc;
using Practicas_Demo1.Models;
using Practicas_Demo1.Services;

namespace Practicas_Demo1.Controllers
{
    public class MaterialsController : Controller
    {

        private readonly MaterialsService _materialsService;

        public MaterialsController(MaterialsService materialsService)
        {
            _materialsService = materialsService;
        }
        public async Task<IActionResult> Index()
        {
            var materials = await _materialsService.GetMaterialsAsync();
            return View(materials ?? new List<MaterialsModel>());
        }

        [HttpPost]
        public IActionResult Crear(MaterialsModel model)
        {
            if (!ModelState.IsValid)
            {
                // Si el modelo no es válido, se regresa a la vista 'Add' con los datos
                // para que el usuario pueda corregir.
                return View("Add", model);
            }

            bool success = _materialsService.AddMaterial(model);

            if (success)
            {
                TempData["SuccessMessage"] = "Material agregado correctamente.";
                // Redirige a la vista de índice para mostrar la lista de materiales.
                // Esto evita que al recargar la página se vuelva a enviar el formulario.
                return RedirectToAction("Index");
            }
            else
            {
                TempData["ErrorMessage"] = "Error al agregar el material.";
                // Si hay un error de lógica de negocio, se regresa a la vista 'Add' con los datos.
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
            var material = _materialsService.GetMaterialById(id);
            if (material == null)
            {
                return NotFound();
            }
            return View(material);
        }

        [HttpPost]
        public IActionResult Edit(MaterialsModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            bool updated = _materialsService.UpdateMaterial(model);
            if (updated)
            {
                TempData["SuccessMessage"] = "Material actualizado correctamente.";
                return RedirectToAction("Index");
            }

            TempData["ErrorMessage"] = "Error al actualizar el material.";
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
