using IotDash.Parsing;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace IotDash.Contracts.V1 {

    [AttributeUsage(AttributeTargets.Property)]
    internal class ValidAliasAttribute : ValidationAttribute {

        char[] forbiddenChars = new[] { ' ', '#', '+' };

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
            => value == null
                ? ValidationResult.Success
                : value is string valueString && !forbiddenChars.Any(c => valueString.Contains(c))
                    ? ValidationResult.Success
                    : new ValidationResult("Alias must be a valid MQTT topic name or null.");
    }
}