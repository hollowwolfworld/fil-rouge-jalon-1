using Dapper;
using e_commerce.Models;
using e_commerce.ViewModels;
using Microsoft.AspNetCore.Components.Web;
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

            foreach (var img in newprod.ImageProd)
            {
                if (img != null)
                {
                    var ext = Path.GetExtension(img.FileName).ToLowerInvariant();

                    if (string.IsNullOrEmpty(ext))
                    {
                        ModelState.AddModelError("ImageProd", "image non accepter");
                    }
                }
            }


            string queryAddProd = "insert into products (name,seller,short_desc,description,discount,price,quantity) values (@name,@seller,@short_desc,@description,@discount,@price,@quantity) returning product_id";

            string queryAddCatProd = "insert into products_categories (product_id_fk,category_id_fk) values (@product_id,@categorieID)";

            string queryAddImg = "insert into image (product_id_fk,url) values (@product_id,@imgBdd)";

            foreach (var img in newprod.ImageProd)
            {
                string? filepath = null;
                if (img != null && img.Length > 0)
                {
                    filepath = Path.Combine("/img/imgsProds/", Path.GetFileNameWithoutExtension(Path.GetRandomFileName()) + Path.GetExtension(img.FileName)).ToString();

                    using (var stream = System.IO.File.Create("wwwroot/" + filepath))
                    {
                        img.CopyTo(stream);
                    }

                     newprod.ImgBdd.Add(filepath);
                }
            }

            using (var connexion = new NpgsqlConnection(_connexionString))
            {
                connexion.Open();
                using (var tran = connexion.BeginTransaction())
                {
                    try
                    {
                        int productId = connexion.ExecuteScalar<int>(queryAddProd,new {name = newprod.Produit.name, seller = newprod.Produit.seller, short_desc = newprod.Produit.short_desc, description = newprod.Produit.description, discount = newprod.Produit.discount,price = newprod.Produit.price, quantity = newprod.Produit.quantity});



                        
                        int res = connexion.Execute(queryAddCatProd, new { product_id = productId, categorieID = newprod.CategorieID});
                        if (res != 1)
                        {
                            throw new InvalidOperationException();
                        }


                        foreach (var item in newprod.ImgBdd)   
                        {
                            int res2 = connexion.Execute(queryAddImg, new { product_id = productId, imgBdd = item });
                            if (res2 != 1)
                            {
                                throw new InvalidOperationException();
                            }
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



        [HttpGet]
        public IActionResult Modification([FromRoute]int Id)
        {
            var uppProd = new EditionViewModel();
            uppProd.Categories = GetCategories();

            string queryProd = "select * from products where product_id = @Id";

            string queryImg = "select url from image where product_id_fk = @Id";

            using (var connexion = new NpgsqlConnection(_connexionString))
            {
                uppProd.Produit = connexion.QuerySingle<Produit>(queryProd,new { Id = Id });

                uppProd.ImgBdd = connexion.Query<string>(queryImg, new { Id = Id }).ToList();
            }

                return View(uppProd);
        }

        [HttpPost]
        public IActionResult Modification([FromForm] EditionViewModel uppProd)
        {




            if (!ModelState.IsValid)
            {
                return View(uppProd); // Retourne la vue avec le modèle en cas d'erreur
            }

            foreach (var img in uppProd.ImageProd)
            {
                if (img != null)
                {
                    var ext = Path.GetExtension(img.FileName).ToLowerInvariant();

                    if (string.IsNullOrEmpty(ext))
                    {
                        ModelState.AddModelError("ImageProd", "image non accepter");
                    }
                }
            }



            string queryUpProd = "update products set name = @name,seller = @seller,short_desc = @short_desc,description = @description,discount =@discount,price = @price,quantity = @quantity where product_id = @Id";

            string queryUpCatProd = "update products_categories set category_id_fk @categorieID where product_id_fk = @Id";

            string queryAddImg = "insert into image (product_id_fk,url) values (@product_id,@imgBdd)";

            foreach (var img in uppProd.ImageProd)
            {
                string? filepath = null;
                if (img != null && img.Length > 0)
                {
                    filepath = Path.Combine("/img/imgsProds/", Path.GetFileNameWithoutExtension(Path.GetRandomFileName()) + Path.GetExtension(img.FileName)).ToString();

                    using (var stream = System.IO.File.Create("wwwroot/" + filepath))
                    {
                        img.CopyTo(stream);
                    }

                    uppProd.ImgBdd.Add(filepath);
                }
            }

            using (var connexion = new NpgsqlConnection(_connexionString))
            {
                connexion.Open();
                using (var tran = connexion.BeginTransaction())
                {
                    try
                    {
                        int res = connexion.Execute(queryUpProd, new { name = uppProd.Produit.name, seller = uppProd.Produit.seller, short_desc = uppProd.Produit.short_desc, description = uppProd.Produit.description, discount = uppProd.Produit.discount, price = uppProd.Produit.price, quantity = uppProd.Produit.quantity, Id = Idprod });
                        if (res != 1)
                        {
                            throw new InvalidOperationException();
                        }



                        int res2 = connexion.Execute(queryUpCatProd, new { categorieID = uppProd.CategorieID, product_id = Idprod });
                        if (res2 != 1)
                        {
                            throw new InvalidOperationException();
                        }


                        foreach (var item in uppProd.ImgBdd)
                        {
                            int res3 = connexion.Execute(queryAddImg, new { product_id = Idprod, imgBdd = item });
                            if (res3 != 1)
                            {
                                throw new InvalidOperationException();
                            }
                        }

                        tran.Commit();

                    }


                    catch (Exception)
                    {
                        tran.Rollback();
                        throw new InvalidOperationException("echec de la modification du produit");
                    }

                }

                return RedirectToAction("Detail");
            }
        }

        public IActionResult Suprresion(string url)
        {


            return RedirectToAction("Modification");
        }

    }
}
