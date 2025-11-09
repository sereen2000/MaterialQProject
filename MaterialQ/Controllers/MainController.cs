using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MaterialQ.Controllers
{
    [Authorize]
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
        public IActionResult Profile()
        {
            return View();
        }

        public IActionResult Quotations()
        {
           
            return View();
        }

        public IActionResult AddQuotation()
        {
           
            return View();
        }

        public IActionResult Products()
        {
            return View();
        }
    }
}
