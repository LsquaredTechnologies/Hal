using System;
using System.Collections.Generic;

namespace Lsquared.Foundation.Net.Hal.Internals
{
    /// <summary>
    /// Represents the default HAL embedded builder.
    /// </summary>
    public sealed class HalEmbeddedBuilder : IHalEmbeddedResourceBuilder
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HalEmbeddedBuilder"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public HalEmbeddedBuilder(string name) =>
            _name = name;

        /// <inheritdoc/>
        public IHalEmbeddedResourceBuilder Add(Action<IHalResourceBuilder> factory)
        {
            _resourceFactories.Add(factory);
            return this;
        }

        /// <inheritdoc/>
        public HalEmbedded Build() =>
            new(_name, BuildResources());

        private HalResourceCollection BuildResources()
        {
            HalResourceCollection result = new();
            foreach (var factory in _resourceFactories)
            {
                var builder = new HalResourceBuilder();
                factory(builder);
                result.Add(builder.Build());
            }
            return result;
        }

        private readonly string _name;
        private readonly List<Action<IHalResourceBuilder>> _resourceFactories = new(10);
    }
}
