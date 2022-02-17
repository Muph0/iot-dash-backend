using System;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IotDash.JsonConverters {
    class DateTimeConverter : JsonConverter<DateTime> {
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
            Debug.Assert(typeToConvert == typeof(DateTime));
            string? utcString = reader.GetString();
            Debug.Assert(utcString != null);
            
            var date = DateTime.Parse(utcString, null, System.Globalization.DateTimeStyles.AssumeUniversal);
            date = date.ToUniversalTime();
            return date;
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options) {
            if (value.Kind == DateTimeKind.Unspecified) {
                value = new DateTime(value.Ticks, DateTimeKind.Utc);
            }

            value = value.ToUniversalTime();
            string utcDate = value.ToString("yyyy-MM-ddTHH:mm:ss.sssZ");
            writer.WriteStringValue(utcDate);
        }
    }
}