using System.ComponentModel.DataAnnotations.Schema;

namespace e_commerce.Models
{
    public class User
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Firstname { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public bool Admin { get; set; } = false;
    }
}
