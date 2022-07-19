using System;
using System.ComponentModel.DataAnnotations;

namespace IotDash.Contracts.V1.Model {

    /// <summary>
    /// Represents an server-to-client event.
    /// This event is fired when a new <see cref="HistoryEntry"/> is created.
    /// </summary>
    public class HistoryEntryUpdate {

        public HistoryEntryUpdate(Data.Model.HistoryEntry entry) {
            this.Entry = new HistoryEntry(entry);
            this.InterfaceId = entry.InterfaceId;
        }

        /// <summary>
        /// The history entry.
        /// </summary>
        [Required]
        public HistoryEntry Entry { get; }
        
        /// <summary>
        /// Interface to which this entry relates.
        /// </summary>
        [Required]
        public Guid InterfaceId { get; }

    }
}