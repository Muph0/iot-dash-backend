using System;
using System.Collections.Generic;

namespace IotDash.Contracts.V1 {

    /// <summary>
    /// Static collection of REST API error messages.
    /// </summary>
    internal struct Error {

        #region Login_Registration_Related
        public static string NoSuchUser() => "No such user exists.";
        public static string NoModificationsInRequest() => "At least one change must be specified.";
        public static string DeviceAlreadyDeleted() => "Device has already been deleted.";
        public static string BadUserPasswordCombo() => "User/password combination is wrong.";
        public static string EmailTaken() => "User with this email already exists.";
        #endregion

        #region Token_Related
        public static string JwtIsNotExpired(DateTime expiryDateUtc) => $"This token has not expired yet. {expiryDateUtc - DateTime.UtcNow} remaining.";
        public static string RefreshTokenNonExistent() => "This refresh token does not exist.";
        public static string RefreshTokenExpired() => "Refresh token is expired.";
        public static string RefreshTokenUsedAlready() => "Refresh token has been used.";
        public static string RefreshTokenJwtMismatch() => "Refresh token does not match this JWT.";
        public static string InterfaceAlreadyExists() => "Interface with specified id already exists on this device.";
        public static string ExpressionOnReadOnlyIface() => "Read only interfaces cannot have an expression.";
        #endregion
    }
}