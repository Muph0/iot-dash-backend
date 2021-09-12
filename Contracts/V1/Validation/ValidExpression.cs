using IotDash.Parsing;
using System;
using System.ComponentModel.DataAnnotations;

namespace IotDash.Contracts.V1 {

    [AttributeUsage(AttributeTargets.Property)]
    internal class ValidExpressionAttribute : ValidationAttribute {

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext) {

            if (value == null) {
                return ValidationResult.Success;
            }
            if (value is string expressionString) {
                var result = ExpressionsParser.Parse(expressionString);
                return result.Success
                    ? ValidationResult.Success
                    : new ValidationResult(result.Error.RenderErrorMessage());
            }

            return new ValidationResult("Expression must be a string");
        }
    }
}