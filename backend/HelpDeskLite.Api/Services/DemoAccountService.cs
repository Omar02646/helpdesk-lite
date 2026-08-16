using HelpDeskLite.Api.Domain;

namespace HelpDeskLite.Api.Services;
public sealed class DemoAccountOptions
{
    public Dictionary<string,string> Accounts { get; set; } = new(StringComparer.Ordinal) {
        [AppRoles.Employee]="demo.employee@helpdesklite.local", [AppRoles.SupportAgent]="demo.agent@helpdesklite.local", [AppRoles.Manager]="demo.manager@helpdesklite.local" };
}
public sealed class DemoAccountService(Microsoft.Extensions.Options.IOptions<DemoAccountOptions> options)
{
    public bool TryGetEmail(string role,out string email)=>options.Value.Accounts.TryGetValue(role,out email!);
    public bool IsDemo(string? email)=>email is not null&&options.Value.Accounts.Values.Contains(email,StringComparer.OrdinalIgnoreCase);
}
