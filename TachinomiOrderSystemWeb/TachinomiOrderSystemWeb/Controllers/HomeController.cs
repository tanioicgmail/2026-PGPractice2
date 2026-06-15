using Microsoft.AspNetCore.Mvc;

namespace TOSWeb.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
