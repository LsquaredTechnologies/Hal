using System;
using System.Text.Json.Serialization;
using System.Xml.Serialization;
using Lsquared.Foundation.Net.Hal.Internals;
using Lsquared.Foundation.Net.Hal.Serialization;

namespace Lsquared.Foundation.Net.Hal
{
    /// <summary>
    /// Represents an HAL resource.
    /// </summary>
    [JsonConverter(typeof(HalResourceJsonConverterFactory))]
    [XmlRoot("resource")]
    public sealed class HalResource : HalResourceBase
    {
        /// <summary>
        /// Prevents a default instance of the <see cref="HalResource"/> class from being created.
        /// </summary>
        private HalResource()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HalResource"/> class.
        /// </summary>
        /// <param name="stateType">The state type.</param>
        public HalResource(Type? stateType)
        {
            _stateType = stateType;
        }

        //public HalResource<T> Create<T>(T item)
        //{
        //    var builder = new HalResourceBuilder();
        //    builder.WithState(item).
        //    new HalResource<T>()
        //}

        /// <inheritdoc/>
        public override Type? GetStateType() =>
            _stateType;

        private readonly Type? _stateType;
    }
}
