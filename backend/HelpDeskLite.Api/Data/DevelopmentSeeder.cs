using HelpDeskLite.Api.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
namespace HelpDeskLite.Api.Data;
public static class DevelopmentSeeder {
    public static async Task SeedAsync(IServiceProvider services,IConfiguration config,ILogger logger) {
        using var scope=services.CreateScope();var db=scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await db.Database.MigrateAsync();
        var roleManager=scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();var userManager=scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        foreach(var role in AppRoles.All)if(!await roleManager.RoleExistsAsync(role))await roleManager.CreateAsync(new IdentityRole(role));
        var password=config["SeedUsers:Password"];if(string.IsNullOrWhiteSpace(password)){logger.LogWarning("SeedUsers:Password is not configured; development users were not created.");return;}
        var users=new[]{("omar@helpdesklite.local","Omar Mohamed",AppRoles.Employee),("ahmed@helpdesklite.local","Ahmed Hassan",AppRoles.SupportAgent),("sara@helpdesklite.local","Sara Ali",AppRoles.SupportAgent),("mona@helpdesklite.local","Mona Adel",AppRoles.SupportAgent),("manager@helpdesklite.local","Manager User",AppRoles.Manager)};
        foreach(var (email,name,role) in users){var user=await userManager.FindByEmailAsync(email);if(user is null){user=new ApplicationUser{UserName=email,Email=email,EmailConfirmed=true,DisplayName=name};var result=await userManager.CreateAsync(user,password);if(!result.Succeeded)throw new InvalidOperationException(string.Join("; ",result.Errors.Select(x=>x.Description)));}else if(!await userManager.CheckPasswordAsync(user,password)){var token=await userManager.GeneratePasswordResetTokenAsync(user);var reset=await userManager.ResetPasswordAsync(user,token,password);if(!reset.Succeeded)throw new InvalidOperationException(string.Join("; ",reset.Errors.Select(x=>x.Description)));}if(!await userManager.IsInRoleAsync(user,role))await userManager.AddToRoleAsync(user,role);}
        if(await db.Tickets.AnyAsync())return;
        var omar=(await userManager.FindByEmailAsync("omar@helpdesklite.local"))!;var ahmed=(await userManager.FindByEmailAsync("ahmed@helpdesklite.local"))!;var sara=(await userManager.FindByEmailAsync("sara@helpdesklite.local"))!;var now=DateTimeOffset.UtcNow;
        var tickets=new[]{
            new Ticket{TicketNumber="HDL-1001",Title="Laptop will not start",Description="My laptop powers on but the screen stays black. I have tried restarting and connecting an external monitor.",Category="IT Support",Status=TicketStatus.InProgress,Priority=TicketPriority.High,CreatedByUserId=omar.Id,AssignedToUserId=ahmed.Id,CreatedAt=now.AddHours(-8),UpdatedAt=now.AddHours(-6)},
            new Ticket{TicketNumber="HDL-1002",Title="VPN access problem",Description="I cannot connect to the company VPN from my laptop while working remotely.",Category="Network",Status=TicketStatus.Open,Priority=TicketPriority.Medium,CreatedByUserId=omar.Id,CreatedAt=now.AddHours(-5),UpdatedAt=now.AddHours(-5)},
            new Ticket{TicketNumber="HDL-1003",Title="Email not syncing",Description="Outlook has not received new messages since yesterday afternoon.",Category="Email",Status=TicketStatus.Resolved,Priority=TicketPriority.Low,CreatedByUserId=omar.Id,AssignedToUserId=sara.Id,CreatedAt=now.AddDays(-1),UpdatedAt=now.AddHours(-2),ResolvedAt=now.AddHours(-2)} };
        foreach(var ticket in tickets)ticket.StatusHistory.Add(new TicketStatusHistory{FromStatus=null,ToStatus=ticket.Status,ChangedByUserId=ticket.CreatedByUserId,ChangedAt=ticket.CreatedAt});
        db.Tickets.AddRange(tickets);await db.SaveChangesAsync();
    }
}
