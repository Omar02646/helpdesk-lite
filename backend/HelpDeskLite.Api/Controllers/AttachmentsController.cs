using System.Security.Claims;
using HelpDeskLite.Api.Contracts;
using HelpDeskLite.Api.Domain;
using HelpDeskLite.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace HelpDeskLite.Api.Controllers;
[ApiController,Route("api/tickets/{ticketId:int}/attachments"),Authorize]
public sealed class AttachmentsController(AttachmentService service):ControllerBase {
    private string UserId=>User.FindFirstValue(ClaimTypes.NameIdentifier)!;private string Role=>User.FindFirstValue(ClaimTypes.Role)!;
    [HttpGet]public async Task<ActionResult<List<AttachmentDto>>> List(int ticketId,CancellationToken ct){try{return Ok(await service.ListAsync(ticketId,UserId,Role,ct));}catch(KeyNotFoundException){return NotFound();}catch(UnauthorizedAccessException){return Forbid();}}
    [HttpPost,Authorize(Roles=AppRoles.Employee),RequestSizeLimit(5*1024*1024+65536)]public async Task<ActionResult<AttachmentDto>> Upload(int ticketId,IFormFile file,CancellationToken ct){try{var result=await service.UploadAsync(ticketId,file,UserId,Role,ct);return CreatedAtAction(nameof(Open),new{ticketId,attachmentId=result.Id},result);}catch(KeyNotFoundException){return NotFound();}catch(UnauthorizedAccessException){return Forbid();}catch(ArgumentException ex){return BadRequest(new{message=ex.Message});}}
    [HttpGet("{attachmentId:int}")]public async Task<IActionResult> Open(int ticketId,int attachmentId,[FromQuery]bool download=false,CancellationToken ct=default){try{var item=await service.OpenAsync(ticketId,attachmentId,UserId,Role,ct);return PhysicalFile(item.Path,item.ContentType,download?item.DownloadName:null,enableRangeProcessing:true);}catch(KeyNotFoundException){return NotFound();}catch(FileNotFoundException){return NotFound();}catch(UnauthorizedAccessException){return Forbid();}}
}
