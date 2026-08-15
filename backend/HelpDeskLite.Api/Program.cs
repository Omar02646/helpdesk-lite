using System.Text.Json.Serialization;
using HelpDeskLite.Api.Data;
using HelpDeskLite.Api.Domain;
using HelpDeskLite.Api.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(IdentityConfiguration.Configure).AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = "HelpDeskLite.Auth";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment() ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always;
    options.SlidingExpiration = true;
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.Events.OnRedirectToLogin = context => { context.Response.StatusCode = StatusCodes.Status401Unauthorized; return Task.CompletedTask; };
    options.Events.OnRedirectToAccessDenied = context => { context.Response.StatusCode = StatusCodes.Status403Forbidden; return Task.CompletedTask; };
});
builder.Services.AddAuthorization();
builder.Services.AddScoped<TicketService>();
builder.Services.AddScoped<AttachmentService>();
builder.Services.Configure<AttachmentStorageOptions>(builder.Configuration.GetSection("AttachmentStorage"));
builder.Services.AddControllers().AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

var app = builder.Build();
if (!app.Environment.IsDevelopment()) { app.UseHsts(); app.UseHttpsRedirection(); }
app.UseExceptionHandler();
if (!app.Environment.IsDevelopment())
{
    app.UseDefaultFiles();
    app.UseStaticFiles();
}
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "Healthy" })).AllowAnonymous();
if (app.Environment.IsDevelopment())
{
    var storage = app.Services.GetRequiredService<Microsoft.Extensions.Options.IOptions<AttachmentStorageOptions>>().Value;
    var root = Path.GetFullPath(Path.IsPathRooted(storage.RootPath) ? storage.RootPath : Path.Combine(app.Environment.ContentRootPath, storage.RootPath));
    Directory.CreateDirectory(root);
    await DevelopmentSeeder.SeedAsync(app.Services, app.Configuration, app.Logger);
}
if (!app.Environment.IsDevelopment() && !app.Environment.IsEnvironment("Testing"))
{
    app.Map("/api/{**path}", () => Results.Problem(statusCode: StatusCodes.Status404NotFound, title: "API endpoint not found."));
    app.MapFallbackToFile("index.html");
}
app.Run();
public partial class Program { }
