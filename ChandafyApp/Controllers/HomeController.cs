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
        private readonly UserManager<IdentityUser> _userManager;
        public HomeController(ILogger<HomeController> logger, ChandafyDbContext dbContext, UserManager<IdentityUser> userManager)
        {
            _logger = logger;
            _context = dbContext;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(int? fiscalYearId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var member = await _context.Members
                .Include(m => m.Jamaat)
                .ThenInclude(j => j.Circuit)
                .ThenInclude(c => c.Zone)
                .FirstOrDefaultAsync(m => m.IdentityUserId == currentUser.Id);

            // Get or set current fiscal year
            var fiscalYear = await GetCurrentFiscalYear(fiscalYearId);
            var fiscalYears = await _context.FiscalYears.OrderByDescending(f => f.Year).ToListAsync();
            AccountSummary accountSummary = null;
            List<Payment> recentPayments = new();
            Dictionary<string, decimal> chandaTypePayments = new();
            List<decimal> monthlyBudget = new();
            List<decimal> monthlyPayments = new();
            if (member != null && fiscalYear != null)
            {
                accountSummary = await GetAccountSummary(member.Id, fiscalYear.Id);
                recentPayments = await GetRecentPayments(member.Id);
                chandaTypePayments = await GetChandaTypePayments(member.Id, fiscalYear.Id);
                (monthlyBudget, monthlyPayments) = await GetMonthlyData(member.Id, fiscalYear.Id);
            }
            //// Get account summary
            //var accountSummary = await GetAccountSummary(member.Id, fiscalYear.Id);

            //// Get recent payments
            //var recentPayments = await GetRecentPayments(member.Id);

            //// Get payments by Chanda type
            //var chandaTypePayments = await GetChandaTypePayments(member.Id, fiscalYear.Id);

            //// Get monthly budget and payments
            //var (monthlyBudget, monthlyPayments) = await GetMonthlyData(member.Id, fiscalYear.Id);

            var viewModel = new DashboardViewModel
            {
                AccountSummary = accountSummary ?? new AccountSummary(),
                CurrentFiscalYear = fiscalYear, // can still be null, but we'll guard in the view
                FiscalYears = fiscalYears ?? new List<FiscalYear>(),
                RecentPayments = recentPayments ?? new List<Payment>(),
                ChandaTypePayments = chandaTypePayments ?? new Dictionary<string, decimal>(),
                MonthlyBudget = monthlyBudget ?? new List<decimal>(new decimal[12]),
                MonthlyPayments = monthlyPayments ?? new List<decimal>(new decimal[12])
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

        private async Task<AccountSummary> GetAccountSummary(int memberId, int fiscalYearId)
        {
            var fiscalYear = await _context.FiscalYears.FindAsync(fiscalYearId);
            if (fiscalYear == null)
                return null;
            var totalPayments = await _context.Payments
                .Where(p => p.MemberId == memberId &&
                           p.PaymentDate >= _context.FiscalYears.Find(fiscalYearId).StartDate &&
                           p.PaymentDate <= _context.FiscalYears.Find(fiscalYearId).EndDate)
                .SumAsync(p => p.Amount);

            var totalExpectedBudget = await _context.Budgets
                .Where(b => b.MemberId == memberId && b.FiscalYearId == fiscalYearId)
                .SumAsync(b => b.Amount);

            return new AccountSummary
            {
                TotalPayments = totalPayments,
                TotalExpectedBudget = totalExpectedBudget,
                BalanceLeft = totalExpectedBudget - totalPayments,
                PercentageComplete = totalExpectedBudget > 0 ? totalPayments / totalExpectedBudget : 0
            };
        }

        private async Task<List<Payment>> GetRecentPayments(int memberId)
        {
            return await _context.Payments
                .Include(p => p.ChandaType)
                .Where(p => p.MemberId == memberId)
                .OrderByDescending(p => p.PaymentDate)
                .Take(5)
                .ToListAsync();
        }

        private async Task<Dictionary<string, decimal>> GetChandaTypePayments(int memberId, int fiscalYearId)
        {
            var fiscalYear = await _context.FiscalYears.FindAsync(fiscalYearId);

            return await _context.Payments
                .Include(p => p.ChandaType)
                .Where(p => p.MemberId == memberId &&
                           p.PaymentDate >= fiscalYear.StartDate &&
                           p.PaymentDate <= fiscalYear.EndDate)
                .GroupBy(p => p.ChandaType.Name)
                .Select(g => new { ChandaType = g.Key, Total = g.Sum(p => p.Amount) })
                .ToDictionaryAsync(x => x.ChandaType, x => x.Total);
        }

        private async Task<(List<decimal>, List<decimal>)> GetMonthlyData(int memberId, int fiscalYearId)
        {
            var fiscalYear = await _context.FiscalYears.FindAsync(fiscalYearId);

            // Get monthly budget
            var monthlyBudget = await _context.Budgets
                .Where(b => b.MemberId == memberId && b.FiscalYearId == fiscalYearId)
                .GroupBy(b => b.Month)
                .OrderBy(g => g.Key)
                .Select(g => g.Sum(b => b.Amount))
                .ToListAsync();

            // Ensure we have 12 months of data
            while (monthlyBudget.Count < 12)
            {
                monthlyBudget.Add(0);
            }

            // Get monthly payments
            var monthlyPayments = new List<decimal>();
            for (int month = 1; month <= 12; month++)
            {
                var startDate = new DateTime(fiscalYear.Year, month, 1);
                var endDate = startDate.AddMonths(1).AddDays(-1);

                var total = await _context.Payments
                    .Where(p => p.MemberId == memberId &&
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
