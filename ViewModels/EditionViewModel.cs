using e_commerce.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace e_commerce.ViewModels
{
    public class EditionViewModel
    {
       public  Produit Produit { get; set; }

        public List<SelectListItem> Categories { get; set; } = new List<SelectListItem>();

        public int? CategorieID {  get; set; }

        public List<IFormFile>? ImageProd {  get; set; }

        public List<string>? ImgBdd { get; set; } = new List<string>();

    }
}
