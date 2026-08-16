using System.Net.Mail;
using System.Text;
using HelpDeskLite.Api.Contracts;
using HelpDeskLite.Api.Domain;
using HelpDeskLite.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;

namespace HelpDeskLite.Api.Controllers;

[ApiController,Route("api/auth")]
public sealed class AuthController(SignInManager<ApplicationUser> signInManager,UserManager<ApplicationUser> userManager,RoleManager<IdentityRole> roleManager,IEmailService emailService,IOptions<EmailOptions> emailOptions,DemoAccountService demoAccounts,ILogger<AuthController> logger):ControllerBase
{
    private const string GenericForgot="If an account exists for this email, a password reset link has been sent.";
    private const string GenericResend="If an account exists and requires confirmation, a confirmation email has been sent.";

    [HttpPost("login"),AllowAnonymous,EnableRateLimiting("auth")]
    public async Task<ActionResult<UserDto>> Login(LoginRequest request)
    {
        var user=await userManager.FindByEmailAsync(request.Email.Trim());
        if(user is null)return Unauthorized(new{message="Invalid email or password."});
        var result=await signInManager.PasswordSignInAsync(user,request.Password,request.RememberMe,lockoutOnFailure:true);
        if(result.IsNotAllowed&&!await userManager.IsEmailConfirmedAsync(user))return Unauthorized(new{message="Please confirm your email before signing in."});
        if(!result.Succeeded)return Unauthorized(new{message="Invalid email or password."});
        return Ok(await Map(user));
    }

    [HttpPost("register"),AllowAnonymous,EnableRateLimiting("auth")]
    public async Task<ActionResult<MessageResponse>> Register(RegisterRequest request)
    {
        var first=ValidateName(request.FirstName,"First name");if(first.Error is not null)return BadRequest(new{message=first.Error});
        var last=ValidateName(request.LastName,"Last name");if(last.Error is not null)return BadRequest(new{message=last.Error});
        if(request.Password!=request.ConfirmPassword)return BadRequest(new{message="Passwords do not match."});
        var email=request.Email.Trim();if(!MailAddress.TryCreate(email,out var address)||!string.Equals(address.Address,email,StringComparison.OrdinalIgnoreCase))return BadRequest(new{message="Please enter a valid email."});
        if(await userManager.FindByEmailAsync(email) is not null)return Conflict(new{message="An account with this email already exists."});
        if(!await roleManager.RoleExistsAsync(AppRoles.Employee))return Problem(statusCode:500,title:"Account registration is temporarily unavailable.");
        var user=new ApplicationUser{FirstName=first.Value,LastName=last.Value,DisplayName=$"{first.Value} {last.Value}",Email=email,UserName=email,EmailConfirmed=false};
        var created=await userManager.CreateAsync(user,request.Password);
        if(!created.Succeeded)return BadRequest(new{message=DescribePassword(created)});
        var assigned=await userManager.AddToRoleAsync(user,AppRoles.Employee);
        if(!assigned.Succeeded){await userManager.DeleteAsync(user);return Problem(statusCode:500,title:"Account registration is temporarily unavailable.");}
        try{await SendConfirmation(user);}catch{await userManager.DeleteAsync(user);return Problem(statusCode:503,title:"Account registration is temporarily unavailable.");}
        return StatusCode(StatusCodes.Status201Created,new MessageResponse("Account created. Please check your email to confirm your account."));
    }

    [HttpPost("confirm-email"),AllowAnonymous]
    public async Task<ActionResult<MessageResponse>> ConfirmEmail(ConfirmEmailRequest request)
    {
        var user=await userManager.FindByIdAsync(request.UserId);if(user is null)return BadRequest(new{message="This confirmation link is invalid or expired."});
        if(!TryDecode(request.Token,out var token))return BadRequest(new{message="This confirmation link is invalid or expired."});
        var result=await userManager.ConfirmEmailAsync(user,token);return result.Succeeded?Ok(new MessageResponse("Email confirmed successfully.")):BadRequest(new{message="This confirmation link is invalid or expired."});
    }

    [HttpPost("resend-confirmation"),AllowAnonymous,EnableRateLimiting("email")]
    public async Task<ActionResult<MessageResponse>> Resend(EmailRequest request)
    {
        var user=await userManager.FindByEmailAsync(request.Email.Trim());
        if(user is not null&&!await userManager.IsEmailConfirmedAsync(user)){try{await SendConfirmation(user);}catch{}}
        return Ok(new MessageResponse(GenericResend));
    }

    [HttpPost("forgot-password"),AllowAnonymous,EnableRateLimiting("email")]
    public async Task<ActionResult<MessageResponse>> Forgot(EmailRequest request)
    {
        var user=await userManager.FindByEmailAsync(request.Email.Trim());
        if(user is not null&&await userManager.IsEmailConfirmedAsync(user))
        {
            var token=Encode(await userManager.GeneratePasswordResetTokenAsync(user));var url=FrontendUrl("reset-password",user.Id,token);var template=EmailTemplates.PasswordReset(user.FirstName??user.PreferredName.Split(' ')[0],url);
            try{await emailService.SendAsync(user.Email!,"Reset your HelpDesk Lite password",template.Html,template.Text,HttpContext.RequestAborted);}catch{}
        }
        return Ok(new MessageResponse(GenericForgot));
    }

    [HttpPost("reset-password"),AllowAnonymous]
    public async Task<ActionResult<MessageResponse>> Reset(ResetPasswordRequest request)
    {
        if(request.NewPassword!=request.ConfirmPassword)return BadRequest(new{message="Passwords do not match."});
        var user=await userManager.FindByIdAsync(request.UserId);if(user is null||!TryDecode(request.Token,out var token))return BadRequest(new{message="This reset link is invalid or expired."});
        var result=await userManager.ResetPasswordAsync(user,token,request.NewPassword);
        return result.Succeeded?Ok(new MessageResponse("Password reset successfully.")):BadRequest(new{message=result.Errors.Any(e=>e.Code.Contains("Password",StringComparison.OrdinalIgnoreCase))?DescribePassword(result):"This reset link is invalid or expired."});
    }

    [HttpPost("demo-login"),AllowAnonymous,EnableRateLimiting("demo")]
    public async Task<ActionResult<UserDto>> DemoLogin(DemoLoginRequest request)
    {
        if(!demoAccounts.TryGetEmail(request.Role,out var email))return BadRequest(new{message="Invalid demo role."});
        var user=await userManager.FindByEmailAsync(email);
        if(user is null){logger.LogWarning("Demo login unavailable: configured account {DemoEmail} for role {DemoRole} was not found.",email,request.Role);return StatusCode(503,new{message="This demo experience is temporarily unavailable."});}
        if(!await userManager.IsInRoleAsync(user,request.Role)){logger.LogWarning("Demo login unavailable: configured account {DemoEmail} does not have expected role {DemoRole}.",email,request.Role);return StatusCode(503,new{message="This demo experience is temporarily unavailable."});}
        await signInManager.SignOutAsync();await signInManager.SignInAsync(user,isPersistent:false);return Ok(await Map(user));
    }

    [HttpPost("logout"),Authorize]public async Task<IActionResult> Logout(){await signInManager.SignOutAsync();return NoContent();}
    [HttpGet("me"),Authorize]public async Task<ActionResult<UserDto>> Me(){var user=await userManager.GetUserAsync(User);return user is null?Unauthorized():Ok(await Map(user));}

    private async Task SendConfirmation(ApplicationUser user){var token=Encode(await userManager.GenerateEmailConfirmationTokenAsync(user));var url=FrontendUrl("confirm-email",user.Id,token);var template=EmailTemplates.Confirmation(user.FirstName??user.PreferredName.Split(' ')[0],url);await emailService.SendAsync(user.Email!,"Confirm your HelpDesk Lite account",template.Html,template.Text,HttpContext.RequestAborted);}
    private string FrontendUrl(string path,string userId,string token)=>$"{emailOptions.Value.FrontendBaseUrl.TrimEnd('/')}/{path}?userId={Uri.EscapeDataString(userId)}&token={Uri.EscapeDataString(token)}";
    private async Task<UserDto> Map(ApplicationUser user){var role=(await userManager.GetRolesAsync(user)).Single();var name=user.PreferredName;var initials=string.Concat(name.Split(' ',StringSplitOptions.RemoveEmptyEntries).Take(2).Select(x=>x[0]));return new(user.Id,name,user.Email!,role,initials,user.FirstName,user.LastName,demoAccounts.IsDemo(user.Email));}
    private static string Encode(string token)=>WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
    private static bool TryDecode(string encoded,out string token){try{token=Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(encoded));return true;}catch(FormatException){token="";return false;}}
    private static (string Value,string? Error) ValidateName(string value,string label){value=value.Trim();if(value.Length==0)return("",$"{label} is required.");if(value.Length>50)return("",$"{label} must be 50 characters or fewer.");if(value.Any(char.IsControl))return("",$"{label} contains invalid characters.");return(value,null);}
    private static string DescribePassword(IdentityResult result)=>string.Join(" ",result.Errors.Select(x=>x.Description));
}
