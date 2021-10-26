using System;

namespace Lsquared.Foundation.Net.Hal
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public sealed class HalLinkAttribute : Attribute
    {
        public string Rel { get; }

        public string Href { get; }

        public bool Templated { get; init; }

        public string? Name { get; init; }

        public string? Type { get; init; }

        public string? Deprecation { get; init; }

        public string? Profile { get; init; }

        public string? Title { get; init; }

        public string? HrefLang { get; init; }

        public HalLinkAttribute(string rel, string href)
        {
            Rel = rel;
            Href = href;
        }
    }
}
