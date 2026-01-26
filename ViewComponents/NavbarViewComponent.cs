using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Npgsql;
using e_commerce.Models;
using e_commerce.ViewModels;

namespace e_commerce.ViewComponents
{
    public class NavbarViewComponent : ViewComponent
    {
        // Chaîne de connexion à la base de données
        private readonly string _connexionString;
        // Le constructeur injecte la configuration
        public NavbarViewComponent(IConfiguration configuration)
        {
            // Récupération de la chaîne de connexion depuis appsettings.json
            _connexionString = configuration.GetConnectionString("e_commerce")!;
            // Vérification que la chaîne de connexion existe
            if (_connexionString == null)
            {
                throw new Exception("Error : Connexion string not found !");
            }
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            // Création d'une instance du ViewModel
            NavbarViewModel rechercheVM = new NavbarViewModel();
            // Appel à la méthode pour récupérer les catégories
            rechercheVM.Categories = GetCategories();
            // Retour de la vue avec le modèle
            return View(rechercheVM);
        }

        private List<SelectListItem> GetCategories()
        {
            string query = "SELECT category_id , C.name  FROM Categories C";
            List<SelectListItem> categories;
            using (var connexion = new NpgsqlConnection(_connexionString))
            {
                // Utilisation de Dapper pour exécuter la requête
                categories = connexion.Query<Categorie>(query)
                    .Select(c => new SelectListItem
                    {
                        Value = c.category_id.ToString(),
                        Text = c.name
                    })
                    .ToList();
            }
            return categories;
        }
    }
}
