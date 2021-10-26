namespace Lsquared.Foundation.Net.Hal
{
    /// <summary>
    /// Represents an HAL embedded resources.
    /// </summary>
    public sealed class HalEmbedded
    {
        /// <summary>
        /// Gets the name.
        /// </summary>
        public string Name { get; init; }

        /// <summary>
        /// Gets the resources.
        /// </summary>
        public HalResourceCollection Resources { get; init; }

        /// <summary>
        /// Initializes a new instance of the <see cref="HalEmbedded"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="resources">The resources.</param>
        public HalEmbedded(string name, HalResourceCollection resources)
        {
            Name = name;
            Resources = resources;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HalEmbedded"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public HalEmbedded(string name) : this(name, new())
        {
        }
    }
}
