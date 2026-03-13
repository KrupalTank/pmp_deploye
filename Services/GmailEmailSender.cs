using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace PlacementMentorshipPortal.Services
{
    public class GmailEmailSender
    {
        private readonly IConfiguration _config;

        public GmailEmailSender(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendAsync(MimeMessage message)
        {
            var host = _config["Smtp:Host"] ?? "smtp.gmail.com";
            var port = int.TryParse(_config["Smtp:Port"], out var p) ? p : 587;
            var user = _config["Smtp:Username"];
            var pass = _config["Smtp:Password"];
            var useStartTls = bool.TryParse(_config["Smtp:UseStartTls"], out var s) ? s : true;

            using var client = new SmtpClient();
            // ADD THIS LINE: It allows the SSL handshake to proceed in cloud environments
            client.ServerCertificateValidationCallback = (s, c, h, e) => true;
            //var secureOption = useStartTls ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto;
            var secureOption = (port == 465) 
                ? SecureSocketOptions.SslOnConnect 
                : SecureSocketOptions.StartTls;
            await client.ConnectAsync(host, port, secureOption).ConfigureAwait(false);

            if (!string.IsNullOrWhiteSpace(user))
            {
                await client.AuthenticateAsync(user, pass).ConfigureAwait(false);
            }

            await client.SendAsync(message).ConfigureAwait(false);
            await client.DisconnectAsync(true).ConfigureAwait(false);
        }


    }
}
