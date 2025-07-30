using System.ComponentModel.DataAnnotations;

namespace ChandafyApp.ViewModels
{
    public class MemberViewModel
    {
        public string Id { get; set; }
        public string AIMS { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string ContactNumber { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string JamaatName { get; set; }
        public int? JamaatId { get; set; }
        public List<string> Roles { get; set; }
    }
    public class MemberCreateViewModel
    {
        [Required]
        public string AIMS { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string ContactNumber { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        public int? JamaatId { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; }

        public List<string> SelectedRoles { get; set; }
    }

    public class MemberEditViewModel
    {
        public string Id { get; set; }

        [Required]
        public string AIMS { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string ContactNumber { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        public int? JamaatId { get; set; }

        public List<string> SelectedRoles { get; set; }
    }

    public class ResetPasswordViewModel
    {
        public string Id { get; set; }
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; }
    }
    public class MemberProfileViewModel
    {
        public string Id { get; set; }
        public string AIMS { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string ContactNumber { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string JamaatName { get; set; }
        public int? JamaatId { get; set; }
        public string CircuitName { get; set; }
        public string ZoneName { get; set; }
        public string RegionName { get; set; }
        public List<string> Roles { get; set; }
    }
}
