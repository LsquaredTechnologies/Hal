namespace Lsquared.Foundation.Net.Hal.Tests.Samples
{
    [HalLink("self", "/simple/{id}")]
    public sealed class SimpleResource
    {
        public int Id { get; init; }

        public string? Title { get; init; }

        public string? Author { get; init; }
    }
}
