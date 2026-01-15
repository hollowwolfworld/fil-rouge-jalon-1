using Dapper;
using e_commerce.Models;
using e_commerce.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using System.Security.Claims;

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

        [Authorize]
        public IActionResult Index()
        {

            int id_user = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            string queryProduits = "select p.* , cp.quantity  from products p join carts_products cp on p.product_id = cp.product_id_fk join carts c on cp.cart_id_fk = c.cart_id where c.user_id_fk = @id_user";
           
            //requetes sql pour recuperer tous les images de chaque produit
            string queryImages = "select * from image where product_id_fk = @product_id";

            Cart cart = new Cart();


            using (var connexion = new NpgsqlConnection(_connexionString))
            {

                cart.Produits = connexion.Query<Produit, int, KeyValuePair<Produit, int>>(queryProduits, (produit, qte) =>
                {
                    return new KeyValuePair<Produit, int>(produit, qte);
                }, new { id_user = id_user }, splitOn: "quantity").ToDictionary<Produit,int>();



                foreach (var product in cart.Produits)
                {
                    
                     List<Image> images = connexion.Query<Image>(queryImages, product.Key).ToList();   
                    
                    product.Key.imagesProduits = images;
                
     
                }
            }

            PanierViewModel panierUser =  new PanierViewModel();

            panierUser.cart = cart;




            return View(panierUser);
        }

        [HttpPost]
        public IActionResult AddProduct(int Id)
        {


            return View();
        }

    }
}
