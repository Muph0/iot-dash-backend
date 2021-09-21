using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IotDash.Data.Model {
    public class HistoryEntry : ModelObject {

        public int InterfaceId { get; set; }
        public Guid DeviceId { get; set; }
        public DateTime When { get; set; }

        public double Min { get; set; }
        public double Max { get; set; }
        public double Average { get; set; }

        [ForeignKey(nameof(InterfaceId) + "," + nameof(DeviceId))]
        public virtual IotInterface Interface { get; set; }
    }
}