using Microsoft.AspNetCore.Identity;
using OMS.Models;

namespace OMS.Data
{
    /// <summary>
    /// Seeds default roles (Admin, Staff, Viewer) and a default Admin user.
    /// </summary>
    public static class IdentitySeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            // ── Create roles ─────────────────────────────────────────────────
            foreach (var role in AppRoles.All)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // ── Create default Admin account ─────────────────────────────────
            const string adminEmail = "admin@oms.local";
            const string adminPassword = "Admin@123";

            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "Quản trị viên",
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(adminUser, adminPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, AppRoles.Admin);
                }
            }

            // ── Create default Staff account ─────────────────────────────────
            const string staffEmail = "staff@oms.local";
            const string staffPassword = "Staff@123";

            var staffUser = await userManager.FindByEmailAsync(staffEmail);
            if (staffUser == null)
            {
                staffUser = new ApplicationUser
                {
                    UserName = staffEmail,
                    Email = staffEmail,
                    FullName = "Nhân viên",
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(staffUser, staffPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(staffUser, AppRoles.Staff);
                }
            }

            // ── Create default Viewer account ────────────────────────────────
            const string viewerEmail = "viewer@oms.local";
            const string viewerPassword = "Viewer@123";

            var viewerUser = await userManager.FindByEmailAsync(viewerEmail);
            if (viewerUser == null)
            {
                viewerUser = new ApplicationUser
                {
                    UserName = viewerEmail,
                    Email = viewerEmail,
                    FullName = "Người xem",
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(viewerUser, viewerPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(viewerUser, AppRoles.Viewer);
                }
            }
        }
    }
}
