using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace MyApp.Services;

public class EmailService(IConfiguration config)
{
    public async Task SendAsync(string subject, string plainTextBody)
    {
        var sender = config["Email:SenderAddress"]!;
        var password = config["Email:AppPassword"]!;
        var recipient = config["Email:RecipientAddress"]!;

        var message = new MimeMessage();
        message.From.Add(MailboxAddress.Parse(sender));
        message.To.Add(MailboxAddress.Parse(recipient));
        message.Subject = subject;
        message.Body = new TextPart("plain") { Text = plainTextBody };

        using var client = new SmtpClient();
        await client.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(sender, password);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}
