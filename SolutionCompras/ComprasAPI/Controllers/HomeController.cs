using Microsoft.AspNetCore.Mvc;

namespace ComprasAPI.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
