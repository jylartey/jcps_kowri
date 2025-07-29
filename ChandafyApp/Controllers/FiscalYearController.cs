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
            var fiscalYears = await _context.FiscalYears.OrderByDescending(x => x.Period).ToListAsync();
            return View(fiscalYears);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FiscalYear fiscalYear)
        {
            if (ModelState.IsValid)
            {
                // Check if we're trying to set this as active
                if (fiscalYear.IsActive == true)
                {
                    // Check if there's already an active fiscal year
                    var activeYearExists = await _context.FiscalYears.AnyAsync(f => f.IsActive == true);
                    if (activeYearExists)
                    {
                        return Json(new { success = false, message = "There is already an active fiscal year. Only one fiscal year can be active at a time." });
                    }
                }

                _context.Add(fiscalYear);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            return Json(new { success = false, message = "Invalid data submitted." });
        }

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
                    // Check if we're trying to set this as active
                    if (fiscalYear.IsActive == true)
                    {
                        // Check if there's already an active fiscal year that's not this one
                        var activeYear = await _context.FiscalYears
                            .FirstOrDefaultAsync(f => f.IsActive == true && f.Id != id);

                        if (activeYear != null)
                        {
                            return Json(new
                            {
                                success = false,
                                message = $"Cannot activate this fiscal year. {activeYear.Period} is already active."
                            });
                        }
                    }

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
            return Json(new { success = false, message = "Invalid data submitted." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var fiscalYear = await _context.FiscalYears.FirstOrDefaultAsync(m => m.Id == id);
            if (fiscalYear == null) return NotFound();

            // Prevent deletion of active fiscal year
            if (fiscalYear.IsActive == true)
            {
                return Json(new
                {
                    success = false,
                    message = "Cannot delete the active fiscal year. Please deactivate it first."
                });
            }

            _context.FiscalYears.Remove(fiscalYear);
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Activate(int id)
        {
            try
            {
                // Get the fiscal year to activate
                var fiscalYearToActivate = await _context.FiscalYears.FindAsync(id);
                if (fiscalYearToActivate == null)
                {
                    return Json(new { success = false, message = "Fiscal year not found" });
                }

                // Deactivate all other fiscal years
                var allFiscalYears = await _context.FiscalYears.ToListAsync();
                foreach (var fy in allFiscalYears)
                {
                    fy.IsActive = (fy.Id != id);
                    if (fy.IsActive == true)
                    {
                        fy.IsActive = false; // Ensure only one fiscal year is active
                    }
                }

                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    message = $"Fiscal year {fiscalYearToActivate.Period} activated successfully.", 
                    year = fiscalYearToActivate.Period,
                    startDate = fiscalYearToActivate.StartDate.ToString("MMM d, yyyy"),
                    endDate = fiscalYearToActivate.EndDate.ToString("MMM d, yyyy")
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deactivate(int id)
        {
            try
            {
                var fiscalYear = await _context.FiscalYears.FindAsync(id);
                if (fiscalYear == null)
                {
                    return Json(new { success = false, message = "Fiscal year not found" });
                }

                fiscalYear.IsActive = false;
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = $"Fiscal year {fiscalYear.Period} deactivated successfully."  });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        private bool FiscalYearExists(int id) => _context.FiscalYears.Any(e => e.Id == id);
    }
}