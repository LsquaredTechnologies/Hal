using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Lsquared.Foundation.Net.Hal.Serialization
{
    /// <summary>
    /// Represents the HAL resource JSON converter.
    /// </summary>
    internal sealed class HalResourceJsonConverter<T> : JsonConverter<T>
        where T : HalResourceBase
    {
        /// <inheritdoc/>
        public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
            throw new NotImplementedException();

        /// <inheritdoc/>
        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            var links = value.Links;
            if (links is not null)
            {
                writer.WritePropertyName("_links");
                writer.WriteStartObject();

                foreach (HalLink link in links)
                    JsonSerializer.Serialize(writer, link, options);

                writer.WriteEndObject();
            }

            var embedded = value.Embedded;
            if (embedded is not null && embedded.Count > 0)
            {
                writer.WritePropertyName("_embedded");
                writer.WriteStartObject();
                foreach (HalEmbedded embed in embedded)
                {
                    writer.WritePropertyName(embed!.Name);
                    writer.WriteStartArray();
                    foreach (var resource in embed!.Resources)
                        JsonSerializer.Serialize(writer, resource, options);
                    writer.WriteEndArray();
                }
                writer.WriteEndObject();
            }

            var state = value.State;
            if (state is not null)
            {
                var properties = state.GetType().GetProperties();
                foreach (var property in properties)
                {
                    var propName = property.Name;
                    var propValue = property.GetValue(state);

                    writer.WritePropertyName(options.PropertyNamingPolicy is null ? propName : options.PropertyNamingPolicy.ConvertName(propName));
                    JsonSerializer.Serialize(writer, propValue, options);
                }
            }

            var extensionData = value.ExtensionData;
            foreach (var (propName, propValue) in extensionData)
            {
                writer.WritePropertyName(options.PropertyNamingPolicy is null ? propName : options.PropertyNamingPolicy.ConvertName(propName));
                JsonSerializer.Serialize(writer, propValue, options);
            }

            writer.WriteEndObject();
        }
    }
}
