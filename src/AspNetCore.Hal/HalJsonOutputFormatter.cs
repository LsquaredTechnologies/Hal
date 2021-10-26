using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Lsquared.Foundation.Net.Hal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Lsquared.AspNetCore.Hal
{
    /// <summary>
    /// The HAL JSON output formatter.
    /// </summary>
    public sealed class HalJsonOutputFormatter : TextOutputFormatter, IApiResponseTypeMetadataProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HalJsonOutputFormatter"/> class.
        /// </summary>
        /// <param name="halJsonMediaTypes">The allowed HAL JSON media types.</param>
        public HalJsonOutputFormatter(params string[] halJsonMediaTypes)
        {
            SupportedEncodings.Add(Encoding.UTF8);

            foreach (var mediaType in halJsonMediaTypes)
                SupportedMediaTypes.Add(mediaType);

            if (SupportedMediaTypes.Count == 0)
                SupportedMediaTypes.Add("application/hal+json");
        }

        /// <inheritdoc/>
        protected override bool CanWriteType(Type type) =>
            type.IsAssignableTo(typeof(HalResourceBase));

        /// <inheritdoc/>
        public override Task WriteResponseBodyAsync(OutputFormatterWriteContext context, Encoding selectedEncoding)
        {
            var jsonOptions = context.HttpContext.RequestServices.GetRequiredService<IOptions<JsonOptions>>();
            var jsonFormatter = new SystemTextJsonOutputFormatter(jsonOptions.Value.JsonSerializerOptions);
            return jsonFormatter.WriteAsync(context);
        }

        /// <inheritdoc/>
        IReadOnlyList<string> IApiResponseTypeMetadataProvider.GetSupportedContentTypes(string contentType, Type objectType) =>
            SupportedMediaTypes;
    }
}
