using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using HelpDeskLite.Api.Contracts;
using HelpDeskLite.Api.Domain;
using Xunit;
namespace HelpDeskLite.Api.Tests;
public sealed class AuthorizationAndWorkflowTests(ApiFactory factory):IClassFixture<ApiFactory>{
    private static readonly JsonSerializerOptions JsonOptions=new(JsonSerializerDefaults.Web){Converters={new JsonStringEnumConverter()}};
    [Fact]public async Task Employee_cannot_read_another_employees_ticket(){var response=await factory.ClientFor(ApiFactory.EmployeeId,AppRoles.Employee).GetAsync("/api/tickets/2");Assert.Equal(HttpStatusCode.Forbidden,response.StatusCode);}
    [Fact]public async Task Employee_cannot_modify_status(){var response=await factory.ClientFor(ApiFactory.EmployeeId,AppRoles.Employee).PatchAsJsonAsync("/api/tickets/1/status",new ChangeStatusRequest(TicketStatus.Resolved));Assert.Equal(HttpStatusCode.Forbidden,response.StatusCode);}
    [Fact]public async Task Manager_cannot_modify_status(){var response=await factory.ClientFor(ApiFactory.ManagerId,AppRoles.Manager).PatchAsJsonAsync("/api/tickets/1/status",new ChangeStatusRequest(TicketStatus.Resolved));Assert.Equal(HttpStatusCode.Forbidden,response.StatusCode);}
    [Fact]public async Task Support_agent_can_assign_ticket(){var response=await factory.ClientFor(ApiFactory.AgentId,AppRoles.SupportAgent).PatchAsJsonAsync("/api/tickets/1/assignee",new ChangeAssigneeRequest(ApiFactory.AgentId));Assert.Equal(HttpStatusCode.NoContent,response.StatusCode);}
    [Fact]public async Task Support_agent_can_update_status(){var response=await factory.ClientFor(ApiFactory.AgentId,AppRoles.SupportAgent).PatchAsJsonAsync("/api/tickets/1/status",new ChangeStatusRequest(TicketStatus.InProgress));Assert.Equal(HttpStatusCode.NoContent,response.StatusCode);}
    [Fact]public async Task My_tickets_returns_only_current_employee_tickets(){var page=await factory.ClientFor(ApiFactory.EmployeeId,AppRoles.Employee).GetFromJsonAsync<TicketPageDto>("/api/tickets/my",JsonOptions);Assert.NotNull(page);Assert.Single(page.Items);Assert.Equal("HDL-1001",page.Items[0].TicketNumber);}
    [Fact]public async Task Support_queue_excludes_resolved_by_default(){var page=await factory.ClientFor(ApiFactory.AgentId,AppRoles.SupportAgent).GetFromJsonAsync<TicketPageDto>("/api/support/queue",JsonOptions);Assert.NotNull(page);Assert.DoesNotContain(page.Items,item=>item.Status==TicketStatus.Resolved);}
    [Fact]public async Task Unauthenticated_support_queue_returns_401(){var response=await factory.CreateClient().GetAsync("/api/support/queue");Assert.Equal(HttpStatusCode.Unauthorized,response.StatusCode);}
    [Fact]public async Task Invalid_status_returns_400(){using var content=new StringContent("{\"status\":999}",System.Text.Encoding.UTF8,"application/json");var response=await factory.ClientFor(ApiFactory.AgentId,AppRoles.SupportAgent).PatchAsync("/api/tickets/1/status",content);Assert.Equal(HttpStatusCode.BadRequest,response.StatusCode);}
    [Fact]public async Task Pagination_is_bounded_and_preserves_employee_scope(){var page=await factory.ClientFor(ApiFactory.EmployeeId,AppRoles.Employee).GetFromJsonAsync<TicketPageDto>("/api/tickets/my?page=1&pageSize=1",JsonOptions);Assert.NotNull(page);Assert.Single(page.Items);Assert.All(page.Items,item=>Assert.Equal("Omar Mohamed",item.CreatedBy));Assert.Equal(1,page.PageSize);}
}
