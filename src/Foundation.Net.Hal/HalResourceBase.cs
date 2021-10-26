using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Lsquared.Foundation.Net.Hal.Serialization;

namespace Lsquared.Foundation.Net.Hal
{
    /// <summary>
    /// The HAL resource base.
    /// </summary>
    [Serializable]
    [JsonConverter(typeof(HalResourceJsonConverterFactory))]
    public abstract class HalResourceBase : IXmlSerializable
    {
        /// <summary>
        /// Gets the links.
        /// </summary>
        //[XmlArray, XmlArrayItem("link")]
        public HalLinkCollection? Links { get; init; }

        /// <summary>
        /// Gets the embedded resources.
        /// </summary>
        //[XmlArray, XmlArrayItem("resource")]
        public IReadOnlyCollection<HalEmbedded>? Embedded { get; init; }

        /// <summary>
        /// Gets the state.
        /// </summary>
        public object? State { get; init; }

        /// <summary>
        /// Gets the extension data.
        /// </summary>
        [JsonExtensionData]
        public IDictionary<string, object?> ExtensionData { get; init; } = new Dictionary<string, object?>(10);

        /// <inheritdoc/>
        public abstract Type? GetStateType();

        /// <inheritdoc/>
        XmlSchema? IXmlSerializable.GetSchema() =>
            null;

        /// <inheritdoc/>
        void IXmlSerializable.ReadXml(XmlReader reader) =>
            throw new NotSupportedException();

        /// <inheritdoc/>
        void IXmlSerializable.WriteXml(XmlWriter writer)
        {
            if (Links is not null)
            {
                var selfLinkRelName = "self";

                var stateType = GetStateType();
                if (stateType is not null && !stateType.Name.Contains("<"))
                {
                    selfLinkRelName = stateType.Name;
                    selfLinkRelName = char.ToLowerInvariant(selfLinkRelName[0]) + selfLinkRelName[1..];
                    if (selfLinkRelName.EndsWith("ViewModel", StringComparison.OrdinalIgnoreCase))
                        selfLinkRelName = selfLinkRelName[0..^9];
                }

                writer.WriteAttributeString("rel", selfLinkRelName);

                var selfLink = Links.FirstOrDefault(x => string.Equals(x.Rel, "self", StringComparison.OrdinalIgnoreCase));
                if (selfLink is not null)
                    writer.WriteAttributeString("href", selfLink.Values[0].Href);

                // TODO handle curries...

                foreach (var link in Links.Except(new[] { selfLink }.ToArray()))
                    if (link is not null)
                    {
                        writer.WriteStartElement("link");
                        ((IXmlSerializable)link).WriteXml(writer);
                        writer.WriteEndElement();
                    }
            }

            if (Embedded is not null)
                foreach (var embed in Embedded)
                    foreach (var resource in embed.Resources)
                    {
                        writer.WriteStartElement("resource");
                        ((IXmlSerializable)resource).WriteXml(writer);
                        writer.WriteEndElement();
                    }

            if (State is not null)
            {
                var properties = State.GetType().GetProperties();
                foreach (var property in properties)
                {
                    var propName = property.Name;
                    var propValue = property.GetValue(State);

                    var key = char.ToLowerInvariant(propName[0]) + propName[1..]; // Simple camelCase from .Net naming rule!
                    switch (propValue)
                    {
                        case null: break;
                        case string s: writer.WriteElementString(key, s); break;
                        default: writer.WriteElementString(key, propValue.ToString()); break;
                    }
                }
            }

            if (ExtensionData is not null)
                foreach (var (k, v) in ExtensionData)
                {
                    var key = char.ToLowerInvariant(k[0]) + k[1..]; // Simple camelCase from .Net naming rule!
                    switch (v)
                    {
                        case null: break;
                        case string s: writer.WriteElementString(key, s); break;
                        default: writer.WriteElementString(key, v.ToString()); break;
                    }
                }
        }
    }
}
