using System;

namespace IotDash.Contracts.V1 {
    public class HistoryRequest {
        public DateTime From { get; set; }
        public DateTime To { get; set; }

        public int PointCount { get; set; }
    }
}