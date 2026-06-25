using System.Text.Json;
using System.Text.Json.Serialization;

namespace IDEPython.DTOs.Http
{
    public class DateTimeConverter : JsonConverter<DateTime>
    {
        private readonly string[] _formats = {
        "yyyy-MM-dd HH:mm:ss",  // this is the MySQL DATETIME format
        "yyyy-MM-dd",            // if it is just a date without time
        "yyyy-MM-ddTHH:mm:ss"   // ISO 8601
    };

        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string? value = reader.GetString();
            foreach (var format in _formats)
            {
                if (DateTime.TryParseExact(value, format,
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None, out DateTime result))
                    return result;
            }
            throw new JsonException($"Formato de fecha no reconocido: {value}");
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
            => writer.WriteStringValue(value.ToString("yyyy-MM-dd HH:mm:ss"));
    }
}
