using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Collections.Generic;
using IotDash.Utils;
using IotDash.Utils.Nullables;

namespace IotDash.Settings {

    /// <summary>
    /// Base class for keys of <c>appsetings.json</c>.
    /// In this project, settings objects (in namespace <see cref="IotDash.Settings"/>) are 
    /// automatically created and populated during application startup.
    /// </summary>
    /// <typeparam name="TInherited"></typeparam>
    public abstract class Settings<TInherited> where TInherited : Settings<TInherited>, new() {

        public static TInherited LoadFrom(IConfiguration configuration, string? path = null) {
            TInherited result = new();
            string sectionName = typeof(TInherited).Name;

            if (sectionName.EndsWith(nameof(Settings))) {
                sectionName = sectionName.Substring(0, sectionName.Length - nameof(Settings).Length);
            }

            IConfiguration parentSection = configuration;
            if (path != null) {
                parentSection = configuration.GetSection(path);
            }

            List<string> errors = new();
            if (parentSection.GetChildren().All(kv => kv.Key != sectionName)) {
                errors.Add($"Section \"{sectionName}\" doesn't exist.");
            }

            var section = parentSection.GetSection(sectionName);
            CheckBindCompatibility(typeof(TInherited), section, errors);

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