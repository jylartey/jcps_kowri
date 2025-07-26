using ChandafyApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChandafyApp.Controllers
{
    [Authorize(Roles = "ItAdmin,Admin")]
    public class RegionController : Controller
    {
        private readonly ChandafyDbContext _context;

        public RegionController(ChandafyDbContext context)
        {
            _context = context;
        }

        // GET: Region
        public async Task<IActionResult> Index()
        {
            var regions = await _context.Regions.ToListAsync();
            return View(regions);
        }

        

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Region region)
        {
            if (ModelState.IsValid)
            {
                _context.Add(region);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }

        // GET: Region/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var region = await _context.Regions.FindAsync(id);
            if (region == null) return NotFound();

            return Json(region);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Region region)
        {
            if (id != region.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(region);
                    await _context.SaveChangesAsync();
                    return Json(new { success = true });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RegionExists(region.Id)) return NotFound();
                    throw;
                }
            }
            return Json(new { success = false });
        }

        // GET: Region/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var region = await _context.Regions
                .FirstOrDefaultAsync(m => m.Id == id);

            if (region == null) return NotFound();

            _context.Regions.Remove(region);
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        

        private bool RegionExists(int id) => _context.Regions.Any(e => e.Id == id);
    }
}
