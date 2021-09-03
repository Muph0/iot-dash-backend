using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Collections.Generic;
using IotDash.Extensions;

namespace IotDash.Settings {

    public abstract class Settings {

        public static T LoadFrom<T>(IConfiguration configuration) where T : Settings, new() {
            T result = new();
            string sectionName = typeof(T).Name;

            if (sectionName.EndsWith(nameof(Settings))) {
                sectionName = sectionName.Substring(0, sectionName.Length - nameof(Settings).Length);
            }

            List<string> errors = new();
            if (configuration.GetChildren().All(kv => kv.Key != sectionName)) {
                errors.Add($"Section \"{sectionName}\" doesn't exist.");
            }

            var section = configuration.GetSection(sectionName);
            CheckBindCompatibility(typeof(T), section, errors);

            if (errors.Count != 0) {
                throw new FormatException("Bad settings format.\n" + string.Join('\n', errors));
            }

            section.Bind(result);
            return result;
        }

        private static void CheckBindCompatibility(Type type, IConfigurationSection section, List<string> errors) {

            var children = section.GetChildren();

            // check for required properties
            foreach (var prop in type.GetProperties()) {
                var subsection = children.SingleOrDefault(kv => kv.Key == prop.Name);
                bool required = !prop.IsNullable();
                var subtype = prop.PropertyType;

                if (subsection == null) {
                    if (required) {
                        errors.Add($"Section \"{section.Path}\" is missing required key \"{prop.Name}\".");
                    }
                } else if (subsection.Value == null) {
                    CheckBindCompatibility(prop.PropertyType, subsection, errors);
                }
            }

            // check for unknown keys
            foreach (var kv in children) {
                var prop = type.GetProperty(kv.Key);
                if (prop == null || !prop.CanWrite) {
                    errors.Add($"Section \"{section.Path}\" has unknown key \"{kv.Key}\".");
                }
            }
        }
    }

}