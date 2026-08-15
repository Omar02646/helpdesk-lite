using System.Net.Mail;
using HelpDeskLite.Api.Domain;
using Microsoft.AspNetCore.Identity;

namespace HelpDeskLite.UserProvisioning;

public sealed record ProvisioningResult(bool Succeeded, string Message, ApplicationUser? User = null, string? Role = null);

public sealed class UserProvisioner(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
{
    public static readonly IReadOnlyList<string> ValidRoles = AppRoles.All;

    public async Task<ProvisioningResult> CreateAsync(string displayName, string email, string role, string password)
    {
        displayName = displayName.Trim();
        email = email.Trim();
        var canonicalRole = ValidRoles.SingleOrDefault(candidate => string.Equals(candidate, role.Trim(), StringComparison.OrdinalIgnoreCase));
        if (string.IsNullOrWhiteSpace(displayName)) return new(false, "Display name is required.");
        if (displayName.Length > 160) return new(false, "Display name must be 160 characters or fewer.");
        if (!MailAddress.TryCreate(email, out var address) || !string.Equals(address.Address, email, StringComparison.OrdinalIgnoreCase)) return new(false, "Enter a valid email address.");
        if (canonicalRole is null) return new(false, "Role must be Employee, SupportAgent, or Manager.");
        if (await userManager.FindByEmailAsync(email) is not null) return new(false, "An account with this email already exists.");

        foreach (var expectedRole in ValidRoles)
        {
            if (!await roleManager.RoleExistsAsync(expectedRole))
            {
                var roleResult = await roleManager.CreateAsync(new IdentityRole(expectedRole));
                if (!roleResult.Succeeded) return new(false, "Unable to ensure application roles: " + Describe(roleResult));
            }
        }

        var user = new ApplicationUser { DisplayName = displayName, Email = email, UserName = email, EmailConfirmed = true };
        var createResult = await userManager.CreateAsync(user, password);
        if (!createResult.Succeeded) return new(false, Describe(createResult));

        var assignmentResult = await userManager.AddToRoleAsync(user, canonicalRole);
        if (!assignmentResult.Succeeded)
        {
            var rollback = await userManager.DeleteAsync(user);
            var suffix = rollback.Succeeded ? " The incomplete account was removed." : " The incomplete account could not be removed; inspect it before retrying.";
            return new(false, "Account role assignment failed: " + Describe(assignmentResult) + suffix);
        }

        return new(true, "User created successfully.", user, canonicalRole);
    }

    private static string Describe(IdentityResult result) => string.Join("; ", result.Errors.Select(error => error.Description));
}
