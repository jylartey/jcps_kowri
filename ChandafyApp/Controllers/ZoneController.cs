using ChandafyApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChandafyApp.Controllers
{
    [Authorize(Roles = "ItAdmin,Admin")]
    public class ZoneController : Controller
    {
        private readonly ChandafyDbContext _context;

        public ZoneController(ChandafyDbContext context)
        {
            _context = context;
        }

        // GET: Zone
        public async Task<IActionResult> Index()
        {
            var zones = await _context.Zones
                .Include(z => z.Region)
                .ToListAsync();
            ViewBag.Regions = await _context.Regions.ToListAsync();
            return View(zones);
        }

        

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create( Zone zone)
        {
            if (ModelState.IsValid)
            {
                _context.Add(zone);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }

        // GET: Zone/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var zone = await _context.Zones.FindAsync(id);
            if (zone == null) return NotFound();

            return Json(zone);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Zone zone)
        {
            if (id != zone.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(zone);
                    await _context.SaveChangesAsync();
                    return Json(new { success = true });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ZoneExists(zone.Id)) return NotFound();
                    throw;
                }
            }
            return Json(new { success = false });
        }

        // GET: Zone/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var zone = await _context.Zones
                .Include(z => z.Region)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (zone == null) return NotFound();

            _context.Zones.Remove(zone);
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        

        private bool ZoneExists(int id) => _context.Zones.Any(e => e.Id == id);
    }
}
