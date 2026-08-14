namespace HelpDeskLite.Api.Domain;
public enum TicketStatus { Open, InProgress, InReview, Resolved }
public enum TicketPriority { High, Medium, Low }
public sealed class Ticket {
    public int Id { get; set; }
    public required string TicketNumber { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public required string Category { get; set; }
    public TicketStatus Status { get; set; }
    public TicketPriority Priority { get; set; }
    public required string CreatedByUserId { get; set; }
    public ApplicationUser? CreatedByUser { get; set; }
    public string? AssignedToUserId { get; set; }
    public ApplicationUser? AssignedToUser { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public DateTimeOffset? ResolvedAt { get; set; }
    public List<TicketComment> Comments { get; set; } = [];
    public List<TicketStatusHistory> StatusHistory { get; set; } = [];
    public List<TicketAttachment> Attachments { get; set; } = [];
}
public sealed class TicketAttachment {
    public int Id { get; set; }
    public int TicketId { get; set; }
    public Ticket? Ticket { get; set; }
    public required string OriginalFileName { get; set; }
    public required string StoredFileName { get; set; }
    public required string ContentType { get; set; }
    public long SizeBytes { get; set; }
    public required string UploadedByUserId { get; set; }
    public ApplicationUser? UploadedByUser { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
public sealed class TicketComment {
    public int Id { get; set; }
    public int TicketId { get; set; }
    public Ticket? Ticket { get; set; }
    public required string AuthorUserId { get; set; }
    public ApplicationUser? AuthorUser { get; set; }
    public required string Body { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
public sealed class TicketStatusHistory {
    public int Id { get; set; }
    public int TicketId { get; set; }
    public Ticket? Ticket { get; set; }
    public TicketStatus? FromStatus { get; set; }
    public TicketStatus ToStatus { get; set; }
    public required string ChangedByUserId { get; set; }
    public ApplicationUser? ChangedByUser { get; set; }
    public DateTimeOffset ChangedAt { get; set; }
}
public static class AppRoles { public const string Employee="Employee"; public const string SupportAgent="SupportAgent"; public const string Manager="Manager"; public static readonly string[] All=[Employee,SupportAgent,Manager]; }
