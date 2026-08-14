using System.Text.Json.Serialization;
using HelpDeskLite.Api.Data;
using HelpDeskLite.Api.Domain;
using HelpDeskLite.Api.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder=WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<ApplicationDbContext>(options=>options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddIdentity<ApplicationUser,IdentityRole>(options=>{options.User.RequireUniqueEmail=true;options.SignIn.RequireConfirmedEmail=true;options.Password.RequiredLength=10;options.Lockout.MaxFailedAccessAttempts=5;}).AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();
builder.Services.ConfigureApplicationCookie(options=>{options.Cookie.Name="HelpDeskLite.Auth";options.Cookie.HttpOnly=true;options.Cookie.SameSite=SameSiteMode.Strict;options.Cookie.SecurePolicy=CookieSecurePolicy.SameAsRequest;options.SlidingExpiration=true;options.ExpireTimeSpan=TimeSpan.FromHours(8);options.Events.OnRedirectToLogin=context=>{context.Response.StatusCode=StatusCodes.Status401Unauthorized;return Task.CompletedTask;};options.Events.OnRedirectToAccessDenied=context=>{context.Response.StatusCode=StatusCodes.Status403Forbidden;return Task.CompletedTask;};});
builder.Services.AddAuthorization();builder.Services.AddScoped<TicketService>();builder.Services.AddScoped<AttachmentService>();builder.Services.Configure<AttachmentStorageOptions>(builder.Configuration.GetSection("AttachmentStorage"));
builder.Services.AddControllers().AddJsonOptions(options=>options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));builder.Services.AddProblemDetails();
var app=builder.Build();app.UseExceptionHandler();app.UseAuthentication();app.UseAuthorization();app.MapControllers();
if(app.Environment.IsDevelopment()){var storage=app.Services.GetRequiredService<Microsoft.Extensions.Options.IOptions<AttachmentStorageOptions>>().Value;var root=Path.GetFullPath(Path.IsPathRooted(storage.RootPath)?storage.RootPath:Path.Combine(app.Environment.ContentRootPath,storage.RootPath));Directory.CreateDirectory(root);await DevelopmentSeeder.SeedAsync(app.Services,app.Configuration,app.Logger);}
app.Run();
public partial class Program { }
