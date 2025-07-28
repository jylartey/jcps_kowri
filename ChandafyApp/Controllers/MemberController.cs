using ChandafyApp.Data;
using ChandafyApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChandafyApp.Controllers
{
    [Authorize(Roles = "ItAdmin,Admin,Regional,Muhtamim")]
    public class MemberController : Controller
    {
        private readonly ChandafyDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public MemberController(ChandafyDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Member
        public async Task<IActionResult> Index()
        {
            var members = await _context.Members
                .Include(m => m.Jamaat)
                .Include(m => m.IdentityUser)
                .ToListAsync();
            ViewBag.Jamaats = _context.Jamaats.ToList();
            return View(members);
        }



        // POST: Member/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Member member, string password)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return Json(new { success = false, errors });
            }

            var user = new ApplicationUser { UserName = member.AIMS, Email = $"{member.AIMS}@yourdomain.com" };
            var result = await _userManager.CreateAsync(user, password);

            if (!result.Succeeded)
            {
                var identityErrors = result.Errors.Select(e => e.Description).ToList();
                return Json(new { success = false, errors = identityErrors });
            }

            member.IdentityUserId = user.Id;
            _context.Members.Add(member);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }


        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var member = await _context.Members.FindAsync(id);
            if (member == null)
                return Json(new { success = false, error = "Member not found" });

            return Json(new { success = true, data = member });
        }

        // POST: Member/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Member member)
        {
            if (id != member.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(member);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MemberExists(member.Id))
                        return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Jamaats = _context.Jamaats.ToList();
            return View(member);
        }

        // GET: Member/Delete/5
        [HttpPost]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var member = await _context.Members
                .Include(m => m.Jamaat)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (member == null) return NotFound();

            var user = await _userManager.FindByIdAsync(member.IdentityUserId);

            await _userManager.DeleteAsync(user);
            _context.Members.Remove(member);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        ////[Authorize]
        public async Task<IActionResult> Profile()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var member = await _context.Members
                .Include(m => m.Jamaat)
                .ThenInclude(j => j.Circuit)
                .ThenInclude(c => c.Zone)
                .ThenInclude(z => z.Region)
                .FirstOrDefaultAsync(m => m.IdentityUserId == currentUser.Id);

            if (member == null)
            {
                return NotFound();
            }

            return View(member);
        }
        private bool MemberExists(int id) => _context.Members.Any(e => e.Id == id);
    }
}
