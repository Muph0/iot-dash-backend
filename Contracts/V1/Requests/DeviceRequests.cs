using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;


namespace IotDash.Contracts.V1 {

    /// <summary>
    /// REST API model of a device creation request.
    /// </summary>
    public class DeviceCreateRequest {


        /// <summary>
        /// An identifier that is unique for that fabrication of the device.
        /// </summary>
        [Required]
        public string HwId { get; set; }

        /// <summary>
        /// IP address the device can be found on.
        /// </summary>
        public string? IpAddress { get; set; }

        /// <summary>
        /// Alias that the device uses for an MQTT the default topic name.
        /// </summary>
        [ValidAlias]
        [MaxLength(ContractedConstraints.AliasMaxLength)]
        public string? Alias { get; internal set; }

        /// <summary>
        /// Collection of interfaces to be created.
        /// </summary>
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
