using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ComprasAPI.Controllers
{
    public class DemoController : Controller
    {
        [AllowAnonymous]
        public IActionResult Index()
        {
            return View();
        }

        [Route("api/demo/demo2")]
        [HttpGet]
        [Authorize]

        public IActionResult Demo2()
        {
            return Ok("This is a demo 2 response from DemoController");
        }
    }
}
