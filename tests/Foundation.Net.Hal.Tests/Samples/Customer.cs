namespace Lsquared.Foundation.Net.Hal.Tests.Samples
{
    [HalLink("self", "/customers/{id}")]
    public sealed class Customer
    {
        public int Id { get; init; }

        public string? Name { get; init; }
    }
}
