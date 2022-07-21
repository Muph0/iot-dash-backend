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
                try {
                    var result = ExpressionsParser.ParseOrThrow(expressionString);
                } catch (Pidgin.ParseException e) {
                    return new ValidationResult(e.Message);
                } catch (Exception e) {
                    return new ValidationResult(e.Message);
                }

                return ValidationResult.Success;
            }

            return new ValidationResult("Expression must be a string");
        }
    }
}