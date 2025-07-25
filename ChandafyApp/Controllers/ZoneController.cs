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

        // GET: Zone/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Regions = await _context.Regions.ToListAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,RegionId")] Zone zone)
        {
            if (ModelState.IsValid)
            {
                _context.Add(zone);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Regions = await _context.Regions.ToListAsync();
            return View(zone);
        }

        // GET: Zone/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var zone = await _context.Zones.FindAsync(id);
            if (zone == null) return NotFound();

            ViewBag.Regions = await _context.Regions.ToListAsync();
            return View(zone);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,RegionId")] Zone zone)
        {
            if (id != zone.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(zone);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ZoneExists(zone.Id)) return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Regions = await _context.Regions.ToListAsync();
            return View(zone);
        }

        // GET: Zone/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var zone = await _context.Zones
                .Include(z => z.Region)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (zone == null) return NotFound();

            return View(zone);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var zone = await _context.Zones.FindAsync(id);
            _context.Zones.Remove(zone);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ZoneExists(int id) => _context.Zones.Any(e => e.Id == id);
    }
}
