using System;

namespace Lsquared.Foundation.Net.Hal
{
    /// <summary>
    /// Provides contract to build an <see cref="HalEmbedded">HAL embedded resource</see> in fluent manner.
    /// </summary>
    public interface IHalEmbeddedResourceBuilder : IFluent
    {
        /// <summary>
        /// Adds the specified factory.
        /// </summary>
        /// <param name="factory">The factory.</param>
        /// <returns>An IHalEmbeddedResourceBuilder.</returns>
        IHalEmbeddedResourceBuilder Add(Action<IHalResourceBuilder> factory);

        /// <summary>
        /// Builds an HAL resource.
        /// </summary>
        /// <returns>A HalEmbedded.</returns>
        HalEmbedded Build();
    }
}
