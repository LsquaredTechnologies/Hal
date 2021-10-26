using Lsquared.Foundation.Net.Hal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace Lsquared.AspNetCore.Hal
{
    /// <summary>
    /// The HAL resource result.
    /// </summary>
    public sealed class HalResourceResult<T> : IConvertToActionResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HalResourceResult"/> class.
        /// </summary>
        /// <param name="resource">The resource.</param>
        /// <param name="statusCode">The status code.</param>
        public HalResourceResult(HalResource resource, int statusCode)
        {
            _resource = resource;
            _statusCode = statusCode;
        }

        /// <inheritdoc/>
        IActionResult IConvertToActionResult.Convert()
        {
            var ar = new OkObjectResult(_resource) { StatusCode = _statusCode };
            return ar;
        }

        private readonly HalResource _resource;
        private readonly int _statusCode;
    }
}
