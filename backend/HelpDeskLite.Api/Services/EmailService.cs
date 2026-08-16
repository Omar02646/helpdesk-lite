using System.Net;
using System.Net.Mail;
using System.Text.Encodings.Web;
using Microsoft.Extensions.Options;

namespace HelpDeskLite.Api.Services;

public sealed class EmailOptions
{
    public string SmtpHost { get; set; } = "smtp.gmail.com";
    public int SmtpPort { get; set; } = 587;
    public string Username { get; set; } = "omarkenawy02@gmail.com";
    public string? Password { get; set; }
    public string FromEmail { get; set; } = "omarkenawy02@gmail.com";
    public string FromName { get; set; } = "HelpDesk Lite";
    public string FrontendBaseUrl { get; set; } = "http://localhost:5173";
}

public interface IEmailService
{
    Task SendAsync(string recipient, string subject, string html, string text, CancellationToken cancellationToken = default);
}

public sealed class SmtpEmailService(IOptions<EmailOptions> options, ILogger<SmtpEmailService> logger) : IEmailService
{
    public async Task SendAsync(string recipient, string subject, string html, string text, CancellationToken cancellationToken = default)
    {
        var settings = options.Value;
        if (string.IsNullOrWhiteSpace(settings.Password))
        {
            logger.LogError("Email delivery is unavailable because SMTP credentials are not configured.");
            throw new InvalidOperationException("Email delivery is unavailable.");
        }
        using var message = new MailMessage { From = new MailAddress(settings.FromEmail, settings.FromName), Subject = subject, Body = html, IsBodyHtml = true };
        message.To.Add(recipient);
        message.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(text, null, "text/plain"));
        using var client = new SmtpClient(settings.SmtpHost, settings.SmtpPort) { EnableSsl = true, Credentials = new NetworkCredential(settings.Username, settings.Password) };
        try { await client.SendMailAsync(message, cancellationToken); }
        catch (Exception exception) { logger.LogError(exception, "Email delivery failed for subject {Subject}.", subject); throw new InvalidOperationException("Email delivery is unavailable."); }
    }
}

public static class EmailTemplates
{
    public static (string Html, string Text) Confirmation(string firstName, string url) => Build(firstName, "Confirm your email", "Thanks for creating your HelpDesk Lite account.<br><br>Please confirm your email address to activate your account and start using HelpDesk Lite.", "Confirm Email", url, "If you didn't create this account, you can safely ignore this email.");
    public static (string Html, string Text) PasswordReset(string firstName, string url) => Build(firstName, "Reset your password", "We received a request to reset the password for your HelpDesk Lite account.<br><br>Click the button below to choose a new password.", "Reset Password", url, "If you didn't request a password reset, you can safely ignore this email.<br>Your password will remain unchanged.");
    private static (string Html, string Text) Build(string firstName, string heading, string body, string action, string url, string closing)
    {
        var e = HtmlEncoder.Default; var safeName=e.Encode(firstName);var safeUrl=e.Encode(url);
        var html=$"""<!doctype html><html><body style="margin:0;background:#eef5ff;font-family:Arial,sans-serif;color:#10233f"><table role="presentation" width="100%" cellpadding="0" cellspacing="0"><tr><td style="padding:32px 12px"><table role="presentation" width="100%" cellpadding="0" cellspacing="0" style="max-width:600px;margin:auto;background:#fff;border-radius:14px"><tr><td style="padding:36px"><div style="font-size:24px;font-weight:700;color:#1264e3">HelpDesk Lite</div><h1 style="font-size:25px;margin:28px 0 18px">{e.Encode(heading)}</h1><p style="line-height:1.6">Hi {safeName},</p><p style="line-height:1.6">{body}</p><p style="margin:28px 0"><a href="{safeUrl}" style="display:inline-block;background:#1264e3;color:#fff;text-decoration:none;padding:14px 22px;border-radius:8px;font-weight:700">{e.Encode(action)}</a></p><p style="font-size:13px;line-height:1.6;color:#596a80">If the button doesn't work, copy and paste the link below into your browser:<br><a href="{safeUrl}" style="color:#1264e3;word-break:break-all">{safeUrl}</a></p><p style="line-height:1.6">{closing}</p><hr style="border:0;border-top:1px solid #e4eaf3;margin:28px 0"><p style="font-size:12px;color:#718096">HelpDesk Lite<br>Internal Support Ticketing Workspace</p></td></tr></table></td></tr></table></body></html>""";
        var plainBody=body.Replace("<br>",Environment.NewLine,StringComparison.OrdinalIgnoreCase);var plainClosing=closing.Replace("<br>",Environment.NewLine,StringComparison.OrdinalIgnoreCase);
        return (html,$"HelpDesk Lite\n\n{heading}\n\nHi {firstName},\n\n{plainBody}\n\n{action}: {url}\n\nIf the button doesn't work, copy and paste the link below into your browser:\n{url}\n\n{plainClosing}\n\nHelpDesk Lite\nInternal Support Ticketing Workspace");
    }
}
