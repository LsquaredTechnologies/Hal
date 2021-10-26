using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Lsquared.Foundation.Net.Hal.Serialization
{
    /// <summary>
    /// Represents the HAL link JSON converter.
    /// </summary>
    internal sealed class HalLinkJsonConverter : JsonConverter<HalLink>
    {
        /// <inheritdoc/>
        public override HalLink? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
            throw new NotImplementedException();

        /// <inheritdoc/>
        public override void Write(Utf8JsonWriter writer, HalLink value, JsonSerializerOptions options)
        {
            writer.WritePropertyName(value.Rel);

            var values = value.Values;
            if (values is null)
            {
                writer.WriteStartArray();
                writer.WriteEndArray();
            }
            else
            {
                if (values.Count == 1)
                {
                    JsonSerializer.Serialize(writer, values[0], options);
                }
                else
                {
                    writer.WriteStartArray();
                    foreach (var item in values)
                        JsonSerializer.Serialize(writer, item, options);
                    writer.WriteEndArray();
                }
            }
        }
    }
}
