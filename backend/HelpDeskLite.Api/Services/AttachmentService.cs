using HelpDeskLite.Api.Contracts;
using HelpDeskLite.Api.Data;
using HelpDeskLite.Api.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace HelpDeskLite.Api.Services;

public sealed class AttachmentStorageOptions { public string RootPath { get; set; } = "App_Data/attachments"; public long MaxFileSizeBytes { get; set; } = 5 * 1024 * 1024; public int MaxFilesPerTicket { get; set; } = 3; }
public sealed record StoredAttachment(string Path, string ContentType, string DownloadName);

public sealed class AttachmentService(ApplicationDbContext db, IOptions<AttachmentStorageOptions> options, IWebHostEnvironment environment)
{
    private static readonly Dictionary<string, string> Allowed = new(StringComparer.OrdinalIgnoreCase) { [".png"] = "image/png", [".jpg"] = "image/jpeg", [".jpeg"] = "image/jpeg", [".webp"] = "image/webp" };
    private readonly AttachmentStorageOptions settings = options.Value;
    private string RootPath => Path.GetFullPath(Path.IsPathRooted(settings.RootPath) ? settings.RootPath : Path.Combine(environment.ContentRootPath, settings.RootPath));

    public async Task EnsureViewAsync(int ticketId, string userId, string role, CancellationToken ct)
    {
        var ticket = await db.Tickets.AsNoTracking().SingleOrDefaultAsync(x => x.Id == ticketId, ct) ?? throw new KeyNotFoundException();
        if (role == AppRoles.Employee && ticket.CreatedByUserId != userId) throw new UnauthorizedAccessException();
    }

    public async Task<List<AttachmentDto>> ListAsync(int ticketId, string userId, string role, CancellationToken ct)
    {
        await EnsureViewAsync(ticketId, userId, role, ct);
        return await db.TicketAttachments.AsNoTracking().Where(x => x.TicketId == ticketId).OrderBy(x => x.CreatedAt).Select(x => new AttachmentDto(x.Id, x.OriginalFileName, x.ContentType, x.SizeBytes, x.UploadedByUser!.DisplayName, x.CreatedAt)).ToListAsync(ct);
    }

    public async Task<AttachmentDto> UploadAsync(int ticketId, IFormFile file, string userId, string role, CancellationToken ct)
    {
        await EnsureViewAsync(ticketId, userId, role, ct);
        if (role != AppRoles.Employee) throw new UnauthorizedAccessException();
        if (await db.TicketAttachments.CountAsync(x => x.TicketId == ticketId, ct) >= settings.MaxFilesPerTicket) throw new ArgumentException($"A ticket can have up to {settings.MaxFilesPerTicket} attachments.");
        if (file.Length <= 0) throw new ArgumentException("The selected image is empty.");
        if (file.Length > settings.MaxFileSizeBytes) throw new ArgumentException($"Images must be {settings.MaxFileSizeBytes / (1024 * 1024)} MB or smaller.");
        var original = Path.GetFileName(file.FileName);
        if (string.IsNullOrWhiteSpace(original) || original.Length > 255) throw new ArgumentException("The image filename is invalid.");
        var extension = Path.GetExtension(original);
        if (!Allowed.TryGetValue(extension, out var expectedType) || !string.Equals(file.ContentType, expectedType, StringComparison.OrdinalIgnoreCase)) throw new ArgumentException("Only PNG, JPG, JPEG, and WebP images are supported.");
        if (!await HasValidSignatureAsync(file, extension, ct)) throw new ArgumentException("The file content does not match a supported image format.");

        Directory.CreateDirectory(RootPath);
        var stored = $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
        var physical = Path.Combine(RootPath, stored);
        try
        {
            await using (var stream = new FileStream(physical, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, true)) await file.CopyToAsync(stream, ct);
            var entity = new TicketAttachment { TicketId = ticketId, OriginalFileName = original, StoredFileName = stored, ContentType = expectedType, SizeBytes = file.Length, UploadedByUserId = userId, CreatedAt = DateTimeOffset.UtcNow };
            db.TicketAttachments.Add(entity);
            await db.SaveChangesAsync(ct);
            var uploader = await db.Users.Where(x => x.Id == userId).Select(x => x.DisplayName).SingleAsync(ct);
            return new(entity.Id, entity.OriginalFileName, entity.ContentType, entity.SizeBytes, uploader, entity.CreatedAt);
        }
        catch { if (File.Exists(physical)) File.Delete(physical); throw; }
    }

    public async Task<StoredAttachment> OpenAsync(int ticketId, int attachmentId, string userId, string role, CancellationToken ct)
    {
        await EnsureViewAsync(ticketId, userId, role, ct);
        var item = await db.TicketAttachments.AsNoTracking().SingleOrDefaultAsync(x => x.Id == attachmentId && x.TicketId == ticketId, ct) ?? throw new KeyNotFoundException();
        var path = Path.GetFullPath(Path.Combine(RootPath, item.StoredFileName));
        if (!path.StartsWith(RootPath + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) || !File.Exists(path)) throw new FileNotFoundException();
        return new(path, item.ContentType, item.OriginalFileName);
    }

    private static async Task<bool> HasValidSignatureAsync(IFormFile file, string extension, CancellationToken ct)
    {
        var header = new byte[12];
        await using var stream = file.OpenReadStream();
        var read = await stream.ReadAsync(header, ct);
        return extension.ToLowerInvariant() switch
        {
            ".png" => read >= 8 && header[..8].SequenceEqual(new byte[] { 0x89, 0x50, 0x4e, 0x47, 0x0d, 0x0a, 0x1a, 0x0a }),
            ".jpg" or ".jpeg" => read >= 3 && header[0] == 0xff && header[1] == 0xd8 && header[2] == 0xff,
            ".webp" => read >= 12 && header[..4].SequenceEqual("RIFF"u8.ToArray()) && header[8..12].SequenceEqual("WEBP"u8.ToArray()),
            _ => false
        };
    }
}
