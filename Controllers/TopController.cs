using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using tec_empty_box_supply_transport_web.Models;

namespace tec_empty_box_supply_transport_web.Controllers
{
    public class TopController : Controller
    {
        private readonly ILogger<TopController> _logger;

        public TopController(ILogger<TopController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}