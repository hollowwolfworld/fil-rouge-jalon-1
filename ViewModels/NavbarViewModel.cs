using Microsoft.AspNetCore.Mvc.Rendering;

namespace e_commerce.ViewModels
{
    public class NavbarViewModel
    {
        // Propriété pour le terme de recherche saisi par l'utilisateur
        public string? TermeRecherche { get; set; }
        // Liste des catégories affichées dans un dropdown
        public List<SelectListItem> Categories { get; set; } = new List<SelectListItem>();
        // ID de la catégorie sélectionnée
        public int? CategorieId { get; set; }
    }
}
