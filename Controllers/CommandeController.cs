using e_commerce.Models;
using e_commerce.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using System.Collections.Generic;
using System.Security.Claims;
using Dapper;

namespace e_commerce.Controllers
{
    public class CommandeController : Controller
    {

        private readonly string _connexionString;

        public CommandeController(IConfiguration configuration)
        {
            _connexionString = configuration.GetConnectionString("e_commerce")!;

            if (_connexionString == null)
            {
                throw new Exception("Error : Connexion string not found ! ");
            }
        }
        public IActionResult Index()
        {

            int id_user = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            Dictionary<Produit, int> Produits = new Dictionary<Produit, int>();

        string queryProduits = "select p.*,op.quantity from products p join orders_products op on p.product_id = op.product_id_fk join orders o on o.order_id = op.order_id_fk where o.user_id_fk = @id_user";


            using (var connexion = new NpgsqlConnection(_connexionString))
            { 
                Produits = connexion.Query<Produit, int, KeyValuePair<Produit, int>>(queryProduits, (produit, qte) =>
                {
                    return new KeyValuePair<Produit, int>(produit, qte);
                }, new { id_user = id_user }, splitOn: "quantity").ToDictionary<Produit, int>();

            }
                return View(Produits);
        }
    }
}
