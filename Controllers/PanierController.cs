using Microsoft.AspNetCore.Mvc;

namespace e_commerce.Controllers
{
    public class PanierController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
