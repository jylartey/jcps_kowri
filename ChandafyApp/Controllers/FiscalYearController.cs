using ChandafyApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChandafyApp.Controllers
{
    [Authorize(Roles = "ItAdmin,Admin")]
    public class FiscalYearController : Controller
    {
        private readonly ChandafyDbContext _context;

        public FiscalYearController(ChandafyDbContext context)
        {
            _context = context;
        }

        // GET: FiscalYear
        public async Task<IActionResult> Index()
        {
            var fiscalYears = await _context.FiscalYears.OrderByDescending(x=>x.Year).ToListAsync();
            return View(fiscalYears);
        }

        // GET: FiscalYear/Create
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FiscalYear fiscalYear)
        {
            if (ModelState.IsValid)
            {
                _context.Add(fiscalYear);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }

        // GET: FiscalYear/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var fiscalYear = await _context.FiscalYears.FindAsync(id);
            if (fiscalYear == null) return NotFound();

            return Json(fiscalYear);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, FiscalYear fiscalYear)
        {
            if (id != fiscalYear.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(fiscalYear);
                    await _context.SaveChangesAsync();
                    return Json(new { success = true });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FiscalYearExists(fiscalYear.Id)) return NotFound();
                    throw;
                }
            }
            return Json(new { success = false });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var fiscalYear = await _context.FiscalYears
                .FirstOrDefaultAsync(m => m.Id == id);

            if (fiscalYear == null) return NotFound();

            _context.FiscalYears.Remove(fiscalYear);
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        

        private bool FiscalYearExists(int id) => _context.FiscalYears.Any(e => e.Id == id);
    }
}
