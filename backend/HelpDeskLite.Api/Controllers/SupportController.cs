using HelpDeskLite.Api.Contracts;using HelpDeskLite.Api.Domain;using HelpDeskLite.Api.Services;using Microsoft.AspNetCore.Authorization;using Microsoft.AspNetCore.Mvc;
namespace HelpDeskLite.Api.Controllers;
[ApiController,Route("api/support"),Authorize(Roles=AppRoles.SupportAgent)]public sealed class SupportController(TicketService service):ControllerBase{[HttpGet("queue")]public Task<List<TicketDto>> Queue([FromQuery]TicketQuery query,CancellationToken ct)=>service.QueryAsync(query,null,true,ct);}
