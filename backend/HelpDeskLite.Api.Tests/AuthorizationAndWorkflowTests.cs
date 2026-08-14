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
    [Fact]public async Task My_tickets_returns_only_current_employee_tickets(){var tickets=await factory.ClientFor(ApiFactory.EmployeeId,AppRoles.Employee).GetFromJsonAsync<List<TicketDto>>("/api/tickets/my",JsonOptions);Assert.NotNull(tickets);Assert.Single(tickets);Assert.Equal("HDL-1001",tickets[0].TicketNumber);}
    [Fact]public async Task Support_queue_excludes_resolved_by_default(){var tickets=await factory.ClientFor(ApiFactory.AgentId,AppRoles.SupportAgent).GetFromJsonAsync<List<TicketDto>>("/api/support/queue",JsonOptions);Assert.NotNull(tickets);Assert.DoesNotContain(tickets,item=>item.Status==TicketStatus.Resolved);}
}
