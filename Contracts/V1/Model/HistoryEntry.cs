using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace IotDash.Contracts.V1.Model {

    /// <summary>
    /// Represents a measurement event in time.
    /// </summary>
    public class HistoryEntry {

        private Data.Model.HistoryEntry entry;

        public HistoryEntry(Data.Model.HistoryEntry entry) {
            this.entry = entry;
        }


        [Required]
        public DateTime TimeUTC => entry.WhenUTC;

        [Required]
        public double? Min => entry.Min;

        [Required]
        public double? Max => entry.Max;

        [Required]
        public double? Average => entry.Average;
    }
}