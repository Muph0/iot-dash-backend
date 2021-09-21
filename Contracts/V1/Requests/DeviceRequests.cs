using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;


namespace IotDash.Contracts.V1 {

    public class DeviceCreateRequest {

        [Required]
        public string HwId { get; set; }
        [Required]
        public string OwnerEmail { get; set; }

        public string? IpAddress { get; set; }

        public bool? Virtual { get; set; } = false;


        [ValidAlias]
        [MaxLength(ContractedConstraints.AliasMaxLength)]
        public string? Alias { get; internal set; }
        [Required]
        public IEnumerable<InterfaceCreateRequest> Interfaces { get; set; }
    }



    public class DevicePatchRequest {

        [ValidAlias]
        [MaxLength(ContractedConstraints.AliasMaxLength)]
        public string? Alias { get; set; }

        public string? OwnerEmail { get; set; }
    }

}
