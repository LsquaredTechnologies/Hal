using System.Collections.ObjectModel;

namespace Lsquared.Foundation.Net.Hal.Tests.Samples
{
    [HalLink("self", "/orders")]
    [HalLink("next", "/orders{?page}")]
    [HalLink("find", "/orders/{id}", Templated = true)]
    [HalEmbedded("orders", typeof(Order))]
    public sealed class Orders : Collection<Order>
    {
        public int ShippedToday { get; init; }
    }
}
