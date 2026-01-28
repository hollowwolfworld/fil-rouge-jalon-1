using Dapper;
using e_commerce.Models;
using e_commerce.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using System.ComponentModel.Design;
using System.Security.Claims;
using System.Text;

namespace e_commerce.Controllers
{
    [AutoValidateAntiforgeryToken]
    public class PanierController : Controller
    {

        private readonly string _connexionString;

        private const string ValidateMessageKey = "ValidateMessage";
        //creation du string de connexion a la base de donner
        public PanierController(IConfiguration configuration)
        {
            _connexionString = configuration.GetConnectionString("e_commerce")!;

            if (_connexionString == null)
            {
                throw new Exception("Error : Connexion string not found ! ");
            }
        }

        [Authorize(Roles = "User")]
        public IActionResult Index()
        {

            int id_user = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));


            string queryIdCart = "select cart_id from carts where user_id_fk = @id_user";

            string queryProduits = "select p.* , cp.quantity  from products p join carts_products cp on p.product_id = cp.product_id_fk join carts c on cp.cart_id_fk = c.cart_id where c.user_id_fk = @id_user";

            //requetes sql pour recuperer tous les images de chaque produit
            string queryImages = "select * from image where product_id_fk = @product_id";

            int prixTotal = 0;

            int cartId;

            Cart cart = new Cart();


            using (var connexion = new NpgsqlConnection(_connexionString))
            {

                cartId = connexion.QuerySingle<int>(queryIdCart, new { id_user = id_user });

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


            foreach (var product in cart.Produits)
            {
                prixTotal += product.Key.price;
            }

            PanierViewModel panierUser = new PanierViewModel();


            panierUser.cart = cart;

            panierUser.cart.CartId = cartId;

            panierUser.PrixPanier = prixTotal;




            return View(panierUser);
        }
        [Authorize(Roles = "User")]
        public IActionResult AddProduct(int id)
        {
            int id_user = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            int cart_user;

            int quantity_product;

            int id_cart;

            string queryCart = "SELECT count(*) FROM carts where user_id_fk = @id_user";

            string queryAddCart = "insert into carts (user_id_fk) values (@id_user)";

            string queryIdCart = "select cart_id from carts where user_id_fk = @id_user";

            string queryCheckQuantity = "select quantity from carts_products where product_id_fk = @id and cart_id_fk = @id_cart";

            string queryinCartProd = "insert into carts_products (product_id_fk,cart_id_fk,quantity) values (@id,@id_cart,@quantity_product)";

            string queryUpCartProd = "update carts_products set quantity = @quantity_product where product_id_fk = @id and cart_id_fk = @id_cart";

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

                        quantity_product = connexion.ExecuteScalar<int>(queryCheckQuantity, new { id = id, id_cart = id_cart });

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

                            int res = connexion.Execute(queryUpCartProd, new { quantity_product = quantity_product, id = id, id_cart = id_cart });
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
        [Authorize(Roles = "User")]
        public IActionResult DeletteProduct(int id)
        {
            int id_user = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            int quantity_product;

            int id_cart;

            string queryIdCart = "select cart_id from carts where user_id_fk = @id_user";

            string queryCheckQuantity = "select quantity from carts_products where product_id_fk = @id and cart_id_fk = @id_cart";

            string queryDeleteProd = "delete from carts_products where product_id_fk = @id and cart_id_fk = @id_cart";

            string queryUpCartProd = "update carts_products set quantity = @quantity_product where product_id_fk = @id and cart_id_fk = @id_cart";

            using (var connexion = new NpgsqlConnection(_connexionString))
            {
                connexion.Open();
                using (var tran = connexion.BeginTransaction())

                    try
                    {


                        id_cart = connexion.ExecuteScalar<int>(queryIdCart, new { id_user = id_user });

                        quantity_product = connexion.ExecuteScalar<int>(queryCheckQuantity, new { id = id, id_cart = id_cart });

                        if (quantity_product == 1)
                        {
                            int res = connexion.Execute(queryDeleteProd, new { id = id, id_cart = id_cart });
                            if (res != 1)
                            {
                                throw new InvalidOperationException();
                            }
                        }

                        else
                        {
                            quantity_product = quantity_product - 1;

                            int res = connexion.Execute(queryUpCartProd, new { quantity_product = quantity_product, id = id, id_cart = id_cart });
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
                        throw new InvalidOperationException("echec du retrait panier");
                    }
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Paiment([FromRoute] int id)
        {
            PanierViewModel model = new PanierViewModel();
            int prixTotal = 0;

            int id_user = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            model.cart.CartId = id;

            string queryProduits = "select p.* , cp.quantity  from products p join carts_products cp on p.product_id = cp.product_id_fk join carts c on cp.cart_id_fk = c.cart_id where c.user_id_fk = @id_user";

            using (var connexion = new NpgsqlConnection(_connexionString))
            {


                model.cart.Produits = connexion.Query<Produit, int, KeyValuePair<Produit, int>>(queryProduits, (produit, qte) =>
                {
                    return new KeyValuePair<Produit, int>(produit, qte);
                }, new { id_user = id_user }, splitOn: "quantity").ToDictionary<Produit, int>();
            }


            foreach (var product in model.cart.Produits)
            {
                prixTotal += product.Key.price;
            }
            model.PrixPanier = prixTotal;

            return View(model);
        }


        [HttpPost]

        public IActionResult Paiment([FromForm] PanierViewModel model)
        {
            if (!VerifCarteBc(model.NumeroCarteBc))
            {
                TempData[ValidateMessageKey] = "carte non valide";
                return View(model);
            }

            int postcode = 0;
            

            Order commandeEnCourt = new Order();

            int id_user = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            string queryProduits = "select p.* , cp.quantity  from products p join carts_products cp on p.product_id = cp.product_id_fk join carts c on cp.cart_id_fk = c.cart_id where c.user_id_fk = @id_user";


            string queryCart = "select cart_id from carts where user_id_fk = @user_id";

            string queryDelCartProd = "delete from carts_products where cart_id_fk = @cartId";

            string queryOrder = "insert into orders (order_status_id_fk,user_id_fk) values (@value,@id_user) returning order_id";

            string queryOrder_Products = "insert into orders_products (order_id_fk,product_id_fk,quantity) values (@id_order,@idProduct,@quantity)";

            string queryUpProd = "update products p set quantity = p.quantity - @quantity, sells_score = sells_score + 1 where product_id = @id";

            

            using (var connexion = new NpgsqlConnection(_connexionString))
            {


                model.cart.Produits = connexion.Query<Produit, int, KeyValuePair<Produit, int>>(queryProduits, (produit, qte) =>
                {
                    return new KeyValuePair<Produit, int>(produit, qte);
                }, new { id_user = id_user }, splitOn: "quantity").ToDictionary<Produit, int>();


                connexion.Open();
                using (var tran = connexion.BeginTransaction())
                {
                    try
                    {
                        int id_order = connexion.ExecuteScalar<int>(queryOrder, new { value = 2, id_user = id_user });


                        foreach (var item in model.cart.Produits)
                        {
                            int res = connexion.Execute(queryOrder_Products, new { id_order = id_order, idProduct = item.Key.product_id, quantity = item.Value });
                            if (res != 1)
                            {
                                throw new InvalidOperationException();
                            }

                            if (item.Value > item.Key.quantity)
                            {
                                throw new InvalidOperationException("pas assez de ce produit en stock");
                            }
                            else
                            {
                                int res2 = connexion.Execute(queryUpProd, new { quantity = item.Value, id = item.Key.product_id });
                                if (res2 != 1)
                                {
                                    throw new InvalidOperationException();
                                }
                            }

                        }

                        int cartId = connexion.QuerySingle<int>(queryCart, new { user_id = id_user });

                        int res3 = connexion.Execute(queryDelCartProd, new { cartId = cartId });


                        tran.Commit();
                    }
                    catch (Exception)
                    {
                        tran.Rollback();
                        throw new InvalidOperationException("echec du paiment");
                    }
                }

            }
            


            return RedirectToAction("Index", "Commande");
        }


        public bool VerifCarteBc(string numeroCarteBc)
        {
            var sb = new StringBuilder(numeroCarteBc);
            for (int i = sb.Length - 2; i >= 0; i = i - 2)
            {
                string num = sb[i].ToString();
                if (int.Parse(num) * 2 > 9)
                {
                    int value = 0;
                    string stockCalc = (int.Parse(num) * 2).ToString();

                    for (int i2 = 0; i2 < stockCalc.Length; i2++)
                    {
                        string num2 = stockCalc[i2].ToString();
                        value += int.Parse(num2);
                    }

                    sb[i] = value.ToString()[0];
                }

            }

            int chiffreAdditionner = 0;

            for (int i = 0; i < sb.Length; i++)
            {

                string resTemp = sb[i].ToString();
                chiffreAdditionner += int.Parse(resTemp);
            }

            if (chiffreAdditionner % 10 == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
         
}
