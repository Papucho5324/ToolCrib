using Microsoft.AspNetCore.Mvc;

namespace Practicas_Demo1.Controllers
{
    public class DashboardController : Controller
    {
        private readonly DashboardService _dashboardService;

        public DashboardController(DashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        public async Task<IActionResult> Index(string from, string to, string materialType, string fisGroup)
        {
            DateTime? fromDate = from != null ? DateTime.Parse(from) : (DateTime?)null;
            DateTime? toDate = to != null ? DateTime.Parse(to) : (DateTime?)null;

            var viewModel = await _dashboardService.GetDashboardData(fromDate, toDate, materialType, fisGroup);

            // Pasar los filtros actuales a la vista para mantener los valores
            ViewBag.FromFilter = from;
            ViewBag.ToFilter = to;
            ViewBag.MaterialTypeFilter = materialType;
            ViewBag.FISGroupFilter = fisGroup;

            return View(viewModel);
        }

        // Método para obtener datos para DataTable, NO Modificar!
        [HttpGet]
        public async Task<IActionResult> GetRequestsTable(string from, string to, string materialType, string fisGroup)
        {
            try
            {
                // Parseo seguro de fechas
                DateTime? fromDate = DateTime.TryParse(from, out var f) ? f : (DateTime?)null;
                DateTime? toDate = DateTime.TryParse(to, out var t) ? t : (DateTime?)null;

                var requests = await _dashboardService.GetDetailedRequests(
                    fromDate?.ToString("yyyy-MM-dd"),  // Siempre formato seguro
                    toDate?.ToString("yyyy-MM-dd"),
                    string.IsNullOrWhiteSpace(materialType) ? null : materialType,
                    string.IsNullOrWhiteSpace(fisGroup) ? null : fisGroup
                );

                return Json(new { data = requests });
            }
            catch (Exception ex)
            {
                // Esto te muestra el error real en el JSON
                return StatusCode(500, new { error = ex.Message, stack = ex.StackTrace });
            }
        }

        // Método adicional para obtener lista de grupos FIS disponibles (para dropdown)
        [HttpGet]
        public async Task<IActionResult> GetFISGroups()
        {
            try
            {
                var groups = await _dashboardService.GetAvailableFISGroups();
                return Json(groups);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}