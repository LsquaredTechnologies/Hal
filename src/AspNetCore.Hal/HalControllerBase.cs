using System;
using Lsquared.Foundation.Net.Hal;
using Lsquared.Foundation.Net.Hal.Internals;
using Microsoft.AspNetCore.Mvc;

namespace Lsquared.AspNetCore.Hal
{
    /// <summary>
    /// Represents the HAL controller base.
    /// </summary>
    public abstract class HalControllerBase : ControllerBase
    {
        /// <summary>
        /// Returns a response with 200-OK status and an HAL resource body.
        /// </summary>
        /// <param name="factory">The factory.</param>
        /// <param name="contentType">The content type.</param>
        /// <returns>An OkObjectResult.</returns>
        public HalResourceResult<T> HalResource<T>(Action<IHalResourceBuilder> factory, int statusCode = 200)
        {
            var builder = new HalResourceBuilder();
            factory(builder);
            HalResource resource = builder.Build();
            return HalResource<T>(resource, statusCode);
        }

        /// <summary>
        /// Hals the resource.
        /// </summary>
        /// <param name="resource">The resource.</param>
        /// <param name="statusCode">The status code.</param>
        /// <param name="contentType">The content type.</param>
        /// <returns>A HalResourceResult.</returns>
        public HalResourceResult<T> HalResource<T>(HalResource resource, int statusCode = 200) =>
            new(resource, statusCode);
    }
}
