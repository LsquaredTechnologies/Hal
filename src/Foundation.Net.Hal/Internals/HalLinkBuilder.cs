using System;
using System.Collections.Generic;

namespace Lsquared.Foundation.Net.Hal.Internals
{
    /// <summary>
    /// Represents the default HAL link builder.
    /// </summary>
    public sealed class HalLinkBuilder : IHalLinkBuilder
    {
        /// <summary>
        /// Gets a value indicating whether the instance has values.
        /// </summary>
        public bool HasValues =>
            _values.Count > 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="HalLinkBuilder"/> class.
        /// </summary>
        /// <param name="rel">The rel.</param>
        public HalLinkBuilder(string rel) =>
            _rel = rel;

        /// <inheritdoc/>
        public HalLink Build() =>
            new(_rel, BuildValues());

        /// <inheritdoc/>
        public IHalLinkBuilder WithValue(string href) =>
            WithValue(href, false);

        /// <inheritdoc/>
        public IHalLinkBuilder WithValue(string href, string name) =>
            WithValue(href, false, name);

        /// <inheritdoc/>
        public IHalLinkBuilder WithValue(string href, string name, string type) =>
            WithValue(href, false, name, type);

        /// <inheritdoc/>
        public IHalLinkBuilder WithValue(
            string href, bool templated = false, string? name = null, string? type = null,
            string? deprecation = null, string? profile = null, string? title = null,
            string? hreflang = null, Dictionary<string, object>? additionalProperties = null)
        {
            _values[href] = new(href, name, type)
            {
                Templated = templated,
                Deprecation = deprecation,
                Profile = profile,
                Title = title,
                HrefLang = hreflang,
                AdditionalProperties = additionalProperties ?? new(0),
            };
            return this;
        }

        private HalLinkValueCollection BuildValues()
        {
            HalLinkValueCollection result = new();
            foreach ((string _, HalLinkValue value) in _values)
                result.Add(value);
            return result;
        }

        private readonly string _rel;
        private readonly Dictionary<string, HalLinkValue> _values = new(10);
    }
}
