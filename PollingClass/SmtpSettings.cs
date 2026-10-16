using System;
using System.Globalization;
using System.Net.Mail;
using Database.Configuration;

namespace PollingClass
{
    public sealed class SmtpSettings
    {
        public string Host { get; private set; }
        public int Port { get; private set; }
        public bool EnableSsl { get; private set; }
        public bool UseDefaultCredentials { get; private set; }
        public string UserName { get; private set; }
        public string Password { get; private set; }

        public static SmtpSettings Load(IAppConfig configuration)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));
            var host = configuration.GetValue("POLLING_SMTP_HOST");
            if (string.IsNullOrWhiteSpace(host)) host = configuration.GetValue("smtpServer");
            if (string.IsNullOrWhiteSpace(host)) throw new InvalidOperationException("SMTP host is not configured (POLLING_SMTP_HOST).");
            if (host.Contains("${") || host.Contains("<")) throw new InvalidOperationException("SMTP host contains an unresolved placeholder.");
            var portValue = configuration.GetValue("POLLING_SMTP_PORT");
            int port;
            if (string.IsNullOrWhiteSpace(portValue)) port = 25;
            else if (!int.TryParse(portValue, NumberStyles.None, CultureInfo.InvariantCulture, out port) || port < 1 || port > 65535)
                throw new InvalidOperationException("SMTP port must be an integer between 1 and 65535.");

            var sslValue = configuration.GetValue("POLLING_SMTP_ENABLE_SSL");
            bool ssl = false;
            if (!string.IsNullOrWhiteSpace(sslValue) && !bool.TryParse(sslValue, out ssl))
                throw new InvalidOperationException("POLLING_SMTP_ENABLE_SSL must be true or false.");
            if (string.IsNullOrWhiteSpace(sslValue)) ssl = false;

            var defaultCredentialsValue = configuration.GetValue("POLLING_SMTP_USE_DEFAULT_CREDENTIALS");
            bool useDefaultCredentials = true;
            if (!string.IsNullOrWhiteSpace(defaultCredentialsValue) && !bool.TryParse(defaultCredentialsValue, out useDefaultCredentials))
                throw new InvalidOperationException("POLLING_SMTP_USE_DEFAULT_CREDENTIALS must be true or false.");
            if (string.IsNullOrWhiteSpace(defaultCredentialsValue)) useDefaultCredentials = true;

            var user = configuration.GetValue("POLLING_SMTP_USERNAME");
            var password = configuration.GetValue("POLLING_SMTP_PASSWORD");
            if (!useDefaultCredentials && (string.IsNullOrWhiteSpace(user) || string.IsNullOrEmpty(password)))
                throw new InvalidOperationException("SMTP username and password are required when default credentials are disabled.");
            return new SmtpSettings { Host = host, Port = port, EnableSsl = ssl, UseDefaultCredentials = useDefaultCredentials, UserName = user, Password = password };
        }

        public static void ValidateRecipientList(string recipients)
        {
            if (string.IsNullOrWhiteSpace(recipients)) throw new ArgumentException("At least one SMTP recipient is required.", nameof(recipients));
            foreach (var address in recipients.Split(';'))
            {
                MailAddress parsed;
                try { parsed = new MailAddress(address.Trim()); }
                catch (FormatException ex) { throw new ArgumentException("SMTP recipient list contains an invalid address.", nameof(recipients), ex); }
                if (!string.Equals(parsed.Address, address.Trim(), StringComparison.OrdinalIgnoreCase))
                    throw new ArgumentException("SMTP recipient list contains an invalid address.", nameof(recipients));
            }
        }
    }
}
