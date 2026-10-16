using System.Collections.Generic;
using Database.Configuration;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace PollingWeb.UnitTests
{
    public class AppConfigTests
    {
        [Fact]
        public void Fake_values_and_connections_are_resolved_without_legacy_configuration()
        {
            var config = new AppConfig(
                new Dictionary<string, string> { ["smtpServer"] = "mail.example.test" },
                new Dictionary<string, string> { ["PollingConnectionString"] = "Server=localhost;Database=Polling;" },
                useLegacyConfiguration: false);

            Assert.Equal("mail.example.test", config.GetValue("smtpServer"));
            Assert.Equal("Server=localhost;Database=Polling;", config.GetConnectionString("PollingConnectionString"));
        }

        [Fact]
        public void Configuration_provider_supplies_json_style_keys()
        {
            var values = new Dictionary<string, string>
            {
                ["ConnectionStrings:PollingConnectionString"] = "Server=localhost;Database=Polling;",
                ["smtpServer"] = "mail.example.test"
            };
            var configuration = new ConfigurationBuilder().AddInMemoryCollection(values).Build();
            var config = new AppConfig(configuration: configuration, useLegacyConfiguration: false);

            Assert.Equal("Server=localhost;Database=Polling;", config.GetConnectionString("PollingConnectionString"));
            Assert.Equal("mail.example.test", config.GetValue("smtpServer"));
        }

        [Fact]
        public void Missing_value_returns_null_without_legacy_configuration()
        {
            var config = new AppConfig(useLegacyConfiguration: false);
            Assert.Null(config.GetConnectionString("missing"));
            Assert.Null(config.GetValue("missing"));
        }
    }
}
