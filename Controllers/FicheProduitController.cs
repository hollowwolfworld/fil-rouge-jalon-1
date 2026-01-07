using e_commerce.Models;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using Dapper;

namespace e_commerce.Controllers
{


    public class FicheProduitController : Controller
    {

        private readonly string _connexionString;

        public FicheProduitController(IConfiguration configuration)
        {
            _connexionString = configuration.GetConnectionString("e_commerce")!;

            if (_connexionString == null)
            {
                throw new Exception("Error : Connexion string not found ! ");
            }
        }

        [HttpGet]
        public IActionResult Index(int id)
        {
            Produit detail;

            List<Image> images;

            string queryImages = "select * from image where product_id_fk = @product_id";

            string queryproduit = "select * from products where product_id = @identifiant";

            using (var connexion = new NpgsqlConnection(_connexionString))
            {
                detail = connexion.QuerySingle<Produit>(queryproduit, new { identifiant = id });
            }

            using (var connexion = new NpgsqlConnection(_connexionString))
            {
                images = connexion.Query<Image>(queryImages, detail).ToList();
            }

            detail.imagesProduits = images;

            return View(detail);
        }
    }
}
