using Microsoft.AspNetCore.Mvc;

namespace ChandafyApp.Controllers
{
    public class LoginController : Controller
    {
        public IActionResult Index()
        {
            return View(); 
        }
    }
}
