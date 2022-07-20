using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

namespace IotDash.Utils.Nullables {
    public static class NullableExtensions {

        public static bool IsNullable(this PropertyInfo property) =>
            IsNullableHelper(property.PropertyType, property.DeclaringType, property.CustomAttributes);

        public static bool IsNullable(this FieldInfo field) =>
            IsNullableHelper(field.FieldType, field.DeclaringType, field.CustomAttributes);

        public static bool IsNullable(this ParameterInfo parameter) =>
            IsNullableHelper(parameter.ParameterType, parameter.Member, parameter.CustomAttributes);

        private static bool IsNullableHelper(Type memberType, MemberInfo? declaringType, IEnumerable<CustomAttributeData> customAttributes) {
            if (memberType.IsValueType)
                return Nullable.GetUnderlyingType(memberType) != null;

            var nullable = customAttributes
                .FirstOrDefault(x => x.AttributeType.FullName == "System.Runtime.CompilerServices.NullableAttribute");
            if (nullable != null) {
                return true;
            }

            for (var type = declaringType; type != null; type = type.DeclaringType) {
                var context = type.CustomAttributes
                    .FirstOrDefault(x => x.AttributeType.FullName == "System.Runtime.CompilerServices.NullableContextAttribute");
                if (context != null) {
                    return true;
                }
            }

            // Couldn't find a suitable attribute
            return false;
        }
    }
}