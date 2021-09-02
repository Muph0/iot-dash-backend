using Microsoft.Extensions.Configuration;
using System;

namespace IotDash.Settings {

    public abstract class Settings {
        public static T LoadFrom<T>(IConfiguration configuration) where T : Settings, new() {
            T result = new();
            string sectionName = typeof(T).Name;

            var section = configuration.GetSection(sectionName);
            if (!section.Exists()) {
                throw new Exception($"Section \"{sectionName}\" doesn't exist.");
            }

            section.Bind(result);
            return result;
        }
    }

}