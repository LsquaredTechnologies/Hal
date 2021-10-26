using System;

namespace Lsquared.Foundation.Net.Hal
{
    /// <summary>
    /// The HAL embedded attribute.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = true)]
    public sealed class HalEmbeddedAttribute : Attribute
    {
        public string? Name { get; }

        public Type? ClrType { get; init; }

        public HalEmbeddedAttribute() : this(null, null)
        {
        }

        public HalEmbeddedAttribute(string name) : this(name, null)
        {
        }

        public HalEmbeddedAttribute(string? name, Type? type)
        {
            Name = name;
            ClrType = type;
        }
    }
}
