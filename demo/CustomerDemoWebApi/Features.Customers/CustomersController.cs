using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Lsquared.AspNetCore.Hal;
using Lsquared.Foundation.Net.Hal;
using Microsoft.AspNetCore.Mvc;

namespace Lsquared.Features.Customers
{
    /// <summary>
    /// The customers controller.
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class CustomersController : HalControllerBase
    {
        /// <summary>
        /// Gets all the customers.
        /// </summary>
        /// <param name="page">The page.</param>
        /// <param name="limit">The limit.</param>
        /// <returns>A HalResourceResult.</returns>
        [HttpGet]
        [HalLinks("self", "prev", "next", "first", "last", "find")]
        [HalEmbedded("customers", typeof(CustomerViewModel))]
        public HalResourceResult<Collection<CustomerViewModel>> GetAll(int page, int limit)
        {
            List<CustomerViewModel> items = new()
            {
                new() { Id = "1", Name = "Lionel", BirthDate = new DateTime(1980, 08, 27), },
                new() { Id = "2", Name = "Tülin", BirthDate = new DateTime(1982, 09, 13), },
            };

            return HalResource<Collection<CustomerViewModel>>((builder) => builder
                .AddSelfLink(Url.ActionLink(action: nameof(GetAll), values: new { page, limit }, protocol: Request.Scheme))
                .AddLink("find", (builder) => builder.WithValue(Url.ActionLink(action: nameof(GetOne), values: new { id = "{?id}" }, protocol: Request.Scheme)))
                .WithState(new { totalCount = 2 })
                .WithEmbeddedResources("customers", items, (item, builder) => builder
                    .Add((builder) => builder
                        .AddSelfLink(Url.ActionLink(action: nameof(GetOne), values: new { id = item.Id }, protocol: Request.Scheme))
                        .WithState(item))));
        }

        /// <summary>
        /// Gets one customer.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns>A HalResourceResult.</returns>
        [HttpGet("{id}")]
        public HalResourceResult<CustomerViewModel> GetOne(string id)
        {
            CustomerViewModel item = new() { Id = "1", Name = "Lionel", BirthDate = new DateTime(1980, 08, 27) };

            return HalResource<CustomerViewModel>((IHalResourceBuilder builder) => builder
               .AddSelfLink(Url.ActionLink(action: nameof(GetOne), values: new { id }, protocol: Request.Scheme))
               .WithState(item));
        }
    }
}
