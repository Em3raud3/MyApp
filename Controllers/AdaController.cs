using Microsoft.AspNetCore.Mvc;

namespace MyApp.Controllers
{
    public class AdaController : Controller
    {
        public IActionResult ADA_Compliance() => View();
        public IActionResult PDF_Remediation() => View();
    }
}
