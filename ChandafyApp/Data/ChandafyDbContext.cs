using ChandafyApp.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

public class ChandafyDbContext : DbContext
{
    public DbSet<Region> Regions { get; set; }
    public DbSet<Zone> Zones { get; set; }
    public DbSet<Circuit> Circuits { get; set; }
    public DbSet<Jamaat> Jamaats { get; set; }
    public DbSet<Member> Members { get; set; }
    public DbSet<Budget> Budgets { get; set; }
    public DbSet<ChandaType> ChandaTypes { get; set; }
    public DbSet<FiscalYear> FiscalYears { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<PaymentMethod> PaymentMethods { get; set; }
    public DbSet<AccountSummary> AccountSummaries { get; set; }
    public DbSet<Expense> Expenses { get; set; }
    public DbSet<Wallet> Wallets { get; set; }
    public DbSet<Income> Incomes { get; set; }
    public DbSet<AnnualNationalBudget> AnnualNationalBudgets { get; set; }
    public DbSet<ReceiptRequest> ReceiptRequests { get; set; }

    public ChandafyDbContext(DbContextOptions<ChandafyDbContext> options)
        : base(options)
    {
    }

    public async Task<FiscalYear> GetActiveFiscalYearAsync()
    {
        return await FiscalYears.FirstOrDefaultAsync(f => f.IsActive == true)
               ?? await FiscalYears.OrderByDescending(f => f.Year).FirstOrDefaultAsync();
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Region -> Zone (One-to-Many)
        modelBuilder.Entity<Zone>()
            .HasOne(z => z.Region)
            .WithMany(r => r.Zones)
            .HasForeignKey(z => z.RegionId)
            .OnDelete(DeleteBehavior.Cascade);

        // Zone -> Circuit (One-to-Many)
        modelBuilder.Entity<Circuit>()
            .HasOne(c => c.Zone)
            .WithMany(z => z.Circuits)
            .HasForeignKey(c => c.ZoneId)
            .OnDelete(DeleteBehavior.Cascade);

        // Circuit -> Jamaat (One-to-Many)
        modelBuilder.Entity<Jamaat>()
            .HasOne(j => j.Circuit)
            .WithMany(c => c.Jamaats)
            .HasForeignKey(j => j.CircuitId)
            .OnDelete(DeleteBehavior.Cascade);

        // Jamaat -> Member (One-to-Many)
        modelBuilder.Entity<Member>()
            .HasOne(m => m.Jamaat)
            .WithMany(j => j.Members)
            .HasForeignKey(m => m.JamaatId)
            .OnDelete(DeleteBehavior.Cascade);

        // Member -> IdentityUser (One-to-One)
        modelBuilder.Entity<Member>()
            .HasOne(m => m.IdentityUser)
            .WithMany()
            .HasForeignKey(m => m.IdentityUserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Member -> Budget (One-to-Many)
        modelBuilder.Entity<Budget>()
            .HasOne(b => b.Member)
            .WithMany()
            .HasForeignKey(b => b.MemberId)
            .OnDelete(DeleteBehavior.Cascade);

        // Budget -> ChandaType (One-to-Many)
        modelBuilder.Entity<Budget>()
            .HasOne(b => b.ChandaType)
            .WithMany()
            .HasForeignKey(b => b.ChandaTypeId)
            .OnDelete(DeleteBehavior.Cascade);

        // Budget -> FiscalYear (One-to-Many)
        modelBuilder.Entity<Budget>()
            .HasOne(b => b.FiscalYear)
            .WithMany()
            .HasForeignKey(b => b.FiscalYearId)
            .OnDelete(DeleteBehavior.Cascade);

        // Payment -> FiscalYear (Many-to-One)
        modelBuilder.Entity<Payment>()
            .HasOne(p => p.FiscalYear)
            .WithMany()
            .HasForeignKey(p => p.FiscalYearId)
            .OnDelete(DeleteBehavior.Cascade);

        // Payment -> ChandaType (One-to-Many)
        modelBuilder.Entity<Payment>()
            .HasOne(p => p.ChandaType)
            .WithMany()
            .HasForeignKey(p => p.ChandaTypeId)
            .OnDelete(DeleteBehavior.Cascade);

        // Payment -> PaymentMethod (One-to-Many)
        modelBuilder.Entity<Payment>()
            .HasOne(p => p.PaymentMethod)
            .WithMany()
            .HasForeignKey(p => p.PaymentMethodId)
            .OnDelete(DeleteBehavior.Cascade);

        // ReceiptRequest -> Payment (One-to-One)
        modelBuilder.Entity<ReceiptRequest>()
            .HasOne(r => r.Payment)
            .WithMany()
            .HasForeignKey(r => r.PaymentId)
            .OnDelete(DeleteBehavior.Cascade);

        // ReceiptRequest -> Member (RequestBy and ApprovedBy)
        modelBuilder.Entity<ReceiptRequest>()
            .HasOne(r => r.RequestBy)
            .WithMany()
            .HasForeignKey(r => r.RequestById)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ReceiptRequest>()
            .HasOne(r => r.ApprovedBy)
            .WithMany()
            .HasForeignKey(r => r.ApprovedById)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
