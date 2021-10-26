using System;
using System.Text.Json.Serialization;
using System.Xml.Serialization;
using Lsquared.Foundation.Net.Hal.Serialization;

namespace Lsquared.Foundation.Net.Hal
{
    /// <summary>
    /// Represents an HAL resource.
    /// </summary>
    //[XmlRoot("resource")]
    [JsonConverter(typeof(HalResourceJsonConverterFactory))]
    public sealed class HalResource<T> : HalResourceBase
    {
        /// <inheritdoc/>
        public new T? State
        {
            get => (T?)base.State;
            init => base.State = value;
        }

        /// <inheritdoc/>
        public override Type? GetStateType() =>
            typeof(T);
    }
}
