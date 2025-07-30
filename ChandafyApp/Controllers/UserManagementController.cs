using ChandafyApp.Data;
using ChandafyApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChandafyApp.Controllers
{
    [Authorize]
    public class UserManagementController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserManagementController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // GET: UserManagement
        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            var userRoles = new List<UserRolesViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userRoles.Add(new UserRolesViewModel
                {
                    UserId = user.Id,
                    Username = user.UserName,
                    Email = user.Email,
                    Roles = roles.ToList()
                });
            }

            return View(userRoles);
        }

        // GET: UserManagement/EditRoles/5
        public async Task<IActionResult> EditRoles(string id)
        {
            if (id == null) return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var allRoles = await _roleManager.Roles.ToListAsync();
            var roleSelections = new List<RoleSelection>();

            foreach (var role in allRoles)
            {
                var isInRole = await _userManager.IsInRoleAsync(user, role.Name);
                roleSelections.Add(new RoleSelection
                {
                    RoleName = role.Name,
                    IsSelected = isInRole
                });
            }

            var model = new EditRolesViewModel
            {
                UserId = user.Id,
                Username = user.UserName,
                Email = user.Email,
                Roles = roleSelections
            };

            return View(model);
        }


        // POST: UserManagement/EditRoles/5
        [HttpPost]
        public async Task<IActionResult> EditRoles(string id, EditRolesViewModel model)
        {
            if (id != model.UserId) return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var roles = await _userManager.GetRolesAsync(user);
            var result = await _userManager.RemoveFromRolesAsync(user, roles);

            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Failed to remove existing roles");
                return View(model);
            }

            var selectedRoles = model.Roles.Where(r => r.IsSelected).Select(r => r.RoleName);
            result = await _userManager.AddToRolesAsync(user, selectedRoles);

            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Failed to add selected roles");
                return View(model);
            }

            return RedirectToAction(nameof(Index));
        }
    }

    

    

    
}
