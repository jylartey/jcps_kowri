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
        private readonly UserManager<IdentityUser> _userManager;

        public MemberController(ChandafyDbContext context, UserManager<IdentityUser> userManager)
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

        // GET: Member/Create
        public IActionResult Create()
        {
            ViewBag.Jamaats = _context.Jamaats.ToList();
            return View();
        }

        // POST: Member/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Member member, string password)
        {
            if (ModelState.IsValid)
            {
                var user = new IdentityUser { UserName = member.AIMS, Email = $"{member.AIMS}@chandafy.org" };
                var result = await _userManager.CreateAsync(user, password);

                if (result.Succeeded)
                {
                    var createdUser = await _userManager.FindByNameAsync(user.UserName);
                    if (createdUser == null)
                    {
                        ModelState.AddModelError(string.Empty, "Failed to retrieve the created user.");
                        ViewBag.Jamaats = _context.Jamaats.ToList();
                        return View(member);
                    }
                    member.IdentityUserId = user.Id;
                    _context.Add(member);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            ViewBag.Jamaats = _context.Jamaats.ToList();
            return View(member);
        }

        
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var member = await _context.Members.FindAsync(id);
            if (member == null) return NotFound();

            ViewBag.Jamaats = _context.Jamaats.ToList();
            return View(member);
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
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var member = await _context.Members
                .Include(m => m.Jamaat)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (member == null) return NotFound();

            return View(member);
        }

        // POST: Member/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var member = await _context.Members.FindAsync(id);
            var user = await _userManager.FindByIdAsync(member.IdentityUserId);

            await _userManager.DeleteAsync(user);
            _context.Members.Remove(member);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        
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
