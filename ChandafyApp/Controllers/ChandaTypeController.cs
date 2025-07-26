using ChandafyApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChandafyApp.Controllers
{
    [Authorize(Roles = "ItAdmin,Admin")]
    public class ChandaTypeController : Controller
    {
        private readonly ChandafyDbContext _context;

        public ChandaTypeController(ChandafyDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var chandaType = await _context.ChandaTypes.ToListAsync();
            return View(chandaType);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ChandaType chandaType)
        {
            if (ModelState.IsValid)
            {
                _context.Add(chandaType);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }

            return Json(new { success = false });
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var chandaType = await _context.ChandaTypes.FindAsync(id);
            if (chandaType == null)
                return NotFound();

            return Json(chandaType); 
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ChandaType chandaType)
        {
            if (id != chandaType.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(chandaType);
                    await _context.SaveChangesAsync();
                    return Json(new { success = true });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ChandaTypeExists(chandaType.Id))
                        return NotFound();

                    throw;
                }
            }

            return Json(new { success = false });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var chandaType = await _context.ChandaTypes.FindAsync(id);
            if (chandaType == null)
                return NotFound();

            _context.ChandaTypes.Remove(chandaType);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        private bool ChandaTypeExists(int id)
        {
            return _context.ChandaTypes.Any(e => e.Id == id);
        }
    }
}