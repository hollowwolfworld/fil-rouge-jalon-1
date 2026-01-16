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
                }, new { id_user = id_user }, splitOn: "quantity").ToDictionary<Produit, int>();



                foreach (var product in cart.Produits)
                {

                    List<Image> images = connexion.Query<Image>(queryImages, product.Key).ToList();

                    product.Key.imagesProduits = images;


                }
            }

            PanierViewModel panierUser = new PanierViewModel();

            panierUser.cart = cart;




            return View(panierUser);
        }

        public IActionResult AddProduct(int id)
        {
            int id_user = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            int cart_user;

            int quantity_product;

            int id_cart;

            string queryCart = "SELECT count(*) FROM carts where user_id_fk = @id_user";

            string queryAddCart = "insert into carts (user_id_fk) values (@id_user)";

            string queryIdCart = "select cart_id from carts where user_id_fk = @id_user";

            string queryCheckExist = "select quantity from carts_products where product_id_fk = @id and cart_id_fk = @id_cart";

            string queryinCartProd = "insert into carts_products (product_id_fk,cart_id_fk,quantity) values (@id,@id_cart,@quantity_product)";

            string queryupCartProd = "update carts_products set quantity = @quantity_product where product_id_fk = @id and cart_id_fk = @id_cart";

            using (var connexion = new NpgsqlConnection(_connexionString))
            {
                connexion.Open();
                using (var tran = connexion.BeginTransaction())
                {
                    cart_user = connexion.ExecuteScalar<int>(queryCart, new { id_user = id_user });

                    try
                    {
                        if (cart_user < 1)
                        {
                            int res = connexion.Execute(queryAddCart, new { id_user = id_user });
                            if (res != 1)
                            {
                                throw new InvalidOperationException();
                            }
                        }

                        id_cart = connexion.ExecuteScalar<int>(queryIdCart, new { id_user = id_user });

                        quantity_product = connexion.ExecuteScalar<int>(queryCheckExist, new { id = id, id_cart = id_cart });

                        if (quantity_product == 0)
                        {
                            quantity_product = quantity_product + 1;

                            int res = connexion.Execute(queryinCartProd, new { id = id, id_cart = id_cart, quantity_product = quantity_product });
                            if (res != 1)
                            {
                                throw new InvalidOperationException();
                            }
                        }

                        else
                        {
                            quantity_product = quantity_product + 1;

                            int res = connexion.Execute(queryupCartProd, new { quantity_product = quantity_product, id = id, id_cart = id_cart });
                            if (res != 1)
                            {
                                throw new InvalidOperationException();
                            }
                        }
                        tran.Commit();
                    }
                    catch (Exception)
                    {

                        tran.Rollback();
                        throw new InvalidOperationException("echec de l'ajout au panier");
                    }
                }
            }

            return RedirectToAction("Index");
        }

    }
}
