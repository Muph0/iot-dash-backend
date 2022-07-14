using IotDash.Contracts.V1;
using IotDash.Contracts.V1.Model;
using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;


namespace IotDash.Data.Model {

    using MQTTInterface_KeyType = Guid;

    public class IotInterface : ModelObject {


        [Key]
        public Guid Id { get; set; }

        [MaxLength(IotDash.Data.Constraints.TopicMaxLength)]
        public string? Topic { get; set; }

        public InterfaceKind Kind { get; set; }

        [MaxLength(36)]
        public string OwnerId { get; set; }

        [ForeignKey(nameof(OwnerId))]
        public virtual IdentityUser Owner { get; set; }

        [MaxLength(1 << 16)]
        [Column(TypeName = "TEXT")]
        public string? Expression { get; set; }

        public double Value { get; set; }

        public bool HistoryEnabled { get; set; } = false;

        internal string GetTopicName() {
            return Topic ?? $"interface/{Id}";
        }

        public override string ToString() => $"MQTT[\"{GetTopicName()}\"]";

        /// <summary>
        /// </summary>
        /// <returns>true if this interface needs an <see cref="Services.Evaluation.InterfaceEvaluator"/>.</returns>
        internal bool NeedsEvaluator() => Kind == InterfaceKind.Switch && Expression != null;

        /// <summary>
        /// </summary>
        /// <returns>true if this interface needs a <see cref="Services.History.HistoryWriter"/>.</returns>
        internal bool NeedsWriter() => true;
    }

}