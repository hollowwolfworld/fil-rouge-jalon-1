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

      //creation du string de connexion a la base de donner
        public AcceuilController(IConfiguration configuration)
        {
            _connexionString = configuration.GetConnectionString("e_commerce")!;

            if (_connexionString == null)
            {
                throw new Exception("Error : Connexion string not found ! ");
            }
        }

        //index de la page aceuill

        public IActionResult Index()
        {
            //requetes sql pour recuperer tous les produits
            string queryProduits = "select * from products";
            //requetes sql pour recuperer tous les images de chaque produit
            string queryImages = "select * from image where product_id_fk = @product_id";

            //creation de lists pour stoquer les resultat des query
            List<Produit> produits;

            List<Image> images;

            //ouverture d'une connexion Npgsql 
            using (var connexion = new NpgsqlConnection(_connexionString))
            {
                //stockage dans produits d'une requete sql 
                produits = connexion.Query<Produit>(queryProduits).ToList();
            }
             // boucle permetant de stoquer une ou plusieur images dans un produit
            foreach (var produit in produits)
            {
                using (var connexion = new NpgsqlConnection(_connexionString))
                {
                    images = connexion.Query<Image>(queryImages,produit).ToList();
                }

                produit.imagesProduits = images;
            }
            
            //retour des d'une liste de produit
            return View(produits);
        }


        //[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        //public IActionResult Error()
        //{
        //    return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        //}


    }
}
