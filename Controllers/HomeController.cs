using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MyApp.Models;

namespace MyApp.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult BasicPage()
        {
            return View();
        }
        public IActionResult BasicPageAlt()
        {
            return View();
        }

        public IActionResult Table()
        {
            return View();
        }

        public IActionResult TableNoHighlight()
        {
            return View();
        }
        public IActionResult WIP()
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
