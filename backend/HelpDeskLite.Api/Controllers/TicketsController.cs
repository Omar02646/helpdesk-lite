using System.Security.Claims;
using HelpDeskLite.Api.Contracts;
using HelpDeskLite.Api.Domain;
using HelpDeskLite.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace HelpDeskLite.Api.Controllers;
[ApiController,Route("api/tickets"),Authorize]
public sealed class TicketsController(TicketService service):ControllerBase {
    private string UserId=>User.FindFirstValue(ClaimTypes.NameIdentifier)!;private string Role=>User.FindFirstValue(ClaimTypes.Role)!;
    [HttpGet("my"),Authorize(Roles=AppRoles.Employee)]public Task<TicketPageDto> My([FromQuery]TicketQuery query,CancellationToken ct)=>service.QueryPageAsync(query,UserId,false,ct);
    [HttpPost,Authorize(Roles=AppRoles.Employee)]public async Task<ActionResult<TicketDto>> Create(CreateTicketRequest request,CancellationToken ct){try{var ticket=await service.CreateAsync(request,UserId,ct);return CreatedAtAction(nameof(Get),new{id=ticket.Id},ticket);}catch(ArgumentException ex){return BadRequest(new{message=ex.Message});}}
    [HttpGet,Authorize(Roles=AppRoles.SupportAgent+","+AppRoles.Manager)]public Task<TicketPageDto> All([FromQuery]TicketQuery query,CancellationToken ct)=>service.QueryPageAsync(query,null,false,ct);
    [HttpGet("{id:int}")]public async Task<ActionResult<TicketDto>> Get(int id,CancellationToken ct){try{var ticket=await service.GetAsync(id,UserId,Role,ct);return ticket is null?NotFound():Ok(ticket);}catch(UnauthorizedAccessException){return Forbid();}}
    [HttpPatch("{id:int}/assignee"),Authorize(Roles=AppRoles.SupportAgent)]public async Task<IActionResult> Assignee(int id,ChangeAssigneeRequest request,CancellationToken ct){try{await service.AssignAsync(id,request.UserId,UserId,ct);return NoContent();}catch(KeyNotFoundException){return NotFound();}catch(ArgumentException ex){return BadRequest(new{message=ex.Message});}}
    [HttpPatch("{id:int}/status"),Authorize(Roles=AppRoles.SupportAgent)]public async Task<IActionResult> Status(int id,ChangeStatusRequest request,CancellationToken ct){if(!Enum.IsDefined(request.Status))return BadRequest(new ProblemDetails{Title="Invalid ticket status",Status=StatusCodes.Status400BadRequest});try{await service.UpdateStatusAsync(id,request.Status,UserId,ct);return NoContent();}catch(KeyNotFoundException){return NotFound();}}
    [HttpGet("{id:int}/comments")]public async Task<ActionResult<IReadOnlyList<CommentDto>>> Comments(int id,CancellationToken ct){var ticket=await Get(id,ct);return ticket.Result is not null?ticket.Result:Ok(ticket.Value!.Comments);}
    [HttpPost("{id:int}/comments"),Authorize(Roles=AppRoles.SupportAgent)]public async Task<IActionResult> Comment(int id,CreateCommentRequest request,CancellationToken ct){try{await service.AddCommentAsync(id,request.Body,UserId,ct);return NoContent();}catch(KeyNotFoundException){return NotFound();}catch(ArgumentException ex){return BadRequest(new{message=ex.Message});}}
}
