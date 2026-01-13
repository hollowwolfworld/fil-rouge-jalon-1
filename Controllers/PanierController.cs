using Microsoft.AspNetCore.Mvc;

namespace e_commerce.Controllers
{
    public class PanierController : Controller
    {

        private readonly string _connexionString;

        //creation du string de connexion a la base de donner
        public PanierController(IConfiguration configuration)
        {
            _connexionString = configuration.GetConnectionString("e_commerce")!;

            if (_connexionString == null)
            {
                throw new Exception("Error : Connexion string not found ! ");
            }
        }
        public IActionResult Index()
        {


            return View();
        }
    }
}
