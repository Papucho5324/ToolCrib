using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Practicas_Demo1.Models;
using Practicas_Demo1.Services;
using System.Threading.Tasks;

namespace Practicas_Demo1.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserService _userService;

        public AccountController(UserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string badgeNumber, string password)
        {
            if (string.IsNullOrWhiteSpace(badgeNumber) || string.IsNullOrWhiteSpace(password))
            {
                TempData["Error"] = "Por favor, introduce tu número de reloj y contraseña.";
                return RedirectToAction("Login");
            }

            var usuario = await _userService.VerificarUsuarioAsync(badgeNumber, password);
            var usuario2 = await _userService.ObtenerUsuarioAsync(badgeNumber);


            if (usuario == null)
            {
                TempData["Error"] = "Usuario y/o contraseña inválidos.";
                return RedirectToAction("Login");
            }

            // Guardar datos en sesión
            HttpContext.Session.SetString("BadgeNumber", usuario2.BadgeNum);
            HttpContext.Session.SetString("UserName", usuario2.Name);
            HttpContext.Session.SetString("Area", usuario.Area);

            switch (usuario.Area.ToUpperInvariant())
            {
                case "OFFICE-SUPPLIES":
                    return RedirectToAction("Index", "OfficeSup");

                case "ADMINISTRATOR":
                    return RedirectToAction("Index", "Admin");

                case "SUPERVISOR":
                    return RedirectToAction("Index", "Supervisor");

                case "NA":
                    TempData["Error"] = "No cuentas con los privilegios suficientes para ingresar. Contacta al equipo FIS.";
                    return RedirectToAction("Login");

                default:
                    TempData["Error"] = "Área no reconocida.";
                    return RedirectToAction("Login");
            }
        }
    }
}
