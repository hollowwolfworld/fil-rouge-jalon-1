namespace e_commerce.Models
{
    public class Addresse
    {
        public int Addresses_Id { get; set; }

        public int Street_number { get; set; }

        public string? Street_name { get; set; }

        public string? City { get; set; }

        public int Postcode { get; set; }

        public string? Country { get; set; }
    }
}
