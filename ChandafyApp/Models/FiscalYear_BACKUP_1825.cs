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

<<<<<<< HEAD
        public string Period { get; set; } // Fiscal Year Period (e.g., "2025-2026")

        public bool? IsActive {  get; set; } // Set Fiscal Year Active
=======
        public bool ?IsActive {  get; set; } // Set Fiscal Year Active
>>>>>>> 588664ca709a6be74f9f26a9118146a7748b5579

        // Navigation Properties
        public ICollection<AnnualNationalBudget> AnnualNationalBudgets { get; set; } // One-to-Many with AnnualNationalBudget
    }
}
