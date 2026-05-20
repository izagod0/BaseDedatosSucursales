using Microsoft.AspNetCore.Mvc;

namespace restauranteswebsbasededatos.Controllers
{
    public class TestController : Controller
    {
        public IActionResult Hola()
        {
            return Content("Funciona", "text/plain");
        }
    }
}
