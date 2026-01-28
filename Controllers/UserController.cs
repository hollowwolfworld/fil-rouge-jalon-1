using e_commerce.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using Dapper;

namespace e_commerce.Controllers
{
    [AutoValidateAntiforgeryToken]
    public class UserController : Controller
    {

        private readonly string _connexionString;

        public UserController(IConfiguration configuration)
        {
            _connexionString = configuration.GetConnectionString("e_commerce")!;

            if (_connexionString == null)
            {
                throw new Exception("Error : Connexion string not found ! ");
            }
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Index()
        {
            List<User> users = new List<User>();

            string queryUsers = "select * from users";

            using (var connexion = new NpgsqlConnection(_connexionString))
            {
                //stockage dans produits d'une requete sql 
                users = connexion.Query<User>(queryUsers).ToList();
            }


            return View(users);
        }
    }
}
