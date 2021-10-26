using System.Collections.Generic;

namespace Lsquared.Foundation.Net.Hal
{
    /// <summary>
    /// Provides contract to build an <see cref="HalLink">HAL link</see> in fluent manner.
    /// </summary>
    public interface IHalLinkBuilder : IFluent
    {
        /// <summary>
        /// Builds an HAL link.
        /// </summary>
        /// <returns>A HalLink.</returns>
        HalLink Build();

        /// <summary>
        /// Adds the specified href.
        /// </summary>
        /// <param name="href">The href.</param>
        /// <returns>The same instance which is used to chain methods.</returns>
        IHalLinkBuilder WithValue(string href);

        /// <summary>
        /// Adds the specified href and name.
        /// </summary>
        /// <param name="href">The href.</param>
        /// <param name="name">The name.</param>
        /// <returns>The same instance which is used to chain methods.</returns>
        IHalLinkBuilder WithValue(string href, string name);

        /// <summary>
        /// Adds the specified href and name and type.
        /// </summary>
        /// <param name="href">The href.</param>
        /// <param name="name">The name.</param>
        /// <param name="type">The type.</param>
        /// <returns>The same instance which is used to chain methods.</returns>
        IHalLinkBuilder WithValue(string href, string name, string type);

        /// <summary>
        /// Adds the specified href and name and type and others.
        /// </summary>
        /// <param name="href">The href.</param>
        /// <param name="templated">If <paramref name="href"/> is templated, set to <c>true</c>.</param>
        /// <param name="name">The name.</param>
        /// <param name="type">The type.</param>
        /// <param name="deprecation">The deprecation.</param>
        /// <param name="profile">The profile.</param>
        /// <param name="title">The title.</param>
        /// <param name="hreflang">The href language.</param>
        /// <param name="additionalProperties">The additional properties.</param>
        /// <returns>The same instance which is used to chain methods.</returns>
        IHalLinkBuilder WithValue(
            string href, bool templated = false, string? name = null, string? type = null,
            string? deprecation = null, string? profile = null, string? title = null,
            string? hreflang = null, Dictionary<string, object>? additionalProperties = null);
    }
}
