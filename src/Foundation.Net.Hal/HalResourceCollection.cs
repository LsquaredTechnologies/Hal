using System.Collections;
using System.Collections.Generic;

namespace Lsquared.Foundation.Net.Hal
{
    /// <summary>
    /// Represents an HAL resource collection.
    /// </summary>
    public sealed class HalResourceCollection : IReadOnlyList<HalResourceBase>
    {
        /// <inheritdoc/>
        public int Count =>
            _items.Count;

        /// <inheritdoc/>
        public HalResourceBase this[int index] =>
            _items[index];

        /// <summary>
        /// Adds the specified resource.
        /// </summary>
        /// <param name="resource">The resource.</param>
        public void Add(HalResourceBase resource) =>
            _items.Add(resource);

        /// <summary>
        /// Removes the specified resource.
        /// </summary>
        /// <param name="resource">The resource.</param>
        public void Remove(HalResourceBase resource) =>
            _items.Remove(resource);

        /// <inheritdoc/>
        public IEnumerator<HalResourceBase> GetEnumerator() =>
            _items.GetEnumerator();

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() =>
            _items.GetEnumerator();

        private readonly List<HalResourceBase> _items = new(10);
    }
}
