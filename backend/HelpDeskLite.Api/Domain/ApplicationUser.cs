using Microsoft.AspNetCore.Identity;
namespace HelpDeskLite.Api.Domain;
public sealed class ApplicationUser : IdentityUser
{
    public required string DisplayName { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string PreferredName => string.Join(' ', new[] { FirstName, LastName }.Where(x => !string.IsNullOrWhiteSpace(x))) is { Length: > 0 } name ? name : DisplayName;
}
