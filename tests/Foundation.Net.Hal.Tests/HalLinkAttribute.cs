using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Lsquared
{
    /// <summary>
    /// The HAL link attribute.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class HalLinkAttribute : Attribute
    {
        public string Rel { get; }

        public UriTemplate Href { get; }

        public bool Templated { get; init; }

        public string? Name { get; init; }

        public string? Type { get; init; }

        public string? Deprecation { get; init; }

        public string? Profile { get; init; }

        public string? Title { get; init; }

        public string? HrefLang { get; init; }

        public HalLinkAttribute(string rel, string href) =>
            (Rel, Href) = (rel, new(href));

        public HalLinkAttribute(string rel, UriTemplate href) =>
            (Rel, Href) = (rel, href);
    }
    /// <summary>
    /// The HAL embedded attribute.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
    public sealed class HalEmbeddedAttribute : Attribute
    {
        [NotNull] public string? Name { get; }

        public Type? ClrType { get; init; }

        public bool SingleElement { get; init; }

        public HalEmbeddedAttribute(string name) : this(name, null)
        {
        }

        public HalEmbeddedAttribute(string name, Type? type)
        {
            Name = name;
            ClrType = type;
        }
    }
    /// <summary>
    /// Represents an HAL resource.
    /// </summary>
    [JsonConverter(typeof(HalResourceJsonConverter))]
    [XmlRoot("resource")]
    public sealed class HalResourceDescription : IXmlSerializable
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
        public object? State { get; }

        /// <summary>
        /// Gets the extension data.
        /// </summary>
        [JsonExtensionData]
        public IDictionary<string, object?> ExtensionData { get; init; } = new Dictionary<string, object?>(10);


        /// <summary>
        /// Prevents a default instance of the <see cref="HalResource"/> class from being created.
        /// </summary>
        private HalResourceDescription()
        {
            _stateType = typeof(object);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HalResource"/> class.
        /// </summary>
        /// <param name="stateType">The state type.</param>
        public HalResourceDescription(object? state, Type? stateType)
        {
            State = state;
            _stateType = stateType ?? state?.GetType() ?? throw new ArgumentException("Invalid state", nameof(stateType));

            Links = CreateLinks();
            Embedded = CreateEmbedded();
        }

        public static HalResourceDescription Create<T>(T resource) =>
            new HalResourceDescription(resource, typeof(T));

        /// <inheritdoc/>
        public Type? GetStateType() =>
            _stateType;

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

                var stateType = State?.GetType();
                if (stateType is not null && !stateType.Name.Contains("<"))
                {
                    selfLinkRelName = stateType.Name;
                    selfLinkRelName = char.ToLowerInvariant(selfLinkRelName[0]) + selfLinkRelName[1..];
                    if (selfLinkRelName.EndsWith("ViewModel", StringComparison.OrdinalIgnoreCase))
                        selfLinkRelName = selfLinkRelName[0..^9];
                }

                writer.WriteAttributeString("rel", selfLinkRelName);

                if (Links.TryGetValue("self", out var selfLinkValues))
                    writer.WriteAttributeString("href", selfLinkValues[0].Href);

                // TODO handle curries...

                foreach (var (rel, values) in Links)
                    if (rel is not "self" && values is not null)
                    {
                        writer.WriteStartElement("link");

                        writer.WriteAttributeString("rel", rel);

                        if (values.Count > 0)
                        {
                            var value = values[0];
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
                var properties = _stateType.GetProperties();
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

        private HalLinkCollection CreateLinks()
        {
            var attrs = _stateType.GetCustomAttributes<HalLinkAttribute>(true).ToArray();
            HalLinkCollection links = new();
            foreach (var attr in attrs)
            {
                var href = attr.Templated ? attr.Href.ToString() : attr.Href.Expand(State);
                HalLinkValue value = new(href, attr.Name, attr.Type)
                {
                    Deprecation = attr.Deprecation,
                    HrefLang = attr.HrefLang,
                    Profile = attr.Profile,
                    Templated = attr.Templated,
                    Title = attr.Title,
                };
                links.Add(attr.Rel, new() { value });
            }
            return links;
        }

        private IReadOnlyCollection<HalEmbedded> CreateEmbedded()
        {
            List<HalEmbedded> embedded = new(10);

            // In case State is a collection...
            var attrs = _stateType.GetCustomAttributes<HalEmbeddedAttribute>(true).ToArray();
            foreach (var attr in attrs)
            {
                if (State is not IEnumerable en) continue;
                List<HalResourceDescription> resources = new(10);
                foreach (var item in en)
                    if (item is not null)
                        resources.Add(new(item, item.GetType()));

                if (resources.Count > 0)
                    embedded.Add(new(attr.Name, new(resources)));
            }

            // In case State has collection or children resources properties...
            var properties = _stateType.GetProperties();
            foreach (var property in properties)
            {
                var attr = property.GetCustomAttribute<HalEmbeddedAttribute>(true);
                if (attr is null) continue;
                var statePropertyValue = property.GetValue(State);
                if (statePropertyValue is IEnumerable en)
                {
                    List<HalResourceDescription> resources = new(10);
                    foreach (var item in en)
                        if (item is not null)
                            resources.Add(new(item, item.GetType()));

                    if (resources.Count > 0)
                        embedded.Add(new(attr.Name, new(resources)));
                }
                else
                    embedded.Add(new(attr.Name, new(new[] { new HalResourceDescription(statePropertyValue, property.PropertyType) }), attr.SingleElement));
            }

            return new List<HalEmbedded>(embedded);
        }

        private readonly Type _stateType;
    }
    /// <summary>
    /// Represents an HAL link collection.
    /// </summary>
    public sealed class HalLinkCollection : IReadOnlyDictionary<string, HalLinkValueCollection>
    {
        /// <inheritdoc/>
        public int Count =>
            _items.Count;

        /// <inheritdoc/>
        public HalLinkValueCollection this[string rel] =>
            _items[rel];

        IEnumerable<string> IReadOnlyDictionary<string, HalLinkValueCollection>.Keys =>
            _items.Keys;

        IEnumerable<HalLinkValueCollection> IReadOnlyDictionary<string, HalLinkValueCollection>.Values =>
            _items.Values;

        /// <summary>
        /// Initializes a new instance of the <see cref="HalLinkCollection"/> class.
        /// </summary>
        public HalLinkCollection() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="HalLinkCollection"/> class.
        /// </summary>
        /// <param name="collection">The collection.</param>
        public HalLinkCollection(IEnumerable<KeyValuePair<string, HalLinkValueCollection>> collection) =>
            _items = collection.ToDictionary(entry => entry.Key, entry => entry.Value);

        /// <summary>
        /// Initializes a new instance of the <see cref="HalLinkCollection"/> class.
        /// </summary>
        /// <param name="collection">The collection.</param>
        public HalLinkCollection(IDictionary<string, HalLinkValueCollection> collection) =>
            _items = new(collection);

        /// <summary>
        /// Adds the specified link.
        /// </summary>
        /// <param name="link">The resource.</param>
        /// <param name="rel"></param>
        /// <param name="values"></param>
        public void Add(string rel, HalLinkValueCollection values) =>
            _items.Add(rel, values);

        public bool ContainsKey(string rel) =>
            _items.ContainsKey(rel);

        /// <summary>
        /// Removes the link with the specified "rel".
        /// </summary>
        /// <param name="rel">The link.</param>
        public void Remove(string rel) =>
            _items.Remove(rel);

        public bool TryGetValue(string rel, [NotNullWhen(true)] out HalLinkValueCollection? values) =>
            _items.TryGetValue(rel, out values);

        /// <inheritdoc/>
        public IEnumerator<KeyValuePair<string, HalLinkValueCollection>> GetEnumerator() =>
            _items.GetEnumerator();

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() =>
            _items.GetEnumerator();

        private readonly Dictionary<string, HalLinkValueCollection> _items = new(10);
    }
    /////// <summary>
    /////// Represents an HAL link.
    /////// </summary>
    ////[Serializable]
    ////[JsonConverter(typeof(HalLinkJsonConverter))]
    ////public sealed class HalLink : IXmlSerializable
    ////{
    ////    /// <summary>
    ////    /// Gets the relation name.
    ////    /// </summary>
    ////    //[XmlAttribute(AttributeName = "rel")]
    ////    public string Rel { get; init; }

    ////    /// <summary>
    ////    /// Gets the values.
    ////    /// </summary>
    ////    public HalLinkValueCollection Values { get; init; } = new();

    ////    /// <summary>
    ////    /// Initializes a new instance of the <see cref="HalLink"/> class.
    ////    /// </summary>
    ////    /// <param name="rel">The rel.</param>
    ////    /// <param name="href">The href.</param>
    ////    public HalLink(string rel, string href)
    ////    {
    ////        Rel = rel;
    ////        Values.Add(new(href));
    ////    }

    ////    /// <summary>
    ////    /// Initializes a new instance of the <see cref="HalLink"/> class.
    ////    /// </summary>
    ////    /// <param name="rel">The rel.</param>
    ////    /// <param name="values">The values.</param>
    ////    public HalLink(string rel, HalLinkValueCollection values)
    ////    {
    ////        Rel = rel;
    ////        Values = values;
    ////    }

    ////    /// <summary>
    ////    /// Initializes a new instance of the <see cref="HalLink"/> class.
    ////    /// </summary>
    ////    /// <param name="rel">The rel.</param>
    ////    /// <param name="value">The value.</param>
    ////    public HalLink(string rel, HalLinkValue value)
    ////    {
    ////        Rel = rel;
    ////        Values = new() { value };
    ////    }

    ////    /// <inheritdoc/>
    ////    XmlSchema? IXmlSerializable.GetSchema() =>
    ////        null;

    ////    /// <inheritdoc/>
    ////    void IXmlSerializable.ReadXml(XmlReader reader) =>
    ////        throw new NotSupportedException();

    ////    /// <inheritdoc/>
    ////    void IXmlSerializable.WriteXml(XmlWriter writer)
    ////    {
    ////        writer.WriteAttributeString("rel", Rel);

    ////        if (Values.Count > 0)
    ////        {
    ////            var value = Values[0];
    ////            writer.WriteAttributeString("href", value.Href);

    ////            if (!string.IsNullOrWhiteSpace(value.Name))
    ////                writer.WriteAttributeString("name", value.Name);

    ////            if (value.Templated)
    ////                writer.WriteAttributeString("templated", "true");

    ////            if (!string.IsNullOrWhiteSpace(value.Type))
    ////                writer.WriteAttributeString("type", value.Type);

    ////            if (!string.IsNullOrWhiteSpace(value.Title))
    ////                writer.WriteAttributeString("title", value.Title);

    ////            if (!string.IsNullOrWhiteSpace(value.Profile))
    ////                writer.WriteAttributeString("profile", value.Profile);

    ////            if (!string.IsNullOrWhiteSpace(value.HrefLang))
    ////                writer.WriteAttributeString("hrefLang", value.HrefLang);

    ////            if (!string.IsNullOrWhiteSpace(value.Deprecation))
    ////                writer.WriteAttributeString("deprecation", value.Deprecation);

    ////            foreach (var (k, v) in value.AdditionalProperties)
    ////                if (v is not null)
    ////                    writer.WriteAttributeString(k, v.ToString());
    ////        }
    ////    }
    ////}
    /// <summary>
    /// Represents an HAL embedded resources.
    /// </summary>
    public sealed class HalEmbedded
    {
        /// <summary>
        /// Gets the name.
        /// </summary>
        public string Name { get; init; }

        /// <summary>
        /// Gets the resources.
        /// </summary>
        public HalResourceCollection Resources { get; init; }

        public bool SingleElement { get; init; }

        /// <summary>
        /// Initializes a new instance of the <see cref="HalEmbedded"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="resources">The resources.</param>
        public HalEmbedded(string name, HalResourceCollection resources, bool singleElement = false)
        {
            Name = name;
            Resources = resources;
            SingleElement = singleElement;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HalEmbedded"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public HalEmbedded(string name) : this(name, new())
        {
        }
    }
    /// <summary>
    /// Represents an HAL resource collection.
    /// </summary>
    public sealed class HalResourceCollection : IReadOnlyList<HalResourceDescription>
    {
        /// <inheritdoc/>
        public int Count =>
            _items.Count;

        /// <inheritdoc/>
        public HalResourceDescription this[int index] =>
            _items[index];

        public HalResourceCollection() { }

        public HalResourceCollection(IEnumerable<HalResourceDescription> resources) =>
            _items.AddRange(resources);

        /// <summary>
        /// Adds the specified resource.
        /// </summary>
        /// <param name="resource">The resource.</param>
        public void Add(HalResourceDescription resource) =>
            _items.Add(resource);

        /// <summary>
        /// Removes the specified resource.
        /// </summary>
        /// <param name="resource">The resource.</param>
        public void Remove(HalResourceDescription resource) =>
            _items.Remove(resource);

        /// <inheritdoc/>
        public IEnumerator<HalResourceDescription> GetEnumerator() =>
            _items.GetEnumerator();

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() =>
            _items.GetEnumerator();

        private readonly List<HalResourceDescription> _items = new(10);
    }
    /// <summary>
    /// Represnets an HAL link value.
    /// </summary>
    public sealed class HalLinkValue
    {
        /// <summary>
        /// Gets the name.
        /// </summary>
        //[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Name { get; init; }

        /// <summary>
        /// Gets the href.
        /// </summary>
        [NotNull] public string? Href { get; init; }

        /// <summary>
        /// Gets a value indicating whether the href is templated.
        /// </summary>
        //[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public bool Templated { get; init; }

        /// <summary>
        /// Gets the type.
        /// </summary>
        //[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Type { get; init; }

        /// <summary>
        /// Gets the deprecation information.
        /// </summary>
        //[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Deprecation { get; init; }

        /// <summary>
        /// Gets the profile.
        /// </summary>
        //[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Profile { get; init; }

        /// <summary>
        /// Gets the title.
        /// </summary>
        //[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Title { get; init; }

        /// <summary>
        /// Gets the href lang.
        /// </summary>
        //[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? HrefLang { get; init; }

        /// <summary>
        /// Gets the additional properties.
        /// </summary>
        [JsonExtensionData]
        public IDictionary<string, object> AdditionalProperties { get; init; } = new Dictionary<string, object>(10);

        /// <summary>
        /// Initializes a new instance of the <see cref="HalLinkValue"/> class.
        /// </summary>
        /// <param name="href">The href.</param>
        public HalLinkValue(string href) : this(href, null, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HalLinkValue"/> class.
        /// </summary>
        /// <param name="href">The href.</param>
        /// <param name="name">The name.</param>
        public HalLinkValue(string href, string? name) : this(href, name, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HalLinkValue"/> class.
        /// </summary>
        /// <param name="href">The href.</param>
        /// <param name="name">The name.</param>
        /// <param name="type">The type.</param>
        public HalLinkValue(string href, string? name, string? type)
        {
            Href = href;
            Name = name;
            Type = type;
        }
    }
    /// <summary>
    /// Represents an HAL link value collection.
    /// </summary>
    public sealed class HalLinkValueCollection : IReadOnlyList<HalLinkValue>
    {
        /// <inheritdoc/>
        public int Count =>
            _items.Count;

        /// <inheritdoc/>
        public HalLinkValue this[int index] =>
            _items[index];

        /// <summary>
        /// Adds the specified value.
        /// </summary>
        /// <param name="value">The resource.</param>
        public void Add(HalLinkValue value) =>
            _items.Add(value);

        /// <summary>
        /// Removes the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        public void Remove(HalLinkValue value) =>
            _items.Remove(value);

        /// <inheritdoc/>
        public IEnumerator<HalLinkValue> GetEnumerator() =>
            _items.GetEnumerator();

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() =>
            _items.GetEnumerator();

        private readonly List<HalLinkValue> _items = new(10);
    }
    /////// <summary>
    /////// Represents the HAL link JSON converter.
    /////// </summary>
    ////internal sealed class HalLinkJsonConverter : JsonConverter<HalLink>
    ////{
    ////    /// <inheritdoc/>
    ////    public override HalLink? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
    ////        throw new NotImplementedException();

    ////    /// <inheritdoc/>
    ////    public override void Write(Utf8JsonWriter writer, HalLink value, JsonSerializerOptions options)
    ////    {
    ////        writer.WritePropertyName(value.Rel);

    ////        var values = value.Values;
    ////        if (values is null)
    ////        {
    ////            writer.WriteStartArray();
    ////            writer.WriteEndArray();
    ////        }
    ////        else
    ////        {
    ////            if (values.Count == 1)
    ////            {
    ////                JsonSerializer.Serialize(writer, values[0], options);
    ////            }
    ////            else
    ////            {
    ////                writer.WriteStartArray();
    ////                foreach (var item in values)
    ////                    JsonSerializer.Serialize(writer, item, options);
    ////                writer.WriteEndArray();
    ////            }
    ////        }
    ////    }
    ////}
    /// <summary>
    /// Represents the HAL resource JSON converter.
    /// </summary>
    internal sealed class HalResourceJsonConverter : JsonConverter<HalResourceDescription>
    {
        /// <inheritdoc/>
        public override HalResourceDescription? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
            throw new NotImplementedException();

        /// <inheritdoc/>
        public override void Write(Utf8JsonWriter writer, HalResourceDescription value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            var links = value.Links;
            if (links is not null)
            {
                writer.WritePropertyName("_links");
                writer.WriteStartObject();

                foreach (var (rel, values) in links)
                {
                    writer.WritePropertyName(rel);
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
                            foreach (HalLinkValue item in values)
                                JsonSerializer.Serialize(writer, item, options);
                            writer.WriteEndArray();
                        }
                    }
                }

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
