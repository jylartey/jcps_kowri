using ChandafyApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;

namespace ChandafyApp.Controllers
{
    [Authorize]
    public class BudgetController : Controller
    {
        private readonly ChandafyDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public BudgetController(ChandafyDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Budget (My Budget)
        public async Task<IActionResult> Index()
        {
            var activeFiscalYear = await _context.GetActiveFiscalYearAsync();
            if (activeFiscalYear == null)
            {
                return View(new List<Budget>());
            }
            var user = await _userManager.GetUserAsync(User);
           
            var member = await _context.Members.FirstOrDefaultAsync(m => m.IdentityUserId == user.Id);
            if (member == null)
            {
                ViewBag.ActiveFiscalYear = activeFiscalYear;
                ViewBag.ChandaTypes = await _context.ChandaTypes.ToListAsync();
                ViewBag.FiscalYears = await _context.FiscalYears.Where(x => x.IsActive == true).OrderByDescending(fy => fy.Year).ToListAsync();
                ViewBag.Members = await _context.Members.ToListAsync();
                return View(new List<Budget>()); // or redirect to an error page
            }
            var budgets = await _context.Budgets
                .Include(b => b.ChandaType)
                .Include(b => b.FiscalYear)
                .Where(b => b.MemberId == member.Id && b.FiscalYearId == activeFiscalYear.Id)
                .OrderBy(b => b.FiscalYear.Year)
                .ThenBy(b => b.Month)
                .ToListAsync();

            ViewBag.ActiveFiscalYear = activeFiscalYear;
            ViewBag.ChandaTypes = await _context.ChandaTypes.ToListAsync();
            ViewBag.FiscalYears = await _context.FiscalYears.Where(x => x.IsActive == true).OrderByDescending(fy => fy.Year).ToListAsync();
            ViewBag.Members = await _context.Members.ToListAsync();

            return View(budgets);
        }

        // GET: Budget/Upload
        [Authorize(Roles = "ItAdmin,Admin,Regional,Muhtamim")]
        public async Task<IActionResult> Upload()
        {
            var activeFiscalYear = await _context.GetActiveFiscalYearAsync();
            ViewBag.FiscalYears = new List<FiscalYear> { activeFiscalYear };
            ViewBag.ChandaTypes = await _context.ChandaTypes.ToListAsync();
            return View();
        }

        // POST: Budget/Upload
        [HttpPost]
        [Authorize(Roles = "ItAdmin,Admin,Regional,Muhtamim")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(IFormFile file, int chandaTypeId)
        {
            var activeFiscalYear = await _context.GetActiveFiscalYearAsync();
            if (activeFiscalYear == null)
            {
                ModelState.AddModelError("", "No active fiscal year found.");
                ViewBag.ChandaTypes = await _context.ChandaTypes.ToListAsync();
                return View();
            }

            if (file == null || file.Length == 0)
            {
                ModelState.AddModelError("", "Please select a file to upload.");
                ViewBag.ChandaTypes = await _context.ChandaTypes.ToListAsync();
                return View();
            }

            try
            {
                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);
                    stream.Position = 0;

                    using (var package = new ExcelPackage(stream))
                    {
                        var worksheet = package.Workbook.Worksheets[0];
                        var rowCount = worksheet.Dimension.Rows;

                        for (int row = 2; row <= rowCount; row++) // Assuming row 1 is header
                        {
                            var memberAIMS = worksheet.Cells[row, 1].Value?.ToString();
                            var month = int.Parse(worksheet.Cells[row, 2].Value?.ToString());
                            var amount = decimal.Parse(worksheet.Cells[row, 3].Value?.ToString());

                            var member = await _context.Members.FirstOrDefaultAsync(m => m.AIMS == memberAIMS);
                            if (member != null)
                            {
                                var existingBudget = await _context.Budgets
                                    .FirstOrDefaultAsync(b => b.MemberId == member.Id &&
                                                           b.FiscalYearId == activeFiscalYear.Id &&
                                                           b.Month == month &&
                                                           b.ChandaTypeId == chandaTypeId);

                                if (existingBudget != null)
                                {
                                    existingBudget.Amount = amount;
                                }
                                else
                                {
                                    var budget = new Budget
                                    {
                                        MemberId = member.Id,
                                        FiscalYearId = activeFiscalYear.Id,
                                        ChandaTypeId = chandaTypeId,
                                        Month = month,
                                        Year = DateTime.Now.Year,
                                        Amount = amount,
                                        AmountPaid = 0
                                    };
                                    _context.Budgets.Add(budget);
                                }
                            }
                        }

                        await _context.SaveChangesAsync();
                    }
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error processing file: {ex.Message}");
                ViewBag.ChandaTypes = await _context.ChandaTypes.ToListAsync();
                return View();
            }
        }

        // GET: Budget/Calculator
        public async Task<IActionResult> Calculator()
        {
            ViewBag.ChandaTypes = await _context.ChandaTypes.ToListAsync();
            return View();
        }

        // POST: Budget/Calculator
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Calculator(decimal income, int chandaTypeId)
        {
            var chandaType = await _context.ChandaTypes.FindAsync(chandaTypeId);
            decimal calculatedAmount = 0;

            // Example calculation logic - replace with your actual rules
            if (chandaType.Name == "Khuddam Chanda")
            {
                calculatedAmount = income * 0.02m; // 2% of income
            }
            else if (chandaType.Name == "Atfal Chanda")
            {
                calculatedAmount = income * 0.01m; // 1% of income
            }
            // Add more conditions for other Chanda types

            ViewBag.ChandaTypes = await _context.ChandaTypes.ToListAsync();
            ViewBag.CalculatedAmount = calculatedAmount;
            ViewBag.SelectedChandaType = chandaType.Name;
            ViewBag.Income = income;

            return View();
        }
        [HttpPost]
        //[Authorize(Roles = "ItAdmin,Admin,Regional,Muhtamim")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create( Budget budget)
        {
            // Set AmountPaid to 0 for a new budget entry
            budget.AmountPaid = 0;

            // Get the active fiscal year if not provided, or validate it
            if (budget.FiscalYearId == 0)
            {
                var activeFy = await _context.GetActiveFiscalYearAsync();
                if (activeFy != null)
                {
                    budget.FiscalYearId = activeFy.Id;
                    budget.Year = activeFy.Year;
                }
                else
                {
                    return Json(new { success = false, message = "No active fiscal year found. Please set one before creating a budget." });
                }
            }
            else
            {
                // Ensure the provided FiscalYearId is valid and set the Year property correctly
                var selectedFy = await _context.FiscalYears.FindAsync(budget.FiscalYearId);
                if (selectedFy == null)
                {
                    return Json(new { success = false, message = "Invalid Fiscal Year selected." });
                }
                budget.Year = selectedFy.Year;
            }

            // Check for duplicate entry based on MemberId, ChandaTypeId, FiscalYearId, Month
            var existingBudget = await _context.Budgets
                .AnyAsync(b => b.MemberId == budget.MemberId &&
                               b.ChandaTypeId == budget.ChandaTypeId &&
                               b.FiscalYearId == budget.FiscalYearId &&
                               b.Month == budget.Month);

            if (existingBudget)
            {
                return Json(new { success = false, message = "A budget entry for this member, chanda type, and month already exists in the selected fiscal year." });
            }


            if (ModelState.IsValid)
            {
                _context.Add(budget);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Budget entry created successfully." });
            }

            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            return Json(new { success = false, message = "Invalid data submitted.", errors = errors });
        }


        // GET: Budget/Edit/5 (For fetching data for the edit modal)
        //[Authorize(Roles = "ItAdmin,Admin,Regional,Muhtamim")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var budget = await _context.Budgets.FindAsync(id);
            if (budget == null)
            {
                return NotFound();
            }

            // Pass necessary data for dropdowns in the modal
            ViewBag.ChandaTypes = await _context.ChandaTypes.ToListAsync();
            ViewBag.FiscalYears = await _context.FiscalYears.Where(x=>x.IsActive == true).OrderByDescending(fy => fy.Year).ToListAsync();
            ViewBag.Members = await _context.Members.ToListAsync();

            return Json(budget); 
        }

        // POST: Budget/Edit/5
        [HttpPost]
        //[Authorize(Roles = "ItAdmin,Admin,Regional,Muhtamim")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Budget budget)
        {
            if (id != budget.Id)
            {
                return NotFound();
            }

            // Ensure the provided FiscalYearId is valid and set the Year property correctly
            var selectedFy = await _context.FiscalYears.FindAsync(budget.FiscalYearId);
            if (selectedFy == null)
            {
                return Json(new { success = false, message = "Invalid Fiscal Year selected." });
            }
            budget.Year = selectedFy.Year;


            if (ModelState.IsValid)
            {
                try
                {
                    // Check for duplicate entry for a different Id
                    var existingBudget = await _context.Budgets
                        .AnyAsync(b => b.MemberId == budget.MemberId &&
                                       b.ChandaTypeId == budget.ChandaTypeId &&
                                       b.FiscalYearId == budget.FiscalYearId &&
                                       b.Month == budget.Month &&
                                       b.Id != budget.Id); // Exclude the current budget being edited

                    if (existingBudget)
                    {
                        return Json(new { success = false, message = "A budget entry for this member, chanda type, and month already exists in the selected fiscal year." });
                    }

                    _context.Update(budget);
                    await _context.SaveChangesAsync();
                    return Json(new { success = true, message = "Budget entry updated successfully." });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BudgetExists(budget.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            return Json(new { success = false, message = "Invalid data submitted.", errors = errors });
        }

        // POST: Budget/Delete/5
        [HttpPost]
        //[Authorize(Roles = "ItAdmin,Admin,Regional,Muhtamim")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var budget = await _context.Budgets.FindAsync(id);
            if (budget == null)
            {
                return Json(new { success = false, message = "Budget entry not found." });
            }

            _context.Budgets.Remove(budget);
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Budget entry deleted successfully." });
        }

        private bool BudgetExists(int id)
        {
            return _context.Budgets.Any(e => e.Id == id);
        }

    }
}
