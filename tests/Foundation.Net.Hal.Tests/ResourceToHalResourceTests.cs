using Lsquared.Foundation.Net.Hal.Tests.Samples;
using Xunit;

namespace Lsquared.Foundation.Net.Hal.Tests
{
    public sealed class ResourceToHalResourceTests
    {
        [Fact]
        public void With_Default_SimpleResource()
        {
            var simple = new SimpleResource();
            var halResource = HalResourceDescription.Create(simple);
            Assert.Equal("/simple/0", halResource.Links!["self"][0].Href);
            Assert.Equal(0, ((dynamic)halResource.State!).Id);
            Assert.Null(((dynamic)halResource.State!).Title);
            Assert.Null(((dynamic)halResource.State!).Author);
        }

        [Fact]
        public void With_Assigned_SimpleResource()
        {
            var simple = new SimpleResource { Id = 1234, Title = "Foundation", Author = "Asimov" };
            var halResource = HalResourceDescription.Create(simple);
            Assert.Equal("/simple/1234", halResource.Links!["self"][0].Href);
            Assert.Equal(1234, ((dynamic)halResource.State!).Id);
            Assert.Equal("Foundation", ((dynamic)halResource.State!).Title);
            Assert.Equal("Asimov", ((dynamic)halResource.State!).Author);
        }
    }
}
