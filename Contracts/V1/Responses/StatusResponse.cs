using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Text.Json.Serialization;

namespace IotDash.Contracts.V1 {
    public interface IStatusResponse {
        bool Success { get; }
        IEnumerable<string>? Errors { get; }
        object? Value { get; }
    }

    public abstract class StatusResponse<TValue, TInherited> : IStatusResponse where TInherited : StatusResponse<TValue, TInherited>, new() {

        [Required]
        public bool Success { get; private init; }
        public IEnumerable<string>? Errors { get; private init; }
        
        [JsonIgnore]
        public TValue? Value { get; private set; }
        object? IStatusResponse.Value => this.Value;

        public static TInherited Succeed(TValue value) {
            return new TInherited() {
                Errors = Enumerable.Empty<string>(),
                Success = true,
                Value = value,
            };
        }
        public static TInherited Fail(string errorMessage) => Fail(new[] { errorMessage });
        public static TInherited Fail(IEnumerable<string> errors) {
            return new TInherited() {
                Errors = errors.Select(x => x),
                Success = false,
                Value = default,
            };
        }

        #region Static_IActionResult_Factories
        public static BadRequestObjectResult BadRequest(IEnumerable<string> errors)
            => Fail(errors).AsBadRequest();
        public static BadRequestObjectResult BadRequest(string error)
            => Fail(error).AsBadRequest();

        public static ConflictObjectResult Conflict(IEnumerable<string> errors)
            => Fail(errors).AsConflict();
        public static ConflictObjectResult Conflict(string error)
            => Fail(error).AsConflict();

        public static NotFoundObjectResult NotFound(IEnumerable<string> errors)
            => Fail(errors).AsNotFound();
        public static NotFoundObjectResult NotFound(string error)
            => Fail(error).AsNotFound();

        public static NoContentResult NoContent()
            => new NoContentResult();

        public static OkObjectResult Ok(TValue value)
            => Succeed(value).AsOk();

        public static IActionResult Created(string locationUri, TValue value)
            => new CreatedAtRouteResult(locationUri, value);
        #endregion

        #region Instance_IActionResult_Factories
        public BadRequestObjectResult AsBadRequest() {
            Debug.Assert(!Success);
            return new BadRequestObjectResult(this);
        }
        public NotFoundObjectResult AsNotFound() {
            Debug.Assert(!Success);
            return new NotFoundObjectResult(this);
        }
        public ConflictObjectResult AsConflict() {
            Debug.Assert(!Success);
            return new ConflictObjectResult(this);
        }
        public NoContentResult AsNoContent() {
            Debug.Assert(Success);
            return new NoContentResult();
        }
        public OkObjectResult AsOk() {
            Debug.Assert(Success && Value != null);
            return new OkObjectResult(this);
        }
        public IActionResult AsOkOrBadRequest() {
            if (Success) {
                return AsOk();
            }
            return AsBadRequest();
        }
        #endregion
    }
}
