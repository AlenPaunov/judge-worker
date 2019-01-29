﻿namespace OJS.Workers.Common.Helpers
{
    using System;

    using Microsoft.Extensions.Configuration;

    public static class SettingsHelper
    {
        private static readonly IConfiguration Configuration;

        static SettingsHelper() => Configuration = BuildConfiguration();

        public static string GetSetting(string settingName)
            => GetSection(settingName)?.Value
                ?? throw new Exception($"{settingName} setting not found in Config file!");

        public static T GetSettingOrDefault<T>(string settingName, T defaultValue)
        {
            var section = GetSection(settingName);

            return section?.Value == null
                ? defaultValue
                : (T)Convert.ChangeType(section.Value, typeof(T));
        }

        private static IConfigurationSection GetSection(string settingName)
        {
            var section = Configuration.GetSection($"OjsWorkersConfig:{settingName}");

            if (LegacyConfigurationProvider.HasSettings())
            {
                section = Configuration.GetSection($"{settingName}");
            }

            return section;
        }

        private static IConfiguration BuildConfiguration()
        {
            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            return new ConfigurationBuilder()
                .AddJsonFile("appSettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appSettings.{env}.json", optional: true, reloadOnChange: true)
                .Add(new LegacyConfigurationProvider())
                .Build();
        }
    }
}
