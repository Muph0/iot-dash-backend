using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace IotDash.Data.Model {
    public abstract class ModelObject {
        public string _JsonTrap => throw new Exception("This object should not be passed to clients.");

        public override string ToString() {
            Type type = GetType();
            while (type.BaseType != typeof(ModelObject)) {
                Debug.Assert(type.BaseType != null);
                type = type.BaseType;
            }

            return string.Join(", ", type.GetProperties()
                .Where(p => !p.Name.StartsWith("_") && p.GetCustomAttribute<ForeignKeyAttribute>() == null)
                .Select(p => $"{p.Name}={{{p.GetValue(this)}}}"));
        }
    }
}