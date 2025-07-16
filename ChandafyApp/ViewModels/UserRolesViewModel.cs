using ChandafyApp.Controllers;

namespace ChandafyApp.ViewModels
{
    public class UserRolesViewModel
    {

        public string UserId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public List<string> Roles { get; set; }

    }
    public class EditRolesViewModel
    {
        public string UserId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public List<RoleSelection> Roles { get; set; }
    }
    public class RoleSelection
    {
        public string RoleName { get; set; }
        public bool IsSelected { get; set; }
    }
}
