using System.ComponentModel.DataAnnotations;

namespace e_commerce.Models
{
    public class Addresse
    {
        public int Addresses_Id { get; set; }

        [Display(Name = "Numéro de rue")]
        public int Street_number { get; set; }

        [Display(Name = "nom de rue")]
        public string? Street_name { get; set; }

        [Display(Name = "ville")]

        public string? City { get; set; }

        [Display(Name = "code postal")]
        public int Postcode { get; set; }

        [Display(Name = "pays")]

        public string? Country { get; set; }
    }
}
