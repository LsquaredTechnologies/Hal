using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Lsquared.Foundation.Net.Hal.Serialization
{
    /// <summary>
    /// The HAL resource JSON converter factory.
    /// </summary>
    internal sealed class HalResourceJsonConverterFactory : JsonConverterFactory
    {
        /// <inheritdoc/>
        public override bool CanConvert(Type typeToConvert) =>
            typeToConvert.IsAssignableTo(typeof(HalResourceBase));

        /// <inheritdoc/>
        public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            var type = typeof(HalResourceJsonConverter<>).MakeGenericType(typeToConvert);
            return (JsonConverter?)Activator.CreateInstance(type);
        }
    }
}
