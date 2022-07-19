using IotDash.Data.Model;
using IotDash.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IotDash.Services.Auth {

    internal class AuthEvaluator : IAuthorizationEvaluator {
        public AuthorizationResult Evaluate(AuthorizationHandlerContext context) {

            if (context.HasSucceeded) {
                return AuthorizationResult.Success();
            } else {
                if (context.HasFailed) {
                    return AuthorizationResult.Failed(AuthorizationFailure.ExplicitFail());
                } else {
                    return AuthorizationResult.Failed(AuthorizationFailure.Failed(context.PendingRequirements));
                }
            }

        }
    }
}