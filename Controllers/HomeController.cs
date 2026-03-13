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

        public IActionResult BasicFormat()
        {
            return View();
        }

        public IActionResult BasicPage()
        {
            return View();
        }

        public IActionResult BasicPageSimple()
        {
            return View();
        }
        public IActionResult BasicPageAlt()
        {
            return View();
        }

        public IActionResult BasicPageList()
        {
            return View();
        }

        public IActionResult BasicPagePretty()
        {
            return View();
        }

        public IActionResult RegionPage()
        {
            return View();
        }

        public IActionResult WVLawPage()
        {
            return View();
        }

        public IActionResult NewsPage()
        {
            return View();
        }

        public IActionResult LicensePage()
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

        public IActionResult TableAccordion()
        {
            return View();
        }

        public IActionResult TableFilters()
        {
            return View();
        }

        public IActionResult QuickNavigation()
        {
            return View();
        }

        public IActionResult WIP()
        {
            return View();
        }

        public IActionResult OSRS()
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
