using System;
using System.Text.Json.Serialization;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Lsquared.Foundation.Net.Hal.Serialization;

namespace Lsquared.Foundation.Net.Hal
{
    /// <summary>
    /// Represents an HAL link.
    /// </summary>
    [Serializable]
    [JsonConverter(typeof(HalLinkJsonConverter))]
    public sealed class HalLink : IXmlSerializable
    {
        /// <summary>
        /// Gets the relation name.
        /// </summary>
        //[XmlAttribute(AttributeName = "rel")]
        public string Rel { get; init; }

        /// <summary>
        /// Gets the values.
        /// </summary>
        public HalLinkValueCollection Values { get; init; } = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="HalLink"/> class.
        /// </summary>
        /// <param name="rel">The rel.</param>
        /// <param name="href">The href.</param>
        public HalLink(string rel, string href)
        {
            Rel = rel;
            Values.Add(new(href));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HalLink"/> class.
        /// </summary>
        /// <param name="rel">The rel.</param>
        /// <param name="values">The values.</param>
        public HalLink(string rel, HalLinkValueCollection values)
        {
            Rel = rel;
            Values = values;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HalLink"/> class.
        /// </summary>
        /// <param name="rel">The rel.</param>
        /// <param name="value">The value.</param>
        public HalLink(string rel, HalLinkValue value)
        {
            Rel = rel;
            Values = new() { value };
        }

        /// <inheritdoc/>
        XmlSchema? IXmlSerializable.GetSchema() =>
            null;

        /// <inheritdoc/>
        void IXmlSerializable.ReadXml(XmlReader reader) =>
            throw new NotSupportedException();

        /// <inheritdoc/>
        void IXmlSerializable.WriteXml(XmlWriter writer)
        {
            writer.WriteAttributeString("rel", Rel);

            if (Values.Count > 0)
            {
                var value = Values[0];
                writer.WriteAttributeString("href", value.Href);

                if (!string.IsNullOrWhiteSpace(value.Name))
                    writer.WriteAttributeString("name", value.Name);

                if (value.Templated)
                    writer.WriteAttributeString("templated", "true");

                if (!string.IsNullOrWhiteSpace(value.Type))
                    writer.WriteAttributeString("type", value.Type);

                if (!string.IsNullOrWhiteSpace(value.Title))
                    writer.WriteAttributeString("title", value.Title);

                if (!string.IsNullOrWhiteSpace(value.Profile))
                    writer.WriteAttributeString("profile", value.Profile);

                if (!string.IsNullOrWhiteSpace(value.HrefLang))
                    writer.WriteAttributeString("hrefLang", value.HrefLang);

                if (!string.IsNullOrWhiteSpace(value.Deprecation))
                    writer.WriteAttributeString("deprecation", value.Deprecation);

                foreach (var (k, v) in value.AdditionalProperties)
                    if (v is not null)
                        writer.WriteAttributeString(k, v.ToString());
            }
        }
    }
}
