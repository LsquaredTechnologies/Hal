using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Lsquared.AspNetCore.Hal;

namespace Lsquared.Features.Customers
{
    /// <summary>
    /// The customer view model.
    /// </summary>
    [HalLinks("self")]
    public sealed class CustomerViewModel
    {
        /// <summary>
        /// Gets the id.
        /// </summary>
        [NotNull] public string? Id { get; init; }

        /// <summary>
        /// Gets the name.
        /// </summary>
        [NotNull] public string Name { get; init; } = string.Empty;

        /// <summary>
        /// Gets the birth date.
        /// </summary>
        [DataType(DataType.Date)]
        public DateTime BirthDate { get; init; }
    }
}
