using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace MaterialQ.Areas.Admin.Pages.Users
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public IndexModel(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public List<IdentityUser> UsersList { get; set; }

        [BindProperty]
        public string SelectedUserId { get; set; }

        [BindProperty]
        public string SelectedRole { get; set; }

        public async Task OnGetAsync()
        {
            UsersList = _userManager.Users.ToList();
        }

        public async Task<IActionResult> OnPostAddRoleAsync()
        {
            if (!string.IsNullOrWhiteSpace(SelectedUserId) && !string.IsNullOrWhiteSpace(SelectedRole))
            {
                var user = await _userManager.FindByIdAsync(SelectedUserId);
                if (user != null)
                {
                    if (await _roleManager.RoleExistsAsync(SelectedRole))
                    {
                        if (!await _userManager.IsInRoleAsync(user, SelectedRole))
                        {
                            await _userManager.AddToRoleAsync(user, SelectedRole);
                            TempData["Success"] = $"✅ Added '{SelectedRole}' to {user.Email}";
                        }
                        else
                        {
                            TempData["Error"] = $"{user.Email} already has role '{SelectedRole}'";
                        }
                    }
                }
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRemoveRoleAsync(string userId, string roleName)
        {
            if (!string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(roleName))
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user != null)
                {
                    await _userManager.RemoveFromRoleAsync(user, roleName);
                    TempData["Success"] = $"🗑️ Removed '{roleName}' from {user.Email}";
                }
            }

            return RedirectToPage();
        }

        public async Task<IList<string>> GetUserRolesAsync(IdentityUser user)
        {
            return await _userManager.GetRolesAsync(user);
        }
    }
}
