using ChandafyApp.Models;
using Microsoft.AspNetCore.Identity;

namespace ChandafyApp.Data
{
    public class ApplicationUser : IdentityUser
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? ContactNumber { get; set; }
        public string? AIMS { get; set; }
        public DateTime? DateOfBirth { get; set; }

        public int? JamaatId { get; set; }
        public Jamaat? Jamaat { get; set; }
    }

}
