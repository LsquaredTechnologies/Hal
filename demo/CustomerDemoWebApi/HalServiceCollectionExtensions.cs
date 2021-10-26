using Lsquared.AspNetCore.Hal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace CustomerDemoWebApi
{
    public static class HalServiceCollectionExtensions
    {

        public static IServiceCollection AddHalFormatters(this IServiceCollection services, bool replaceOthers = false)
        {
            services.Configure<MvcOptions>((options) =>
            {
                if (replaceOthers)
                    options.OutputFormatters.Clear();

                options.OutputFormatters.Insert(0, new HalJsonOutputFormatter());
                options.OutputFormatters.Insert(1, new HalXmlOutputFormatter());
            });

            return services;
        }

        public static IServiceCollection AddHal(this IServiceCollection services, bool replaceOthers = false)
        {
            // TODO add naming policy service for XML (usage of JSON naming policy for JSON!)
            return services.AddHalFormatters(replaceOthers);
        }
    }
}
