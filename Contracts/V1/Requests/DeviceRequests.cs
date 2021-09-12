using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;


namespace IotDash.Contracts.V1 {

    public class DeviceCreateRequest {

        public string HwId { get; set; }
        public string OwnerEmail { get; set; }


        [ValidAlias]
        [MaxLength(ContractedConstraints.AliasMaxLength)]
        public string? Alias { get; internal set; }
        public IEnumerable<CreateInterfaceRequest> Interfaces { get; set; }
    }


    public enum InterfaceKind {
        Probe, Switch
    }

    public class DevicePatchRequest {

        [ValidAlias]
        [MaxLength(ContractedConstraints.AliasMaxLength)]
        public string? Alias { get; set; }

        public string? OwnerEmail { get; set; }
    }

}
