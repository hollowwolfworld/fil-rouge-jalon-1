using e_commerce.Models;

namespace e_commerce.ViewModels
{
    public class PanierViewModel
    {
        public Cart cart { get; set; }

        public List<Produit> produits { get; set; }

        public int? quantity { get; set; }
    }
}
