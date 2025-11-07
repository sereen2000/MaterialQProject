  using Microsoft.AspNetCore.Identity;
  using Microsoft.AspNetCore.Mvc;
  using System.Threading.Tasks;

    namespace MaterialQ.Controllers
    {
        [Area("Admin")]
        public class RoleController : Controller
        {
            private readonly RoleManager<IdentityRole> _roleManager;

            public RoleController(RoleManager<IdentityRole> roleManager)
            {
                _roleManager = roleManager;
            }

            public IActionResult Index()
            {
                var roles = _roleManager.Roles;
                return View(roles);
            }

            [HttpPost]
            public async Task<IActionResult> Create(string name)
            {
                if (!string.IsNullOrEmpty(name))
                {
                    var roleExist = await _roleManager.RoleExistsAsync(name);
                    if (!roleExist)
                    {
                        await _roleManager.CreateAsync(new IdentityRole(name));
                    }
                }
                return RedirectToAction("Index");
            }

            [HttpPost]
            public async Task<IActionResult> Delete(string id)
            {
                var role = await _roleManager.FindByIdAsync(id);
                if (role != null)
                {
                    await _roleManager.DeleteAsync(role);
                }
                return RedirectToAction("Index");
            }
        }
    
}
