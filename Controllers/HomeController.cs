using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PhotoAdventureWorks.Data;
using PhotoAdventureWorks.Models;
using System.Diagnostics;
using System.Linq;

namespace PhotoAdventureWorks.Controllers
{
    public class HomeController : Controller
    {

        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var photos = _context.Photos
                .OrderByDescending(p => p.PhotoID)
                .Take(8)
                .ToList();
            return View(photos);

        }

        public IActionResult About()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
