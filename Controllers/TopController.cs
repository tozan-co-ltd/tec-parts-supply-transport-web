using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using tec_empty_box_preparation_transportation_web.Models;

namespace tec_empty_box_preparation_transportation_web.Controllers
{
    public class TopController : Controller
    {
        private readonly ILogger<TopController> _logger;

        public TopController(ILogger<TopController> logger)
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