using IotDash.Contracts.V1.Model;
using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace IotDash.Contracts.V1 {
    public class InterfaceCreateRequest : InterfacePatchRequest {

        [Required]
        public InterfaceKind Kind { get; set; }

        public Data.Model.IotInterface CreateModel(Guid deviceId, int ifaceId) {
            return new() {
                Id = ifaceId,
                Alias = Alias,
                DeviceId = deviceId,
                Expression = Expression,
                Kind = Kind,
            };
        }
    }

    public class InterfacePatchRequest {

        [ValidAlias]
        [MaxLength(ContractedConstraints.AliasMaxLength)]
        public string? Alias { get; set; }

        [ValidExpression]
        [MaxLength(ContractedConstraints.ExpressionMaxLength)]
        public string? Expression { get; set; }

        public bool? LogHistory { get; set; }
    }

}