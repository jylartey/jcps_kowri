using System.ComponentModel.DataAnnotations;

namespace ChandafyApp.Models
{
    public class ChandaType
    {
        [Key]
        public int Id { get; set; } // Primary Key

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } // Name of the Chanda Type

        [MaxLength(500)]
        public string Description { get; set; } // Optional description
    }
}
