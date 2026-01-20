namespace e_commerce.Models
{
    public class Cart
    {
        public int CartId { get; set; }

        //public User user { get; set; }

        public Dictionary<Produit, int> Produits { get; set; }  = new Dictionary<Produit, int>();
    }
}
