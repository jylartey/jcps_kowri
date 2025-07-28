using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting; // Make sure this is included
using Microsoft.AspNetCore.Http; // Make sure this is included
using ChandafyApp.Data; // Assuming your DbContext is here
using ChandafyApp.Models; // Assuming your Expense and FiscalYear models are here

namespace ChandafyApp.Controllers
{
    [Authorize(Roles = "ItAdmin,Admin,Regional,Muhtamim")]
    public class ExpenseController : Controller
    {
        private readonly ChandafyDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ExpenseController(ChandafyDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // GET: Expense
        public async Task<IActionResult> Index()
        {
            var activeFiscalYear = await _context.GetActiveFiscalYearAsync();
            if (activeFiscalYear == null)
            {
                return View(new List<Expense>());
            }
            var expenses = await _context.Expenses
                .Include(e => e.FiscalYear)
                .Where(e => e.FiscalYearId == activeFiscalYear.Id)
                .ToListAsync();

            // FIX: Populate ViewBag.FiscalYears in the Index action
            ViewBag.FiscalYears = new List<FiscalYear> { activeFiscalYear };

            return View(expenses);
        }

        

        // POST: Expense/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Expense expense, IFormFile receiptImage)
        {
            var activeFiscalYear = await _context.GetActiveFiscalYearAsync();
            if (activeFiscalYear == null)
            {
                return Json(new { success = false, errors = new List<string> { "No active fiscal year found." } });
            }

            expense.FiscalYearId = activeFiscalYear.Id;

            if (ModelState.IsValid)
            {
                if (receiptImage != null && receiptImage.Length > 0)
                {
                    var uploads = Path.Combine(_env.WebRootPath, "uploads/receipts");
                    // Ensure the directory exists
                    if (!Directory.Exists(uploads))
                    {
                        Directory.CreateDirectory(uploads);
                    }
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(receiptImage.FileName);
                    var filePath = Path.Combine(uploads, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await receiptImage.CopyToAsync(stream);
                    }

                    expense.ExpenseReceiptImage = fileName;
                }

                _context.Add(expense);
                await _context.SaveChangesAsync();
                // Return a JSON response for AJAX success
                return Json(new { success = true });
            }
            // If ModelState is not valid, return JSON with errors
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return Json(new { success = false, errors = errors });
        }

        // GET: Expense/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var expense = await _context.Expenses.FindAsync(id);
            if (expense == null) return NotFound();

            ViewBag.FiscalYears = await _context.FiscalYears.ToListAsync();
            // For AJAX call, return JSON of the expense data
            return Json(new
            {
                id = expense.Id,
                expenseType = expense.ExpenseType,
                totalExpectedAmount = expense.TotalExpectedAmount,
                expenseDescription = expense.ExpenseDescription,
                fiscalYearId = expense.FiscalYearId,
                expenseReceiptImage = expense.ExpenseReceiptImage
            });
        }

        // POST: Expense/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Expense expense, IFormFile receiptImage)
        {
            if (id != expense.Id) return Json(new { success = false, errors = new List<string> { "Expense ID mismatch." } });

            // Retrieve the existing entity to handle image deletion correctly
            var existingExpense = await _context.Expenses.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id);
            if (existingExpense == null) return Json(new { success = false, errors = new List<string> { "Expense not found." } });

            if (ModelState.IsValid)
            {
                try
                {
                    if (receiptImage != null && receiptImage.Length > 0)
                    {
                        var uploads = Path.Combine(_env.WebRootPath, "uploads/receipts");
                        // Ensure the directory exists
                        if (!Directory.Exists(uploads))
                        {
                            Directory.CreateDirectory(uploads);
                        }

                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(receiptImage.FileName);
                        var filePath = Path.Combine(uploads, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await receiptImage.CopyToAsync(stream);
                        }

                        // Delete old image if exists
                        if (!string.IsNullOrEmpty(existingExpense.ExpenseReceiptImage))
                        {
                            var oldFilePath = Path.Combine(uploads, existingExpense.ExpenseReceiptImage);
                            if (System.IO.File.Exists(oldFilePath))
                            {
                                System.IO.File.Delete(oldFilePath);
                            }
                        }
                        expense.ExpenseReceiptImage = fileName;
                    }
                    else // If no new image is uploaded, retain the existing one
                    {
                        expense.ExpenseReceiptImage = existingExpense.ExpenseReceiptImage;
                    }

                    _context.Update(expense);
                    await _context.SaveChangesAsync();
                    return Json(new { success = true });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ExpenseExists(expense.Id))
                    {
                        return Json(new { success = false, errors = new List<string> { "Expense not found." } });
                    }
                    throw; // Re-throw if it's a real concurrency issue not related to not found
                }
            }
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return Json(new { success = false, errors = errors });
        }


        // GET: Expense/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var expense = await _context.Expenses
                .Include(e => e.FiscalYear)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (expense == null)
            {
                return Json(new { success = false, errors = new List<string> { "Expense not found." } });
            }

            if (!string.IsNullOrEmpty(expense.ExpenseReceiptImage))
            {
                var uploads = Path.Combine(_env.WebRootPath, "uploads/receipts");
                var filePath = Path.Combine(uploads, expense.ExpenseReceiptImage);
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            _context.Expenses.Remove(expense);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }


       

        private bool ExpenseExists(int id) => _context.Expenses.Any(e => e.Id == id);
    }
}