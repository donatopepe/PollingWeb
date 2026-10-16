using System.Collections.Generic;
using System.Threading.Tasks;
using Database.Configuration;
using Microsoft.Extensions.Configuration;
using Database.Base;
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
        public void DatabaseInfo_instances_keep_connection_and_timeout_state_isolated_under_parallel_load()
        {
            const int count = 100;
            var instances = new DatabaseInfo[count];
            Parallel.For(0, count, i =>
            {
                var values = new Dictionary<string, string> { ["POLLING_DB_COMMAND_TIMEOUT_SECONDS"] = (i + 1).ToString() };
                var connections = new Dictionary<string, string>
                {
                    ["PollingConnectionString"] = "Server=localhost;Database=Polling" + i + ";"
                };
                instances[i] = new DatabaseInfo(new AppConfig(values, connections, useLegacyConfiguration: false));
            });

            for (var i = 0; i < count; i++)
            {
                Assert.Equal(i + 1, instances[i].CommandTimeoutSeconds);
                Assert.Equal("localhost", instances[i].connessione.DataSource);
                Assert.Equal("Polling" + i, instances[i].connessione.Database);
                instances[i].Dispose();
            }
        }

        [Fact]
        public void DatabaseInfo_uses_safe_default_for_missing_or_invalid_timeout()
        {
            var connection = new Dictionary<string, string> { ["PollingConnectionString"] = "Server=localhost;Database=Polling;" };
            Assert.Equal(30, new DatabaseInfo(new AppConfig(connectionStrings: connection, useLegacyConfiguration: false)).CommandTimeoutSeconds);
            Assert.Equal(30, new DatabaseInfo(new AppConfig(
                new Dictionary<string, string> { ["POLLING_DB_COMMAND_TIMEOUT_SECONDS"] = "invalid" }, connection, useLegacyConfiguration: false)).CommandTimeoutSeconds);
        }

        [Fact]
        public void Connection_string_validation_requires_server_database_and_valid_syntax()
        {
            Assert.Throws<System.Configuration.ConfigurationErrorsException>(() => new AppConfig(
                connectionStrings: new Dictionary<string, string> { ["PollingConnectionString"] = "Database=Polling;" },
                useLegacyConfiguration: false).GetConnectionString("PollingConnectionString"));
            Assert.Throws<System.Configuration.ConfigurationErrorsException>(() => new AppConfig(
                connectionStrings: new Dictionary<string, string> { ["PollingConnectionString"] = "not-a-connection-string" },
                useLegacyConfiguration: false).GetConnectionString("PollingConnectionString"));
        }

        [Fact]
        public void Legacy_provider_name_is_validated()
        {
            Assert.Throws<System.Configuration.ConfigurationErrorsException>(() => AppConfig.ValidateProviderName(
                "PollingConnectionString", "System.Data.EntityClient"));
            AppConfig.ValidateProviderName("PollingConnectionString", "Microsoft.Data.SqlClient");
        }

        [Fact]
        public void Sap_text_reader_uses_utf8_and_detects_unicode_bom()
        {
            var directory = System.IO.Path.Combine(System.IO.Path.GetTempPath(), System.Guid.NewGuid().ToString("N"));
            System.IO.Directory.CreateDirectory(directory);
            var path = System.IO.Path.Combine(directory, "sap.txt");
            try
            {
                System.IO.File.WriteAllText(path, "città", new System.Text.UTF8Encoding(true));
                using (var reader = PollingClass.ConfiguredFilePaths.OpenSapText(path))
                    Assert.Equal("città", reader.ReadToEnd());
                System.IO.File.WriteAllText(path, "", new System.Text.UTF8Encoding(false));
                using (var reader = PollingClass.ConfiguredFilePaths.OpenSapText(path))
                    Assert.Equal(string.Empty, reader.ReadToEnd());
            }
            finally
            {
                System.IO.Directory.Delete(directory, recursive: true);
            }
        }

        [Fact]
        public void Configured_paths_require_existing_directory_and_bare_file_name()
        {
            var directory = System.IO.Path.Combine(System.IO.Path.GetTempPath(), System.Guid.NewGuid().ToString("N"));
            System.IO.Directory.CreateDirectory(directory);
            try
            {
                Assert.Equal(System.IO.Path.Combine(directory, "input.dat"), PollingClass.ConfiguredFilePaths.Combine(directory, "input.dat"));
                Assert.Throws<System.IO.DirectoryNotFoundException>(() => PollingClass.ConfiguredFilePaths.Combine(directory + "-missing", "input.dat"));
                Assert.Throws<System.ArgumentException>(() => PollingClass.ConfiguredFilePaths.Combine(directory, "sub/input.dat"));
            }
            finally
            {
                System.IO.Directory.Delete(directory);
            }
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
