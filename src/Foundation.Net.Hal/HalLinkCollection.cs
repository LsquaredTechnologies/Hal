using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Lsquared.Foundation.Net.Hal
{
    /// <summary>
    /// Represents an HAL link collection.
    /// </summary>
    public sealed class HalLinkCollection : IReadOnlyList<HalLink>
    {
        /// <inheritdoc/>
        public int Count =>
            _items.Count;

        /// <inheritdoc/>
        public HalLink this[int index] =>
            _items[index];

        /// <summary>
        /// Initializes a new instance of the <see cref="HalLinkCollection"/> class.
        /// </summary>
        public HalLinkCollection() =>
            _items = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="HalLinkCollection"/> class.
        /// </summary>
        /// <param name="collection">The collection.</param>
        public HalLinkCollection(IEnumerable<HalLink> collection) =>
            _items = collection.ToList();

        /// <summary>
        /// Initializes a new instance of the <see cref="HalLinkCollection"/> class.
        /// </summary>
        /// <param name="collection">The collection.</param>
        public HalLinkCollection(ICollection<HalLink> collection) =>
            _items = new(collection);

        /// <summary>
        /// Adds the specified link.
        /// </summary>
        /// <param name="link">The resource.</param>
        public void Add(HalLink link) =>
            _items.Add(link);

        /// <summary>
        /// Removes the specified link.
        /// </summary>
        /// <param name="link">The link.</param>
        public void Remove(HalLink link) =>
            _items.Remove(link);

        /// <inheritdoc/>
        public IEnumerator<HalLink> GetEnumerator() =>
            _items.GetEnumerator();

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() =>
            _items.GetEnumerator();

        private readonly List<HalLink> _items = new(10);
    }
}
