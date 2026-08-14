using HelpDeskLite.Api.Domain;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
namespace HelpDeskLite.Api.Data;
public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options) {
    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<TicketComment> TicketComments => Set<TicketComment>();
    public DbSet<TicketStatusHistory> TicketStatusHistory => Set<TicketStatusHistory>();
    public DbSet<TicketAttachment> TicketAttachments => Set<TicketAttachment>();
    protected override void OnModelCreating(ModelBuilder builder) {
        base.OnModelCreating(builder);
        builder.Entity<ApplicationUser>().Property(x=>x.DisplayName).HasMaxLength(160);
        builder.Entity<Ticket>(entity => {
            entity.Property(x=>x.TicketNumber).HasMaxLength(30); entity.HasIndex(x=>x.TicketNumber).IsUnique();
            entity.Property(x=>x.Title).HasMaxLength(200); entity.Property(x=>x.Description).HasMaxLength(5000); entity.Property(x=>x.Category).HasMaxLength(80);
            entity.HasIndex(x=>x.Status); entity.HasIndex(x=>x.AssignedToUserId); entity.HasIndex(x=>x.CreatedByUserId); entity.HasIndex(x=>x.CreatedAt);
            entity.HasOne(x=>x.CreatedByUser).WithMany().HasForeignKey(x=>x.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x=>x.AssignedToUser).WithMany().HasForeignKey(x=>x.AssignedToUserId).OnDelete(DeleteBehavior.Restrict);
        });
        builder.Entity<TicketComment>(entity => { entity.Property(x=>x.Body).HasMaxLength(4000); entity.HasOne(x=>x.AuthorUser).WithMany().HasForeignKey(x=>x.AuthorUserId).OnDelete(DeleteBehavior.Restrict); });
        builder.Entity<TicketStatusHistory>().HasOne(x=>x.ChangedByUser).WithMany().HasForeignKey(x=>x.ChangedByUserId).OnDelete(DeleteBehavior.Restrict);
        builder.Entity<TicketAttachment>(entity => { entity.Property(x=>x.OriginalFileName).HasMaxLength(255); entity.Property(x=>x.StoredFileName).HasMaxLength(100); entity.Property(x=>x.ContentType).HasMaxLength(100); entity.HasIndex(x=>x.TicketId); entity.HasOne(x=>x.UploadedByUser).WithMany().HasForeignKey(x=>x.UploadedByUserId).OnDelete(DeleteBehavior.Restrict); });
    }
}
