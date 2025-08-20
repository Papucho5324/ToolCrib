using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Practicas_Demo1.Models;
using Practicas_Demo1.Services;

namespace Practicas_Demo1.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly UserService _userService;
        private readonly RequesitionService _requesitionService;

        public EmployeeController(UserService userService, RequesitionService requesitionService)
        {
            _userService = userService;
            _requesitionService = requesitionService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string badgeNumber)
        {
            if (string.IsNullOrWhiteSpace(badgeNumber))
            {
                TempData["Error"] = "Introduce tu numero de reloj.";
                return RedirectToAction("Login");
            }

            var usuario = await _userService.ObtenerUsuarioAsync(badgeNumber);
            if (usuario == null)
            {
                TempData["Error"] = $"El numero {badgeNumber} no esta autorizado.";
                return RedirectToAction("Login");
            }

            HttpContext.Session.SetString("BadgeNumber", usuario.BadgeNum);
            HttpContext.Session.SetString("UserName", usuario.Name);
            HttpContext.Session.SetString("Area", usuario.Area);

            return RedirectToAction("Solicitar");
        }

        [HttpGet]
        public IActionResult Employee()
        {
            var badge = HttpContext.Session.GetString("BadgeNumber");
            if (string.IsNullOrEmpty(badge)) return RedirectToAction("Login");

            var model = new UsuarioModel
            {
                BadgeNum = badge,
                Name = HttpContext.Session.GetString("UserName"),
                Area = HttpContext.Session.GetString("Area")
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerMateriales()
        {
            try
            {
                var materiales = await _requesitionService.ObtenerMaterialesAsync();
                return Json(materiales);
            }
            catch (Exception ex)
            {
                return Json(new { error = "Error al cargar materiales: " + ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerHerramientas()
        {
            try
            {
                var herramientas = await _requesitionService.ObtenerHerramientasAsync();
                return Json(herramientas);
            }
            catch (Exception ex)
            {
                return Json(new { error = "Error al cargar herramientas: " + ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerInfoProducto(string descripcion, string tipo)
        {
            try
            {
                var producto = await _requesitionService.ObtenerInfoProductoAsync(descripcion, tipo);

                if (producto == null)
                    return NotFound(new { error = "Producto no encontrado" });

                return Json(producto);
            }
            catch (Exception ex)
            {
                return Json(new { error = "Error al obtener información del producto: " + ex.Message });
            }
        }

        [HttpPost]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult Solicitar()
        {
            
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerFolio()
        {
            try
            {
                var folio = await _requesitionService.ObtenerFolioAsync();
                return Json(new { folio = folio });
            }
            catch (Exception ex)
            {
                return Json(new { error = "Error al obtener folio: " + ex.Message });
            }
        }

        // Nuevo método para enviar múltiples productos en una sola solicitud
        [HttpPost]
        public async Task<IActionResult> EnviarSolicitudCompleta([FromBody] List<SolicitudModel> productos)
        {
            try
            {
                if (productos == null || !productos.Any())
                {
                    return BadRequest(new { error = "No hay productos para procesar." });
                }

                // Obtener folio una sola vez
                var folio = await _requesitionService.ObtenerFolioAsync();
                var badgeNum = HttpContext.Session.GetString("BadgeNumber");

                if (string.IsNullOrEmpty(badgeNum))
                {
                    return Unauthorized(new { error = "Sesión expirada." });
                }

                var resultados = new List<string>();

                // Procesar cada producto con el mismo folio
                foreach (var producto in productos)
                {
                    producto.Folio = folio;
                    producto.BadgeNum = badgeNum;

                    if (string.IsNullOrWhiteSpace(producto.Codigo) || producto.Cantidad <= 0)
                    {
                        continue; // Saltar productos inválidos
                    }

                    var resultado = await _requesitionService.InsertarSolicitudAsync(producto);
                    resultados.Add(resultado);
                }

                // Finalizar la solicitud con el código del primer producto válido
                var primerProductoValido = productos.FirstOrDefault(p => !string.IsNullOrWhiteSpace(p.Codigo));
                if (primerProductoValido != null)
                {
                    var finalizacion = await _requesitionService.FinalizarSolicitudAsync(
                        folio,
                        badgeNum,
                        primerProductoValido.Codigo
                    );

                    return Ok(new
                    {
                        folio = folio,
                        mensaje = $"Solicitud procesada exitosamente. {finalizacion}",
                        productosEnviados = resultados.Count
                    });
                }
                else
                {
                    return BadRequest(new { error = "No hay productos válidos para procesar." });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Error en el proceso: " + ex.Message });
            }
        }

        // Mantener el método original para compatibilidad (para un solo producto)
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

                // Si no tiene folio, generar uno nuevo
                if (string.IsNullOrEmpty(model.Folio))
                {
                    model.Folio = await _requesitionService.ObtenerFolioAsync();
                }

                var insercion = await _requesitionService.InsertarSolicitudAsync(model);
                var finalizacion = await _requesitionService.FinalizarSolicitudAsync(model.Folio!, model.BadgeNum!, model.Codigo!);

                return Ok(new
                {
                    folio = model.Folio,
                    mensaje = $"{insercion}. {finalizacion}"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Error en el proceso: " + ex.Message });
            }
        }

        // Método adicional para ver productos de un folio específico
        public async Task<IActionResult> VerFolio(string folio, string badgeNum, string tipo)
        {
            try
            {
                var productos = await _requesitionService.ObtenerProductosPorFolioAsync(folio, badgeNum, tipo);
                return View(productos);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al obtener productos del folio: " + ex.Message;
                return View(new List<MaterialModel>());
            }
        }
    }
}