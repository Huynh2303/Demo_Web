using Microsoft.AspNetCore.Mvc;

namespace Demo_web_MVC.Controllers
{
    public class OderController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
