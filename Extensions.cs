using System;
using Microsoft.Extensions.Configuration;

namespace PublisherNonBlocking {
    
    /// <summary>
    /// Extension methods for managing commandline parameters.
    /// </summary>
    internal static class Extensions {

        internal static T GetEnum<T>(this IConfiguration config, string key, T value) {
            T enumValue = config.GetValue<T>(key);
            return (!Enum.Equals(enumValue, default(T))) ? enumValue : value;
        }

        internal static int GetInteger(this IConfiguration config, string key, int value) {
            if (string.IsNullOrWhiteSpace(config[key])) {
                return value;
            } else {
                return config.GetValue<int>(key);
            }
        }

        internal static string GetString(this IConfiguration config, string key, string value) {
            string token;
            return !string.IsNullOrWhiteSpace(token = config.GetValue<string>(key)) ? token : value;
        }

    }
    
}