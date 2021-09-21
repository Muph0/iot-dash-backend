using Microsoft.Extensions.Configuration;
using System;

namespace IotDash.Settings {

    public class HistorySettings : Settings<HistorySettings> {
        public TimeSpan? Keep1Second { get; set; } = TimeSpan.FromSeconds(300);
        public TimeSpan? Keep10Second { get; set; } = TimeSpan.FromHours(1);
        public TimeSpan? Keep1Minute { get; set; } = TimeSpan.FromHours(6);
        public TimeSpan? Keep10Minute { get; set; } = TimeSpan.FromDays(3);
        public TimeSpan? Keep1Hour { get; set; } = TimeSpan.FromDays(370);
        public TimeSpan? Keep6Hour { get; set; } = null;
    }

}