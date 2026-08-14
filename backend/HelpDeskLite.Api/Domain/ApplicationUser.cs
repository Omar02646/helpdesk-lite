using Microsoft.AspNetCore.Identity;
namespace HelpDeskLite.Api.Domain;
public sealed class ApplicationUser : IdentityUser { public required string DisplayName { get; set; } }
