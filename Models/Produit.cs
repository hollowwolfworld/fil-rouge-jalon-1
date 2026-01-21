using Microsoft.AspNetCore.Mvc.Rendering;

namespace e_commerce.Models
{
    public class Produit
    {
        

        public int product_id { get; set; }
        
        public string name { get; set; }
        public string seller { get; set; }

        public string short_desc { get; set; }

        public string description { get; set; }

        public int discount { get; set; }

        public int price { get; set; }

        public int sells_score { get; set; }

        public int quantity { get; set; }

        public DateTime created_at { get; set; }

        public List<Image> ?imagesProduits { get; set; }

        public List<Categorie> ?categoriesProd { get; set; }

        public string? Datedelivraison { get; set; }
    }
}
