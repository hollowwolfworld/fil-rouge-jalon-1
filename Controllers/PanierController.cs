using Dapper;
using e_commerce.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

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
            int emailIndex = 0;

            var email = User.Claims.ToList()[emailIndex].Value;

            string queryIdUser = "select user_id from users where email = @email";

            string queryCarts = "select * from carts where user_id_fk = @id_user ";

            string queryCart_Product = "select * from carts_products where cart_id_fk = @cart";


            string queryProduits = "select * from products where product_id = @Item.product_id_fk";
           
            //requetes sql pour recuperer tous les images de chaque produit
            string queryImages = "select * from image where product_id_fk = @product_id";

            

            int id_user;

            List<CartProduct> cartProducts;

            Cart cart;

            List<Produit> produits;

            List<Image> images;


            using (var connexion = new NpgsqlConnection(_connexionString))
            {
               
                id_user = connexion.ExecuteScalar<int>(queryIdUser,email);
            }

            using (var connexion = new NpgsqlConnection(_connexionString))
            {
               
                cart = connexion.QuerySingle<Cart>(queryCarts, id_user);
            }



            using (var connexion = new NpgsqlConnection(_connexionString))
            {

                cartProducts = connexion.Query<CartProduct>(queryCart_Product, cart).ToList();
            }


            foreach (var item in cartProducts)
            {
                using (var connexion = new NpgsqlConnection(_connexionString))
                {

                    produits = connexion.Query<Produit>(queryProduits, item.product_id_fk).ToList();
                }
                foreach (var produit in produits)
                {
                    using (var connexion = new NpgsqlConnection(_connexionString))
                    {
                        images = connexion.Query<Image>(queryImages, produit).ToList();
                    }

                    produit.imagesProduits = images;
                }
            }


            
           

            return View();
        }

        [HttpPost]
        public IActionResult AddProduct(int Id)
        {


            return View();
        }

    }
}
