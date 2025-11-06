using Microsoft.AspNetCore.Mvc;

namespace MaterialQ.Controllers
{
    public class MainController : Controller
    {

        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Login()
        {
            return View();
        }
      
    }
}
