using System;
using System.Collections.Generic;

namespace Lsquared.Foundation.Net.Hal
{
    /// <summary>
    /// Provides contract to build an <see cref="HalResource">HAL resource</see> in fluent manner.
    /// </summary>
    public interface IHalResourceBuilder : IFluent
    {
        /// <summary>
        /// Adds a new link with the specified rel and configures it.
        /// </summary>
        /// <param name="rel">The rel.</param>
        /// <param name="factory">The factory.</param>
        /// <returns>The same instance which is used to chain methods.</returns>
        IHalResourceBuilder AddLink(string rel, Action<IHalLinkBuilder> factory);

        /// <summary>
        /// Adds the self link.
        /// </summary>
        /// <param name="href">The href.</param>
        /// <returns>The same instance which is used to chain methods.</returns>
        IHalResourceBuilder AddSelfLink(string href);

        /// <summary>
        /// Builds an HAL resource.
        /// </summary>
        /// <returns>A HalResource.</returns>
        HalResource Build();

        /// <summary>
        /// Withs the state.
        /// </summary>
        /// <param name="state">The state.</param>
        /// <returns>The same instance which is used to chain methods.</returns>
        IHalResourceBuilder WithState(object state);

        /// <summary>
        /// Withs the embedded resources.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="items">The items.</param>
        /// <param name="factory">The factory.</param>
        /// <returns>The same instance which is used to chain methods.</returns>
        IHalResourceBuilder WithEmbeddedResources<T>(string name, IEnumerable<T> items, Action<T, IHalEmbeddedResourceBuilder> factory);
    }
}
