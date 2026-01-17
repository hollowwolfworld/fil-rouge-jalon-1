using Dapper;
using e_commerce.Models;
using e_commerce.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Npgsql;
using System.Collections.Generic;
using System.Reflection;

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

        private List<SelectListItem> GetCategories()
        {
            List<SelectListItem> cat = new List<SelectListItem>();

            string queryCat = "select * from categories";

            List<Categorie> categories = new List<Categorie>();

            using (var connexion = new NpgsqlConnection(_connexionString))
            {
                categories = connexion.Query<Categorie>(queryCat).ToList();
            }

            foreach (var item in categories)
            {
                cat.Add(new SelectListItem(item.name,item.category_id.ToString()));
            }
          
            return cat;
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

        [HttpGet]
        public IActionResult Creation()
        {
            var model = new EditionViewModel();
            model.Categories = GetCategories();

           
            return View(model);
        }


        [HttpPost]
        public IActionResult Creation([FromForm] EditionViewModel newprod)
        {
            if (!ModelState.IsValid)
            {
                return View(newprod); // Retourne la vue avec le modèle en cas d'erreur
            }

            Console.WriteLine(newprod.Produit.categoriesProd.ToString());


            string queryAddProd = "insert into product (name,seller,short_desc,description,discount,price,quantity) values (@newprod.Produit.name,@newprod.Produit.seller,@newprod.Produit.short_desc,@newprod.Produit.description,@newprod.Produit.discount,@newprod.Produit.price,newprod.Produit.quantity)";

            string queryAddCatProd = "insert into products_categories (product_id_fk,category_id_fk) values (@newprod.Produit.product_id,@newprod.categorieID)";

            using (var connexion = new NpgsqlConnection(_connexionString))
            {
                connexion.Open();
                using (var tran = connexion.BeginTransaction())
                {
                    try
                    {
                        int res = connexion.ExecuteScalar<int>(queryAddProd, newprod);
                        if (res != 1)
                        {
                            throw new InvalidOperationException();
                        }
                        foreach (var item in newprod.Produit.categoriesProd)   
                        {
                            int res2 = connexion.ExecuteScalar<int>(queryAddCatProd, newprod);
                            if (res2 != 1)
                            {
                                throw new InvalidOperationException();
                            }
                        }

                    }


                    catch (Exception)
                    {
                        tran.Rollback();
                        throw new InvalidOperationException("echec de l'ajout du produit");
                    }

                    tran.Commit();
                }
            }



            return RedirectToRoute("Acceuil/Index");
        }
    }
}
