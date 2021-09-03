using System;
using System.ComponentModel.DataAnnotations;

#nullable enable

namespace IotDash.Contracts.V1 {

    public class CreateDeviceRequest {
        [Required]
        public string HardwareId { get; set; }

        
    }

    public class CreateInterfaceRequest {

    }

    public class UpdateDeviceRequest {
        public string? Name { get; set; }
    }

}