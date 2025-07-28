using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChandafyApp.ViewComponents
{
    public class FiscalYearViewComponent:ViewComponent
    {
        private readonly ChandafyDbContext _context;

        public FiscalYearViewComponent(ChandafyDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            // Fetch the single active fiscal year
            var activeFiscalYear = await _context.FiscalYears.FirstOrDefaultAsync(fy => fy.IsActive == true);
            // Pass it to the component's default view
            return View(activeFiscalYear);
        }
    }
}
