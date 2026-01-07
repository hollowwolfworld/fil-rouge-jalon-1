namespace e_commerce.Models
{
    public class Image
    {
        public int image_Id { get; set; }

        public int product_id_fk { get; set; }

        public string url { get; set; }

        public string alt {  get; set; }

        public DateTime created_at { get; set; }
    }
}
