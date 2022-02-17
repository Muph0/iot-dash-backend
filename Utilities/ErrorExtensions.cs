using IotDash.Contracts.V1;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Collections.Generic;
using System.Linq;

namespace IotDash.Utils.Error {
    internal static class ErrorExtensions {

        public static IEnumerable<string> ErrorMessages(this ModelStateDictionary ModelState) {
            return ModelState.Select(kv => $"{kv.Key}: {string.Join(", ", kv.Value.Errors.Select(err => err.ErrorMessage))}");
        }
    }
}
