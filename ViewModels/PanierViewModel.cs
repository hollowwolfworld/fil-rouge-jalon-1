using e_commerce.Models;

namespace e_commerce.ViewModels
{
    public class PanierViewModel
    {

        public Cart cart { get; set; } = new Cart();

        public int? PrixPanier { get; set; }

        public string? NumeroCarteBc { get; set; }

    }
}
