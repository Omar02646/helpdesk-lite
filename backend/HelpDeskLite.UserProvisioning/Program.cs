using HelpDeskLite.Api.Data;
using HelpDeskLite.Api.Domain;
using HelpDeskLite.UserProvisioning;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var parsed = Arguments.Parse(args);
if (!parsed.Succeeded) { Console.Error.WriteLine(parsed.Error); return 2; }

var builder = Host.CreateApplicationBuilder(args);
builder.Logging.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Warning);
var localConfig = new[]
{
    Path.Combine(Directory.GetCurrentDirectory(), "backend", "HelpDeskLite.Api", "appsettings.json"),
    Path.Combine(Directory.GetCurrentDirectory(), "..", "HelpDeskLite.Api", "appsettings.json")
}.FirstOrDefault(File.Exists);
if (localConfig is not null) builder.Configuration.AddJsonFile(localConfig, optional: true, reloadOnChange: false);
builder.Configuration.AddEnvironmentVariables();
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString)) { Console.Error.WriteLine("ConnectionStrings__DefaultConnection is not configured."); return 2; }

builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));
builder.Services.AddDataProtection();
builder.Services.AddIdentityCore<ApplicationUser>(IdentityConfiguration.Configure).AddRoles<IdentityRole>().AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();
builder.Services.AddScoped<UserProvisioner>();
using var host = builder.Build();
using var scope = host.Services.CreateScope();

var name = parsed.Name ?? Prompt("Display Name: ");
var email = parsed.Email ?? Prompt("Email: ");
var role = ResolveRole(parsed.Role ?? Prompt("Role (1 = Employee, 2 = SupportAgent, 3 = Manager): "));
if (role is null) { Console.Error.WriteLine("Role must be 1, 2, 3, Employee, SupportAgent, or Manager."); return 2; }

var password = Environment.GetEnvironmentVariable("HELPDESKLITE_PROVISIONING_PASSWORD");
if (string.IsNullOrEmpty(password))
{
    password = ReadPassword("Password: ");
    var confirmation = ReadPassword("Confirm Password: ");
    if (!string.Equals(password, confirmation, StringComparison.Ordinal)) { Console.Error.WriteLine("Password confirmation does not match."); return 2; }
}

try
{
    var result = await scope.ServiceProvider.GetRequiredService<UserProvisioner>().CreateAsync(name, email, role, password);
    if (!result.Succeeded) { Console.Error.WriteLine(result.Message); return 1; }
    Console.WriteLine();
    Console.WriteLine(result.Message);
    Console.WriteLine($"Name: {result.User!.DisplayName}");
    Console.WriteLine($"Email: {result.User.Email}");
    Console.WriteLine($"Role: {result.Role}");
    return 0;
}
catch (Exception)
{
    Console.Error.WriteLine("Provisioning failed. Verify the database connection, migrations, and operator access, then try again.");
    return 1;
}

static string Prompt(string label) { Console.Write(label); return Console.ReadLine() ?? string.Empty; }
static string? ResolveRole(string value) => value.Trim().ToLowerInvariant() switch { "1" or "employee" => AppRoles.Employee, "2" or "supportagent" => AppRoles.SupportAgent, "3" or "manager" => AppRoles.Manager, _ => null };
static string ReadPassword(string label)
{
    Console.Write(label);
    if (Console.IsInputRedirected) return Console.ReadLine() ?? string.Empty;
    var characters = new List<char>();
    while (true)
    {
        var key = Console.ReadKey(intercept: true);
        if (key.Key == ConsoleKey.Enter) { Console.WriteLine(); return new string(characters.ToArray()); }
        if (key.Key == ConsoleKey.Backspace && characters.Count > 0) { characters.RemoveAt(characters.Count - 1); continue; }
        if (!char.IsControl(key.KeyChar)) characters.Add(key.KeyChar);
    }
}

file sealed record Arguments(bool Succeeded, string? Name, string? Email, string? Role, string? Error)
{
    public static Arguments Parse(string[] args)
    {
        string? name = null, email = null, role = null;
        for (var index = 0; index < args.Length; index++)
        {
            var option = args[index];
            if (option is not ("--name" or "--email" or "--role") || index + 1 >= args.Length) return new(false, null, null, null, $"Unknown or incomplete option: {option}");
            var value = args[++index];
            if (option == "--name") name = value; else if (option == "--email") email = value; else role = value;
        }
        return new(true, name, email, role, null);
    }
}
