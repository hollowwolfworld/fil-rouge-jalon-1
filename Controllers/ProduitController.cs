using e_commerce.Models;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using Dapper;

namespace e_commerce.Controllers
{


    public class ProduitController : Controller
    {
        //creation d'un string de connexion a la base de donner 
        private readonly string _connexionString;

        public ProduitController(IConfiguration configuration)
        {
            _connexionString = configuration.GetConnectionString("e_commerce")!;

            if (_connexionString == null)
            {
                throw new Exception("Error : Connexion string not found ! ");
            }
        }

        [HttpGet]

        //detail du produit qui prend en paramettre l'id d'un produit
        public IActionResult Detail(int id)
        {
            // objet produit qui sera retourner pour la vue
            Produit produit;
            //liste d'images contenue dans le produit
            List<Image> images;

            //stockage de la requete sql permettant de recuperer les  images d'un produit
            string queryImages = "select * from image where product_id_fk = @product_id";
            //stockage de la requete sql permettant de recuperer un produit d'apres l'id recu en paramettre
            string queryproduit = "select * from products where product_id = @identifiant";

            //connexion a la base de donner pour effectuer la requetes sql 
            using (var connexion = new NpgsqlConnection(_connexionString))
            {
                produit = connexion.QuerySingle<Produit>(queryproduit, new { identifiant = id });
            }

            //connexion a la base de donner pour effectuer la requetes sql 
            using (var connexion = new NpgsqlConnection(_connexionString))
            {
                images = connexion.Query<Image>(queryImages, produit).ToList();
            }

            //stockage de la liste d'images dans le produit
            produit.imagesProduits = images;

            // retourne un produit.
            return View(produit);
        }
    }
}
