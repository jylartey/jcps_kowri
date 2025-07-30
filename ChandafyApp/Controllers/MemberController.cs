using ChandafyApp.Data;
using ChandafyApp.Models;
using ChandafyApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChandafyApp.Controllers
{
    [Authorize]
    public class MemberController : Controller
    {
        private readonly ChandafyDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public MemberController(ChandafyDbContext context, UserManager<ApplicationUser> userManager,
                          RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // GET: Member
        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users
            .Include(u => u.Jamaat)
            .ToListAsync();

            var userViewModels = new List<MemberViewModel>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userViewModels.Add(new MemberViewModel
                {
                    Id = user.Id,
                    AIMS = user.AIMS,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    ContactNumber = user.ContactNumber,
                    DateOfBirth = user.DateOfBirth,
                    JamaatName = user.Jamaat?.Name,
                    JamaatId = user.JamaatId,
                    Roles = roles.ToList()
                });
            }

            ViewBag.Jamaats = await _context.Jamaats.ToListAsync();
            ViewBag.AllRoles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
            return View(userViewModels);
        }



        // POST: Member/Create
        public async Task<IActionResult> Create(MemberCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.JamaatId.HasValue)
                {
                    var jamaatExists = await _context.Jamaats.AnyAsync(j => j.Id == model.JamaatId.Value);
                    if (!jamaatExists)
                    {
                        return Json(new
                        {
                            success = false,
                            message = "Invalid Jamaat selected",
                            errors = new List<string> { "The selected Jamaat does not exist." }
                        });
                    }
                }

                var user = new ApplicationUser
                {
                    UserName = model.AIMS,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    ContactNumber = model.ContactNumber,
                    DateOfBirth = model.DateOfBirth,
                    AIMS = model.AIMS,
                    JamaatId = model.JamaatId
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    if (model.SelectedRoles != null && model.SelectedRoles.Any())
                    {
                        await _userManager.AddToRolesAsync(user, model.SelectedRoles);
                    }
                    return Json(new { success = true });
                }

                return Json(new
                {
                    success = false,
                    message = "User creation failed",
                    errors = result.Errors.Select(e => e.Description).ToList()
                });
            }

            return Json(new
            {
                success = false,
                message = "Invalid data",
                errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList()
            });
        }



        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
                return Json(new { success = false, error = "ID is required" });

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return Json(new { success = false, error = "User not found" });

            var userRoles = await _userManager.GetRolesAsync(user);

            var model = new MemberEditViewModel
            {
                Id = user.Id,
                AIMS = user.AIMS,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                ContactNumber = user.ContactNumber,
                DateOfBirth = user.DateOfBirth,
                JamaatId = user.JamaatId,
                SelectedRoles = userRoles.ToList()
            };

            return Json(new { success = true, data = model });
        }

        // POST: Member/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, MemberEditViewModel model)
        {
            if (id != model.Id)
                return Json(new { success = false, error = "ID mismatch" });

            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                    return Json(new { success = false, error = "User not found" });

                // Update user properties
                user.AIMS = model.AIMS;
                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.Email = model.Email;
                user.ContactNumber = model.ContactNumber;
                user.DateOfBirth = model.DateOfBirth;
                user.JamaatId = model.JamaatId;

                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    // Update roles
                    var currentRoles = await _userManager.GetRolesAsync(user);
                    await _userManager.RemoveFromRolesAsync(user, currentRoles);

                    if (model.SelectedRoles != null && model.SelectedRoles.Any())
                    {
                        await _userManager.AddToRolesAsync(user, model.SelectedRoles);
                    }

                    return Json(new { success = true });
                }

                return Json(new
                {
                    success = false,
                    message = "User update failed",
                    errors = result.Errors.Select(e => e.Description).ToList()
                });
            }

            return Json(new
            {
                success = false,
                message = "Invalid data",
                errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList()
            });
        }
        // GET: Member/ResetPassword/{id}
        public async Task<IActionResult> ResetPassword(string id)
        {
            if (id == null)
                return Json(new { success = false, error = "ID is required" });

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return Json(new { success = false, error = "User not found" });

            return Json(new
            {
                success = true,
                data = new { Id = user.Id, Email = user.Email }
            });
        }

        // POST: Member/ResetPassword/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return Json(new
                {
                    success = false,
                    message = "Invalid data",
                    errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList()
                });

            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null)
                return Json(new { success = false, error = "User not found" });

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, model.NewPassword);

            if (result.Succeeded)
            {
                return Json(new { success = true });
            }

            return Json(new
            {
                success = false,
                message = "Password reset failed",
                errors = result.Errors.Select(e => e.Description).ToList()
            });
        }

        // GET: Member/Delete/5
        // POST: Member/Delete/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return Json(new { success = false, error = "User not found" });

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                return Json(new { success = true });
            }

            return Json(new
            {
                success = false,
                message = "User deletion failed",
                errors = result.Errors.Select(e => e.Description).ToList()
            });
        }


        ////[Authorize]
        public async Task<IActionResult> Profile()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Get the user with all related Jamaat hierarchy
            var user = await _userManager.Users
                .Include(u => u.Jamaat)
                .ThenInclude(j => j.Circuit)
                .ThenInclude(c => c.Zone)
                .ThenInclude(z => z.Region)
                .FirstOrDefaultAsync(u => u.Id == currentUser.Id);

            if (user == null)
            {
                return NotFound();
            }

            // Get user roles
            var roles = await _userManager.GetRolesAsync(user);

            // Create the view model
            var model = new MemberProfileViewModel
            {
                Id = user.Id,
                AIMS = user.AIMS,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                ContactNumber = user.ContactNumber,
                DateOfBirth = user.DateOfBirth,
                JamaatName = user.Jamaat?.Name,
                CircuitName = user.Jamaat?.Circuit?.Name,
                ZoneName = user.Jamaat?.Circuit?.Zone?.Name,
                JamaatId = user.JamaatId,
                RegionName = user.Jamaat?.Circuit?.Zone?.Region?.Name,
                Roles = roles.ToList()
            };
            ViewBag.Jamaats = await _context.Jamaats.ToListAsync();
            ViewBag.AllRoles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
            return View(model);
        }
        //private bool MemberExists(int id) => _context.u.Any(e => e.Id == id);
    }
}
