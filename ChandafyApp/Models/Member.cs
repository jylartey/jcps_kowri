using ChandafyApp.Data;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace ChandafyApp.Models;

public class Member
{
    [Key]
    public int Id { get; set; }
    public string AIMS { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }

    public string ContactNumber { get; set; }
    public DateTime DateOfBirth { get; set; }

    // Foreign Key to Jamaat
    public int JamaatId { get; set; }

    // Foreign Key to IdentityUser
    //public string IdentityUserId { get; set; }

    // Navigation Properties
    //public ApplicationUser IdentityUser { get; set; }
    //public Jamaat Jamaat { get; set; }
}

