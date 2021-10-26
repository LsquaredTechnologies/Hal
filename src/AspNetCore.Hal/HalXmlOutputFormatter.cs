using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Lsquared.Foundation.Net.Hal;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Formatters;

namespace Lsquared.AspNetCore.Hal
{
    /// <summary>
    /// The HAL XML output formatter.
    /// </summary>
    public sealed class HalXmlOutputFormatter : TextOutputFormatter, IApiResponseTypeMetadataProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HalXmlOutputFormatter"/> class.
        /// </summary>
        /// <param name="halXmlMediaTypes">The allowed HAL JSON media types.</param>
        public HalXmlOutputFormatter(params string[] halXmlMediaTypes)
        {
            SupportedEncodings.Add(Encoding.UTF8);

            foreach (var mediaType in halXmlMediaTypes)
                SupportedMediaTypes.Add(mediaType);

            if (SupportedMediaTypes.Count == 0)
                SupportedMediaTypes.Add("application/hal+xml");
        }

        /// <inheritdoc/>
        protected override bool CanWriteType(Type type) =>
            type.IsAssignableTo(typeof(HalResourceBase));

        /// <inheritdoc/>
        public override async Task WriteResponseBodyAsync(OutputFormatterWriteContext context, Encoding selectedEncoding)
        {
            var xmlFormatter = new XmlSerializerOutputFormatter();
            try
            {
                await xmlFormatter.WriteAsync(context);
            }
            catch (Exception e)
            {
                _ = e;
            }
        }

        /// <inheritdoc/>
        IReadOnlyList<string> IApiResponseTypeMetadataProvider.GetSupportedContentTypes(string contentType, Type objectType) =>
            SupportedMediaTypes;
    }
}
