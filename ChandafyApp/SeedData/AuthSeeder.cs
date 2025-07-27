using ChandafyApp.Data;
using Microsoft.AspNetCore.Identity;

namespace ChandafyApp.SeedData;
public static class AuthSeeder
{
    public static async Task SeedUsersAndRoles(IServiceProvider serviceProvider)
    {
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        string adminEmail = "itAdmin@chandafy.com";
        string adminPassword = "Test123!";

        var roles = new[] { "ItAdmin" }; // Add to list

        // Create roles if they don't exist
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // Check if the admin user exists
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,

            };

            var result = await userManager.CreateAsync(adminUser, adminPassword);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "ItAdmin");
            }
            else
            {
                throw new Exception($"Failed to create ItAdmin user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }
    }
}
