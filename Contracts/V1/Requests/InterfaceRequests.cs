using IotDash.Contracts.V1.Model;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace IotDash.Contracts.V1 {
    public class InterfaceCreateRequest : InterfacePatchRequest {

        [Required]
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