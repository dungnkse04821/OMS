using Microsoft.AspNetCore.Identity;

namespace OMS.Models
{
    /// <summary>
    /// Custom user class extending IdentityUser with application-specific fields.
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Static list of application roles.
    /// </summary>
    public static class AppRoles
    {
        public const string Admin = "Admin";
        public const string Staff = "Staff";
        public const string Viewer = "Viewer";

        public static readonly string[] All = { Admin, Staff, Viewer };
    }
}
