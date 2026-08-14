using HelpDeskLite.Api.Contracts;
using HelpDeskLite.Api.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
namespace HelpDeskLite.Api.Controllers;
[ApiController,Route("api/auth")]
public sealed class AuthController(SignInManager<ApplicationUser> signInManager,UserManager<ApplicationUser> userManager):ControllerBase {
    [HttpPost("login"),AllowAnonymous] public async Task<ActionResult<UserDto>> Login(LoginRequest request){var user=await userManager.FindByEmailAsync(request.Email);if(user is null)return Unauthorized(new{message="Invalid email or password."});var result=await signInManager.PasswordSignInAsync(user,request.Password,request.RememberMe,lockoutOnFailure:true);if(!result.Succeeded)return Unauthorized(new{message="Invalid email or password."});return Ok(await Map(user));}
    [HttpPost("logout"),Authorize] public async Task<IActionResult> Logout(){await signInManager.SignOutAsync();return NoContent();}
    [HttpGet("me"),Authorize] public async Task<ActionResult<UserDto>> Me(){var user=await userManager.GetUserAsync(User);return user is null?Unauthorized():Ok(await Map(user));}
    private async Task<UserDto> Map(ApplicationUser user){var role=(await userManager.GetRolesAsync(user)).Single();var initials=string.Concat(user.DisplayName.Split(' ',StringSplitOptions.RemoveEmptyEntries).Select(x=>x[0]));return new(user.Id,user.DisplayName,user.Email!,role,initials);}
}
