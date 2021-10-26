namespace Lsquared.Foundation.Net.Hal.Tests.Samples
{
    [HalLink("self", "/orders/{id}")]
    [HalLink("basket", "/baskets/{id}")]
    [HalLink("customer", "/customers/{id}")]
    public sealed class Order
    {
        public int Id { get; init; }

        [HalEmbedded("basket", SingleElement = true)]
        public Basket? Basket { get; init; }

        [HalEmbedded("customer", typeof(Customer), SingleElement = true)]
        public Customer? Customer { get; init; }

        public string? Currency { get; init; }

        public string? Status { get; init; }

        public decimal Total { get; init; }
    }
}
