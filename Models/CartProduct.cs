namespace e_commerce.Models
{
    public class CartProduct
    {
        public int product_id_fk { get; set; } 

        public int cart_id_fk { get; set; }

        public int quantity { get; set; }

    }
}
