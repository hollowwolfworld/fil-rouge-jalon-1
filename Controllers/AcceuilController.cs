using Dapper;
using e_commerce.Models;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using System.Diagnostics;

namespace e_commerce.Controllers
{
    public class AcceuilController : Controller
    {
 
        private readonly string _connexionString;

        public AcceuilController(IConfiguration configuration)
        {
            _connexionString = configuration.GetConnectionString("e_commerce")!;

            if (_connexionString == null)
            {
                throw new Exception("Error : Connexion string not found ! ");
            }
        }

        public IActionResult Index()
        {
            string queryProduits = "select * from products";

            string queryImages = "select * from image where product_id_fk = @product_id";

            List<Produit> produits;

            List<Image> images;

            using (var connexion = new NpgsqlConnection(_connexionString))
            {
                produits = connexion.Query<Produit>(queryProduits).ToList();
            }

            foreach (var produit in produits)
            {
                using (var connexion = new NpgsqlConnection(_connexionString))
                {
                    images = connexion.Query<Image>(queryImages,produit).ToList();
                }

                produit.imagesProduits = images;
            }
            

            return View(produits);
        }

        public IActionResult Privacy()
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
