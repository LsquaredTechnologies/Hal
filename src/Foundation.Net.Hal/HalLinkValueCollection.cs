using System.Collections;
using System.Collections.Generic;

namespace Lsquared.Foundation.Net.Hal
{
    /// <summary>
    /// Represents an HAL link value collection.
    /// </summary>
    public sealed class HalLinkValueCollection : IReadOnlyList<HalLinkValue>
    {
        /// <inheritdoc/>
        public int Count =>
            _items.Count;

        /// <inheritdoc/>
        public HalLinkValue this[int index] =>
            _items[index];

        /// <summary>
        /// Adds the specified value.
        /// </summary>
        /// <param name="value">The resource.</param>
        public void Add(HalLinkValue value) =>
            _items.Add(value);

        /// <summary>
        /// Removes the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        public void Remove(HalLinkValue value) =>
            _items.Remove(value);

        /// <inheritdoc/>
        public IEnumerator<HalLinkValue> GetEnumerator() =>
            _items.GetEnumerator();

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() =>
            _items.GetEnumerator();

        private readonly List<HalLinkValue> _items = new(10);
    }
}
