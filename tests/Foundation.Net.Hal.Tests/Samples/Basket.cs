namespace Lsquared.Foundation.Net.Hal.Tests.Samples
{
    [HalLink("self", "/baskets/{id}")]
    public sealed class Basket
    {
        public int Id { get; init; }

        public int NumberOfItems { get; init; }
    }
}
