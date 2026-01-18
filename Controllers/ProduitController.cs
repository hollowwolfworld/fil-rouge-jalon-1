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


            if (newprod.ImageProd != null)
            {
                var ext = Path.GetExtension(newprod.ImageProd.FileName).ToLowerInvariant();

                if (string.IsNullOrEmpty(ext))
                {
                    ModelState.AddModelError("ImageProd", "image non accepter");
                }
            }



            string queryAddProd = "insert into products (name,seller,short_desc,description,discount,price,quantity) values (@name,@seller,@short_desc,@description,@discount,@price,@quantity) returning product_id";

            string queryAddCatProd = "insert into products_categories (product_id_fk,category_id_fk) values (@product_id,@categorieID)";

            string queryAddImg = "insert into image (product_id_fk,url) values (@product_id,@imgBdd)";


            string? filepath = null;
            if (newprod.ImageProd != null && newprod.ImageProd.Length > 0)
            {
                filepath = Path.Combine("img/imgsProds/",Path.GetFileNameWithoutExtension(Path.GetRandomFileName()) + Path.GetExtension(newprod.ImageProd.FileName)).ToString();

                using (var stream = System.IO.File.Create("wwwroot/" + filepath))
                {
                    newprod.ImageProd.CopyTo(stream);
                }

                newprod.imgBdd = filepath;
            }


            using (var connexion = new NpgsqlConnection(_connexionString))
            {
                connexion.Open();
                using (var tran = connexion.BeginTransaction())
                {
                    try
                    {
                        int productId = connexion.ExecuteScalar<int>(queryAddProd,new {name = newprod.Produit.name, seller = newprod.Produit.seller, short_desc = newprod.Produit.short_desc, description = newprod.Produit.description, discount = newprod.Produit.discount,price = newprod.Produit.price, quantity = newprod.Produit.quantity});



                        
                        int res = connexion.Execute(queryAddCatProd, new { product_id = productId, categorieID = newprod.categorieID});
                        if (res != 1)
                        {
                            throw new InvalidOperationException();
                        }

                        int res2 = connexion.Execute(queryAddImg, new { product_id = productId,imgBdd = newprod.imgBdd });
                        if (res2 != 1)
                        {
                            throw new InvalidOperationException();
                        }


                        tran.Commit();

                    }


                    catch (Exception)
                    {
                        tran.Rollback();
                        throw new InvalidOperationException("echec de l'ajout du produit");
                    }

                }
            }



            return RedirectToAction("Creation");
        }
    }
}
