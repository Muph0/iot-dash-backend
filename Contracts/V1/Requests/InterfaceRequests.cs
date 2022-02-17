using IotDash.Contracts.V1.Model;
using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace IotDash.Contracts.V1 {
    public class InterfaceCreateRequest : InterfacePatchRequest {

        [Required]
        public new InterfaceKind Kind { get; set; }

        public Data.Model.IotInterface CreateModel(Guid ifaceId, IdentityUser owner) {
            return new() {
                OwnerId = owner.Id,
                Id = ifaceId,
                Topic = Topic,
                Expression = Expression,
                Kind = Kind,
            };
        }
    }

    public class InterfacePatchRequest {

        [ValidAlias]
        [MaxLength(ContractedConstraints.AliasMaxLength)]
        public string? Topic { get; set; }

        [ValidExpression]
        [MaxLength(ContractedConstraints.ExpressionMaxLength)]
        public string? Expression { get; set; }

        public bool? HistoryEnabled { get; set; }

        public InterfaceKind? Kind { get; set; }
    }

}