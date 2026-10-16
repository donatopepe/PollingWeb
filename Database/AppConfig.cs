using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using System.Configuration;

namespace Database.Configuration
{
    /// <summary>Small configuration boundary for domain/service components.</summary>
    public interface IAppConfig
    {
        string GetConnectionString(string name);
        string GetValue(string name);
    }

    /// <summary>Environment-first provider with appsettings.json and legacy config fallbacks.</summary>
    public sealed class AppConfig : IAppConfig
    {
        private readonly IDictionary<string, string> values;
        private readonly IDictionary<string, string> connectionStrings;
        private readonly IConfiguration configuration;
        private readonly bool useLegacyConfiguration;

        public AppConfig(IDictionary<string, string> values = null,
            IDictionary<string, string> connectionStrings = null, bool useLegacyConfiguration = true,
            IConfiguration configuration = null)
        {
            this.values = values ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            this.connectionStrings = connectionStrings ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            this.configuration = configuration ?? CreateDefaultConfiguration();
            this.useLegacyConfiguration = useLegacyConfiguration;
        }

        private static IConfiguration CreateDefaultConfiguration()
        {
            var builder = new Microsoft.Extensions.Configuration.ConfigurationBuilder().AddEnvironmentVariables();
#if NET8_0
            builder.AddJsonFile("appsettings.json", optional: true);
#endif
            return builder.Build();
        }

        public string GetConnectionString(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("A connection string name is required.", nameof(name));
            string value;
            if (connectionStrings.TryGetValue(name, out value) && !string.IsNullOrWhiteSpace(value)) return value;

            value = configuration?["ConnectionStrings:" + name];
            if (!string.IsNullOrWhiteSpace(value)) return value;

            value = Environment.GetEnvironmentVariable("ConnectionStrings__" + name);
            if (!string.IsNullOrWhiteSpace(value)) return value;

            if (useLegacyConfiguration)
            {
                var setting = System.Configuration.ConfigurationManager.ConnectionStrings[name];
                if (setting != null && !string.IsNullOrWhiteSpace(setting.ConnectionString)) return setting.ConnectionString;
            }
            return null;
        }

        public string GetValue(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("A configuration key is required.", nameof(name));
            string value;
            if (values.TryGetValue(name, out value) && !string.IsNullOrWhiteSpace(value)) return value;

            value = configuration?[name];
            if (!string.IsNullOrWhiteSpace(value)) return value;

            value = Environment.GetEnvironmentVariable(name);
            if (!string.IsNullOrWhiteSpace(value)) return value;

            if (useLegacyConfiguration)
            {
                value = System.Configuration.ConfigurationManager.AppSettings[name];
                if (!string.IsNullOrWhiteSpace(value)) return value;
            }
            return null;
        }
    }
}
