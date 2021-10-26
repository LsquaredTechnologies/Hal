using System.ComponentModel;

namespace Lsquared.Foundation.Net.Hal
{
    /// <summary>
    /// Provides contract to hide inherited members which are not useful for fluent interface.
    /// </summary>
    public interface IFluent
    {
        /// <inheritdoc/>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        bool Equals(object? obj);

        /// <inheritdoc/>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        int GetHashCode();

        /// <inheritdoc/>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        string? ToString();
    }
}
