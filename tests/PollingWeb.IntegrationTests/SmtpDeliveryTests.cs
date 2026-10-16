using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Database.Configuration;
using PollingClass;
using Xunit;

namespace PollingWeb.IntegrationTests
{
    public class SmtpDeliveryTests
    {
        [Fact]
        public void MailSede_sends_message_to_local_smtp_sink()
        {
            using (var listener = new TcpListener(IPAddress.Loopback, 0))
            {
                listener.Start();
                var port = ((IPEndPoint)listener.LocalEndpoint).Port;
                var received = new ManualResetEventSlim(false);
                var server = new Thread(() =>
                {
                    using (var client = listener.AcceptTcpClient())
                    using (var stream = client.GetStream())
                    {
                        var greeting = Encoding.ASCII.GetBytes("220 localhost ESMTP\r\n");
                        stream.Write(greeting, 0, greeting.Length);
                        var buffer = new byte[4096];
                        var data = new StringBuilder();
                        var inData = false;
                        while (true)
                        {
                            var count = stream.Read(buffer, 0, buffer.Length);
                            if (count <= 0) break;
                            var text = Encoding.ASCII.GetString(buffer, 0, count);
                            data.Append(text);
                            var response = new StringBuilder();
                            foreach (var line in data.ToString().Split(new[] { "\r\n" }, StringSplitOptions.None))
                            {
                                if (line.Length == 0) continue;
                                if (inData)
                                {
                                    if (line == ".")
                                    {
                                        response.Append("250 queued\r\n");
                                        inData = false;
                                        received.Set();
                                    }
                                    continue;
                                }
                                var upper = line.ToUpperInvariant();
                                if (upper.StartsWith("EHLO") || upper.StartsWith("HELO")) response.Append("250 localhost\r\n");
                                else if (upper.StartsWith("MAIL FROM") || upper.StartsWith("RCPT TO")) response.Append("250 OK\r\n");
                                else if (upper == "DATA") { response.Append("354 End data with <CR><LF>.<CR><LF>\r\n"); inData = true; }
                                else if (upper == "QUIT") { response.Append("221 bye\r\n"); var bytes = Encoding.ASCII.GetBytes(response.ToString()); stream.Write(bytes, 0, bytes.Length); return; }
                                else response.Append("250 OK\r\n");
                            }
                            data.Clear();
                            if (response.Length > 0) { var bytes = Encoding.ASCII.GetBytes(response.ToString()); stream.Write(bytes, 0, bytes.Length); }
                            if (received.IsSet) continue;
                        }
                    }
                }) { IsBackground = true };
                server.Start();
                var configuration = new AppConfig(new System.Collections.Generic.Dictionary<string, string>
                {
                    ["POLLING_SMTP_HOST"] = "127.0.0.1",
                    ["POLLING_SMTP_PORT"] = port.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    ["POLLING_SMTP_ENABLE_SSL"] = "false",
                    ["POLLING_SMTP_USE_DEFAULT_CREDENTIALS"] = "true"
                }, useLegacyConfiguration: false);
                MailSede.Configuration = configuration;
                new MailSede().Invia("recipient@example.test", "sender@example.test", "synthetic test", "synthetic body");
                Assert.True(received.Wait(TimeSpan.FromSeconds(5)), "SMTP sink did not receive the message.");
                listener.Stop();
                Assert.True(server.Join(TimeSpan.FromSeconds(5)));
            }
        }
    }
}
