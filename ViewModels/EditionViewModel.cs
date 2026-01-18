using e_commerce.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace e_commerce.ViewModels
{
    public class EditionViewModel
    {
       public  Produit Produit { get; set; }

        public List<SelectListItem> Categories { get; set; } = new List<SelectListItem>();

        public int categorieID {  get; set; }

        public IFormFile? ImageProd {  get; set; }

        public string? imgBdd { get; set; }
    }
}
