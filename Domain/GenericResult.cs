using System.Collections.Generic;
using System.Linq;

namespace IotDash.Domain {
    public interface IOperationResult {
        public bool IsSuccess { get; }
        public IEnumerable<string> Errors { get; }
    }

    public class GenericResult<T> : IOperationResult {

        public bool IsSuccess { get; private set; }
        public IEnumerable<string> Errors { get; private set; }
        public T Value { get; private set; }

        public static GenericResult<T> Ok(T value) {
            return new() {
                Errors = Enumerable.Empty<string>(),
                IsSuccess = true,
                Value = value,
            };
        }

        public static GenericResult<T> Fail(string errorMessage) => Fail(new[] { errorMessage });
        public static GenericResult<T> Fail(IEnumerable<string> errors) {
            return new () {
                Errors = errors.Select(x => x),
                IsSuccess = false,
                Value = default,
            };
        }
    }
}