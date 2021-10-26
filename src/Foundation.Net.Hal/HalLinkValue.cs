using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Lsquared.Foundation.Net.Hal
{
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
        [NotNull] public string Href { get; init; }

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
}
