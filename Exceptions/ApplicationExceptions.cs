using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace IotDash.Exceptions {

    public class ResourceNotFoundException : ApplicationException {
        public ResourceNotFoundException(string? message = null, Exception? innerException = null)
            : base(message, innerException) { }
    }

    public class OperationForbiddenException : ApplicationException {
        public OperationForbiddenException(string? message = null, Exception? innerException = null)
            : base(message, innerException) { }
    }

    public class UnauthorizedException : ApplicationException {
        public UnauthorizedException(string? message = null, Exception? innerException = null)
            : base(message, innerException) { }
    }

    public class BadRequestException : ApplicationException {
        public BadRequestException(string? message = null, Exception? innerException = null)
            : base(message, innerException) { }
    }
}
