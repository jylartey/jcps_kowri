using ChandafyApp.Data;
using ChandafyApp.Models;
using ChandafyApp.NewFolder;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using AccountSummary = ChandafyApp.NewFolder.AccountSummary;

namespace ChandafyApp.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ChandafyDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        public HomeController(ILogger<HomeController> logger, ChandafyDbContext dbContext, UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            _context = dbContext;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(int? fiscalYearId)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            // Get or set current fiscal year
            var fiscalYear = await GetCurrentFiscalYear(fiscalYearId);
            var fiscalYears = await _context.FiscalYears.OrderByDescending(f => f.Year).ToListAsync();

            AccountSummary accountSummary = null;
            List<Payment> recentPayments = new();
            Dictionary<string, decimal> chandaTypePayments = new();
            List<decimal> monthlyBudget = new();
            List<decimal> monthlyPayments = new();

            if (currentUser != null && fiscalYear != null)
            {
                accountSummary = await GetAccountSummary(currentUser.Id, fiscalYear.Id);
                recentPayments = await GetRecentPayments(currentUser.Id);
                chandaTypePayments = await GetChandaTypePayments(currentUser.Id, fiscalYear.Id);
                (monthlyBudget, monthlyPayments) = await GetMonthlyData(currentUser.Id, fiscalYear.Id);
            }

            var viewModel = new DashboardViewModel
            {
                AccountSummary = accountSummary ?? new AccountSummary(),
                CurrentFiscalYear = fiscalYear,
                FiscalYears = fiscalYears,
                RecentPayments = recentPayments,
                ChandaTypePayments = chandaTypePayments,
                MonthlyBudget = monthlyBudget,
                MonthlyPayments = monthlyPayments
            };

            return View(viewModel);
        }


        private async Task<FiscalYear> GetCurrentFiscalYear(int? fiscalYearId)
        {
            if (fiscalYearId.HasValue)
            {
                return await _context.FiscalYears.FindAsync(fiscalYearId.Value);
            }

            var currentDate = DateTime.Now;
            return await _context.FiscalYears
                .FirstOrDefaultAsync(f => f.StartDate <= currentDate && f.EndDate >= currentDate)
                ?? await _context.FiscalYears.OrderByDescending(f => f.Year).FirstOrDefaultAsync();
        }

        private async Task<AccountSummary> GetAccountSummary(string userId, int fiscalYearId)
        {
            var fiscalYear = await _context.FiscalYears.FindAsync(fiscalYearId);
            if (fiscalYear == null)
                return null;

            var totalPayments = await _context.Payments
                .Where(p => p.UserId == userId &&
                            p.PaymentDate >= fiscalYear.StartDate &&
                            p.PaymentDate <= fiscalYear.EndDate)
                .SumAsync(p => p.Amount);

            var totalExpectedBudget = await _context.Budgets
                .Where(b => b.UserId == userId && b.FiscalYearId == fiscalYearId)
                .SumAsync(b => b.Amount);

            return new AccountSummary
            {
                TotalPayments = totalPayments,
                TotalExpectedBudget = totalExpectedBudget,
                BalanceLeft = totalExpectedBudget - totalPayments,
                PercentageComplete = totalExpectedBudget > 0 ? totalPayments / totalExpectedBudget : 0
            };
        }

        private async Task<List<Payment>> GetRecentPayments(string userId)
        {
            return await _context.Payments
                .Include(p => p.ChandaType)
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.PaymentDate)
                .Take(5)
                .ToListAsync();
        }


        private async Task<Dictionary<string, decimal>> GetChandaTypePayments(string userId, int fiscalYearId)
        {
            var fiscalYear = await _context.FiscalYears.FindAsync(fiscalYearId);

            return await _context.Payments
                .Include(p => p.ChandaType)
                .Where(p => p.UserId == userId &&
                            p.PaymentDate >= fiscalYear.StartDate &&
                            p.PaymentDate <= fiscalYear.EndDate)
                .GroupBy(p => p.ChandaType.Name)
                .Select(g => new { ChandaType = g.Key, Total = g.Sum(p => p.Amount) })
                .ToDictionaryAsync(x => x.ChandaType, x => x.Total);
        }

        private async Task<(List<decimal>, List<decimal>)> GetMonthlyData(string userId, int fiscalYearId)
        {
            var fiscalYear = await _context.FiscalYears.FindAsync(fiscalYearId);

            // Monthly budget
            var monthlyBudget = await _context.Budgets
                .Where(b => b.UserId == userId && b.FiscalYearId == fiscalYearId)
                .GroupBy(b => b.Month)
                .OrderBy(g => g.Key)
                .Select(g => g.Sum(b => b.Amount))
                .ToListAsync();

            while (monthlyBudget.Count < 12)
                monthlyBudget.Add(0);

            // Monthly payments
            var monthlyPayments = new List<decimal>();
            for (int month = 1; month <= 12; month++)
            {
                var startDate = new DateTime(fiscalYear.Year, month, 1);
                var endDate = startDate.AddMonths(1).AddDays(-1);

                var total = await _context.Payments
                    .Where(p => p.UserId == userId &&
                                p.PaymentDate >= startDate &&
                                p.PaymentDate <= endDate)
                    .SumAsync(p => (decimal?)p.Amount) ?? 0;

                monthlyPayments.Add(total);
            }

            return (monthlyBudget, monthlyPayments);
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
