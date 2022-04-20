using System;
using System.ComponentModel.DataAnnotations;

namespace IotDash.Contracts.V1.Model {
    public class HistoryEntryUpdate {

        public HistoryEntryUpdate(Data.Model.HistoryEntry entry) {
            this.Entry = new HistoryEntry(entry);
            this.InterfaceId = entry.InterfaceId;
        }

        [Required]
        public HistoryEntry Entry { get; }
        [Required]
        public Guid InterfaceId { get; }

    }
}