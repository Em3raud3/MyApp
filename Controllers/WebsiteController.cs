using Microsoft.AspNetCore.Mvc;

namespace MyApp.Controllers
{
    public class WebsiteController : Controller
    {
        public IActionResult BasicFormat() => View();
        public IActionResult BasicPage() => View();
        public IActionResult BasicPageSimple() => View();
        public IActionResult BasicPageAlt() => View();
        public IActionResult BasicPageList() => View();
        public IActionResult BasicPagePretty() => View();
        public IActionResult RegionPage() => View();
        public IActionResult WVLawPage() => View();
        public IActionResult NewsPage() => View();
        public IActionResult LicensePage() => View();
        public IActionResult LicenseGallery() => View();
        public IActionResult Table() => View();
        public IActionResult TableNoHighlight() => View();
        public IActionResult TableAccordion() => View();
        public IActionResult TableFilters() => View();
        public IActionResult QuickNavigation() => View();
    }
}
