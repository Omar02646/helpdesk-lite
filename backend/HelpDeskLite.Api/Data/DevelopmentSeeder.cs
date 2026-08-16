using HelpDeskLite.Api.Domain;
using HelpDeskLite.Api.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HelpDeskLite.Api.Data;

public static class DevelopmentSeeder
{
    public static async Task SeedAsync(IServiceProvider services,IConfiguration config,ILogger logger)
    {
        using var scope=services.CreateScope();
        var db=scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await db.Database.MigrateAsync();
        await SeedIdentityAsync(scope.ServiceProvider,config,logger);
        if(await db.Tickets.AnyAsync())return;
        var userManager=scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var omar=(await userManager.FindByEmailAsync("omar@helpdesklite.local"))!;var ahmed=(await userManager.FindByEmailAsync("ahmed@helpdesklite.local"))!;var sara=(await userManager.FindByEmailAsync("sara@helpdesklite.local"))!;var now=DateTimeOffset.UtcNow;
        var tickets=new[]{
            new Ticket{TicketNumber="HDL-1001",Title="Laptop will not start",Description="My laptop powers on but the screen stays black. I have tried restarting and connecting an external monitor.",Category="IT Support",Status=TicketStatus.InProgress,Priority=TicketPriority.High,CreatedByUserId=omar.Id,AssignedToUserId=ahmed.Id,CreatedAt=now.AddHours(-8),UpdatedAt=now.AddHours(-6)},
            new Ticket{TicketNumber="HDL-1002",Title="VPN access problem",Description="I cannot connect to the company VPN from my laptop while working remotely.",Category="Network",Status=TicketStatus.Open,Priority=TicketPriority.Medium,CreatedByUserId=omar.Id,CreatedAt=now.AddHours(-5),UpdatedAt=now.AddHours(-5)},
            new Ticket{TicketNumber="HDL-1003",Title="Email not syncing",Description="Outlook has not received new messages since yesterday afternoon.",Category="Email",Status=TicketStatus.Resolved,Priority=TicketPriority.Low,CreatedByUserId=omar.Id,AssignedToUserId=sara.Id,CreatedAt=now.AddDays(-1),UpdatedAt=now.AddHours(-2),ResolvedAt=now.AddHours(-2)}};
        foreach(var ticket in tickets)ticket.StatusHistory.Add(new TicketStatusHistory{FromStatus=null,ToStatus=ticket.Status,ChangedByUserId=ticket.CreatedByUserId,ChangedAt=ticket.CreatedAt});
        db.Tickets.AddRange(tickets);await db.SaveChangesAsync();
    }

    public static async Task SeedIdentityAsync(IServiceProvider services,IConfiguration config,ILogger logger)
    {
        var roleManager=services.GetRequiredService<RoleManager<IdentityRole>>();var userManager=services.GetRequiredService<UserManager<ApplicationUser>>();
        foreach(var role in AppRoles.All)if(!await roleManager.RoleExistsAsync(role)){var result=await roleManager.CreateAsync(new IdentityRole(role));Ensure(result,$"create role {role}");}
        var password=config["SeedUsers:Password"];
        if(string.IsNullOrWhiteSpace(password)){logger.LogWarning("SeedUsers:Password is not configured; development users were not created.");return;}

        var legacyUsers=new[]{("omar@helpdesklite.local","Omar Mohamed",AppRoles.Employee),("ahmed@helpdesklite.local","Ahmed Hassan",AppRoles.SupportAgent),("sara@helpdesklite.local","Sara Ali",AppRoles.SupportAgent),("mona@helpdesklite.local","Mona Adel",AppRoles.SupportAgent),("manager@helpdesklite.local","Manager User",AppRoles.Manager)};
        foreach(var (email,name,role) in legacyUsers)await EnsureUser(userManager,email,name,role,password,enforceExactRole:false);

        var demos=services.GetRequiredService<DemoAccountService>();
        var demoNames=new Dictionary<string,string>(StringComparer.Ordinal){{AppRoles.Employee,"Demo Employee"},{AppRoles.SupportAgent,"Demo Support Agent"},{AppRoles.Manager,"Demo Manager"}};
        foreach(var role in AppRoles.All){if(!demos.TryGetEmail(role,out var email))throw new InvalidOperationException($"Demo account mapping is missing for role {role}.");await EnsureUser(userManager,email,demoNames[role],role,password,enforceExactRole:true);}
        logger.LogInformation("Development Identity seed verified legacy users and {DemoCount} portfolio demo accounts.",AppRoles.All.Length);
    }

    private static async Task EnsureUser(UserManager<ApplicationUser> users,string email,string name,string role,string password,bool enforceExactRole)
    {
        var user=await users.FindByEmailAsync(email);
        if(user is null){user=new ApplicationUser{UserName=email,Email=email,EmailConfirmed=true,DisplayName=name};Ensure(await users.CreateAsync(user,password),$"create user {email}");}
        else
        {
            var changed=false;if(!user.EmailConfirmed){user.EmailConfirmed=true;changed=true;}if(string.IsNullOrWhiteSpace(user.DisplayName)){user.DisplayName=name;changed=true;}if(changed)Ensure(await users.UpdateAsync(user),$"update user {email}");
            if(!await users.CheckPasswordAsync(user,password)){var token=await users.GeneratePasswordResetTokenAsync(user);Ensure(await users.ResetPasswordAsync(user,token,password),$"reset password for {email}");}
        }
        var roles=await users.GetRolesAsync(user);
        if(enforceExactRole){var incorrect=roles.Where(existing=>!string.Equals(existing,role,StringComparison.Ordinal)).ToArray();if(incorrect.Length>0)Ensure(await users.RemoveFromRolesAsync(user,incorrect),$"remove incorrect demo roles for {email}");}
        if(!await users.IsInRoleAsync(user,role))Ensure(await users.AddToRoleAsync(user,role),$"assign {role} to {email}");
    }

    private static void Ensure(IdentityResult result,string operation){if(!result.Succeeded)throw new InvalidOperationException($"Unable to {operation}: {string.Join("; ",result.Errors.Select(error=>error.Description))}");}
}
