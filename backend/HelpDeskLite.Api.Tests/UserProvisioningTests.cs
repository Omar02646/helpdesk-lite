using HelpDeskLite.Api.Data;
using HelpDeskLite.Api.Domain;
using HelpDeskLite.UserProvisioning;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HelpDeskLite.Api.Tests;

public sealed class UserProvisioningTests
{
    [Theory]
    [InlineData(AppRoles.Employee)]
    [InlineData(AppRoles.SupportAgent)]
    [InlineData(AppRoles.Manager)]
    public async Task Creates_user_with_exactly_selected_role(string role)
    {
        await using var provider = BuildProvider();
        using var scope = provider.CreateScope();
        var result = await scope.ServiceProvider.GetRequiredService<UserProvisioner>().CreateAsync($"Test {role}", $"{role.ToLowerInvariant()}@test.local", role, "StrongTest!123");
        Assert.True(result.Succeeded, result.Message);
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roles = await userManager.GetRolesAsync(result.User!);
        Assert.Equal([role], roles);
    }

    [Fact]
    public async Task Rejects_duplicate_email()
    {
        await using var provider = BuildProvider();
        using var scope = provider.CreateScope();
        var provisioner = scope.ServiceProvider.GetRequiredService<UserProvisioner>();
        var first = await provisioner.CreateAsync("First User", "duplicate@test.local", AppRoles.Employee, "StrongTest!123");
        var duplicate = await provisioner.CreateAsync("Second User", "duplicate@test.local", AppRoles.Manager, "StrongTest!123");
        Assert.True(first.Succeeded);
        Assert.False(duplicate.Succeeded);
        Assert.Contains("already exists", duplicate.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Rejects_unsupported_role()
    {
        await using var provider = BuildProvider();
        using var scope = provider.CreateScope();
        var result = await scope.ServiceProvider.GetRequiredService<UserProvisioner>().CreateAsync("Invalid Role", "invalid-role@test.local", "Admin", "StrongTest!123");
        Assert.False(result.Succeeded);
        Assert.Contains("Employee, SupportAgent, or Manager", result.Message);
    }

    [Fact]
    public async Task Enforces_shared_identity_password_policy()
    {
        await using var provider = BuildProvider();
        using var scope = provider.CreateScope();
        var result = await scope.ServiceProvider.GetRequiredService<UserProvisioner>().CreateAsync("Weak Password", "weak@test.local", AppRoles.Employee, "weak");
        Assert.False(result.Succeeded);
        Assert.Contains("10 characters", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    private static ServiceProvider BuildProvider()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDataProtection();
        services.AddDbContext<ApplicationDbContext>(options => options.UseInMemoryDatabase($"provisioning-{Guid.NewGuid():N}"));
        services.AddIdentityCore<ApplicationUser>(IdentityConfiguration.Configure).AddRoles<IdentityRole>().AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();
        services.AddScoped<UserProvisioner>();
        return services.BuildServiceProvider();
    }
}
