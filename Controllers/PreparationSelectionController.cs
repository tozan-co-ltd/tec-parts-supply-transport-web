using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using tec_parts_supply_transport_web.Models;

namespace tec_parts_supply_transport_web.Controllers
{
    public class PreparationSelectionController : Controller
    {
        private readonly ILogger<PreparationSelectionController> _logger;

        public PreparationSelectionController(ILogger<PreparationSelectionController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// トップ画面表示
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            return View();
        }
    }
}