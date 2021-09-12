using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace IotDash.Contracts.V1 {
    public class CreateInterfaceRequest : InterfacePatchRequest {

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public InterfaceKind Kind { get; set; }
    }

    public class InterfacePatchRequest {

        [ValidAlias]
        [MaxLength(ContractedConstraints.AliasMaxLength)]
        public string? Alias { get; set; }

        [ValidExpression]
        [MaxLength(ContractedConstraints.ExpressionMaxLength)]
        public string? Expression { get; set; }
    }

}