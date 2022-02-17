using System;
using System.ComponentModel.DataAnnotations;

namespace IotDash.Contracts.V1 {

    /// <summary>
    /// REST model of a history query. Represents a time interval with point density information.
    /// </summary>
    public class HistoryRequest {

        /// <summary>
        /// Start of the time interval.
        /// </summary>
        [Required]
        public DateTime FromUTC { get; set; }

        /// <summary>
        /// End of the time interval.
        /// </summary>
        [Required]
        public DateTime ToUTC { get; set; }

        /// <summary>
        /// Maximum number of points in the interval. If exceeded,
        /// poins will be recalculated to uniformly cover the interval.
        /// </summary>
        public int? PointCount { get; set; }
    }
}