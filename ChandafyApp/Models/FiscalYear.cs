using System.ComponentModel.DataAnnotations;

namespace ChandafyApp.Models
{
    public class FiscalYear
    {
        [Key]
        public int Id { get; set; } // Primary Key

        [Required]
        public int Year { get; set; } // Fiscal Year (e.g., 2025)

        [Required]
        public DateTime StartDate { get; set; } // Start Date of the Fiscal Year

        [Required]
        public DateTime EndDate { get; set; } // End Date of the Fiscal Year

        // Navigation Properties
        public ICollection<AnnualNationalBudget> AnnualNationalBudgets { get; set; } // One-to-Many with AnnualNationalBudget
    }
}
