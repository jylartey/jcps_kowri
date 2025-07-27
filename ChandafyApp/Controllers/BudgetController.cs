using ChandafyApp.Data;
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
        private readonly UserManager<ApplicationUser> _userManager;

        public BudgetController(ChandafyDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Budget (My Budget)
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var member = await _context.Members.FirstOrDefaultAsync(m => m.IdentityUserId == user.Id);
            if (member == null)
            {
                
                return View(new List<Budget>()); // or redirect to an error page
            }
            var budgets = await _context.Budgets
                .Include(b => b.ChandaType)
                .Include(b => b.FiscalYear)
                .Where(b => b.MemberId == member.Id)
                .OrderBy(b => b.FiscalYear.Year)
                .ThenBy(b => b.Month)
                .ToListAsync();

            return View(budgets);
        }

        // GET: Budget/Upload
        [Authorize(Roles = "ItAdmin,Admin,Regional,Muhtamim")]
        public async Task<IActionResult> Upload()
        {
            ViewBag.FiscalYears = await _context.FiscalYears.ToListAsync();
            ViewBag.ChandaTypes = await _context.ChandaTypes.ToListAsync();
            return View();
        }

        // POST: Budget/Upload
        [HttpPost]
        [Authorize(Roles = "ItAdmin,Admin,Regional,Muhtamim")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(IFormFile file, int fiscalYearId, int chandaTypeId)
        {
            if (file == null || file.Length == 0)
            {
                ModelState.AddModelError("", "Please select a file to upload.");
                ViewBag.FiscalYears = await _context.FiscalYears.ToListAsync();
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
                                                           b.FiscalYearId == fiscalYearId &&
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
                                        FiscalYearId = fiscalYearId,
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
                ViewBag.FiscalYears = await _context.FiscalYears.ToListAsync();
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
    }
}
