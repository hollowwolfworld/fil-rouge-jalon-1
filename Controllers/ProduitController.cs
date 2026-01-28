using Dapper;
using e_commerce.Models;
using e_commerce.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Npgsql;
using System.Collections.Generic;
using System.Reflection;

namespace e_commerce.Controllers
{
    [AutoValidateAntiforgeryToken]

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

        [Authorize]
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

        [Authorize(Roles = "Admin")]

        [HttpGet]
        public IActionResult Creation()
        {
            var model = new EditionViewModel();
            model.Categories = GetCategories();

           
            return View(model);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public IActionResult Creation([FromForm] EditionViewModel newprod)
        {
            if (!ModelState.IsValid)
            {
                return View(newprod); // Retourne la vue avec le modèle en cas d'erreur
            }
            if (newprod.ImageProd != null)
            {

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
            }


            string queryAddProd = "insert into products (name,seller,short_desc,description,discount,price,quantity) values (@name,@seller,@short_desc,@description,@discount,@price,@quantity) returning product_id";

            string queryAddCatProd = "insert into products_categories (product_id_fk,category_id_fk) values (@product_id,@categorieID)";

            string queryAddImg = "insert into image (product_id_fk,url) values (@product_id,@imgBdd)";


            if (newprod.ImageProd != null)
            {
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


        [Authorize(Roles = "Admin")]
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

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public IActionResult Modification([FromRoute] int Id, [FromForm] EditionViewModel uppProd)
        {


            
            uppProd.Categories = GetCategories();

            uppProd.Produit.product_id = Id;

            if (!ModelState.IsValid)
            {
                return View(uppProd); // Retourne la vue avec le modèle en cas d'erreur
            }

            if (uppProd.ImageProd != null)
            {
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
            }


            string queryUpProd = "update products set name = @name,seller = @seller,short_desc = @short_desc,description = @description,discount =@discount,price = @price,quantity = @quantity where product_id = @Id";

            string queryUpCatProd = "update products_categories set category_id_fk = @categorieID where product_id_fk = @Id";

            string queryAddImg = "insert into image (product_id_fk,url) values (@product_id,@imgBdd)";


            if (uppProd.ImageProd != null)
            {
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
            }

            using (var connexion = new NpgsqlConnection(_connexionString))
            {
                connexion.Open();
                using (var tran = connexion.BeginTransaction())
                {
                    try
                    {
                        int res = connexion.Execute(queryUpProd, new { name = uppProd.Produit.name, seller = uppProd.Produit.seller, short_desc = uppProd.Produit.short_desc, description = uppProd.Produit.description, discount = uppProd.Produit.discount, price = uppProd.Produit.price, quantity = uppProd.Produit.quantity, Id = Id });
                        if (res != 1)
                        {
                            throw new InvalidOperationException();
                        }



                        int res2 = connexion.Execute(queryUpCatProd, new { categorieID = uppProd.CategorieID, Id = Id });
                        if (res2 != 1)
                        {
                            throw new InvalidOperationException();
                        }


                        foreach (var item in uppProd.ImgBdd)
                        {
                            int res3 = connexion.Execute(queryAddImg, new { product_id = Id, imgBdd = item });
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

                return RedirectToAction("Creation");
            }
        }

        [HttpGet]
        public IActionResult Recherche(string? termeRecherche, int? categorieId)
        {
            List<Produit> produits = new List<Produit>();

            string queryImages = "select * from image where product_id_fk = @product_id";

            string queryCategories = "select * from categories c join products_categories pc on pc.category_id_fk = c.category_id where pc.product_id_fk = @product_id ";

            string query = "SELECT * FROM products p join products_categories pc on p.product_id = pc.product_id_fk WHERE 1=1 ";

            List<Image> images;

            List<Categorie> categories;

            // Ajouter des filtres selon les paramètres
            if (!string.IsNullOrEmpty(termeRecherche))
            {
                query += $" AND name LIKE '%{termeRecherche}%'";
            }

            if (categorieId.HasValue)
            {
                query += $" AND category_id_fk = {categorieId}";
            }

            using (var connexion = new NpgsqlConnection(_connexionString))
            {
                produits = connexion.Query<Produit>(query).ToList();
            }

            foreach (var produit in produits)
            {
                using (var connexion = new NpgsqlConnection(_connexionString))
                {
                    images = connexion.Query<Image>(queryImages, produit).ToList();
                }

                produit.imagesProduits = images;
            }

            foreach (var produit in produits)
            {
                using (var connexion = new NpgsqlConnection(_connexionString))
                {
                    categories = connexion.Query<Categorie>(queryCategories, produit).ToList();
                }

                produit.categoriesProd = categories;
            }

            return View("Views/Acceuil/Index.cshtml", produits);
        }

    }
}
