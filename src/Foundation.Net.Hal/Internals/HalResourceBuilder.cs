using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Lsquared.Foundation.Net.Hal.Internals
{
    /// <summary>
    /// Represents the default HAL resource builder.
    /// </summary>
    public sealed class HalResourceBuilder : IHalResourceBuilder
    {
        /// <inheritdoc/>
        public IHalResourceBuilder AddLink(string rel, Action<IHalLinkBuilder> factory)
        {
            _linkSteps.Add((_) =>
            {
                var builder = new HalLinkBuilder(rel);
                factory(builder);
                return builder.Build();
            });
            return this;
        }

        /// <inheritdoc/>
        public IHalResourceBuilder AddSelfLink(string href)
        {
            _linkSteps.Add((state) =>
            {
                var builder = new HalLinkBuilder("self");
                //factory?.Invoke(builder);
                //if (!builder.HasValues)
                //{
                //    var href = _urlHelper.RouteUrl(null, null, _urlHelper.ActionContext.HttpContext.Request.Scheme);
                builder.WithValue(href);
                //}
                return builder.Build();
            });
            return this;
        }

        /// <inheritdoc/>
        public HalResource Build()
        {
            var state = CreateState();
            return new HalResource(_stateType)
            {
                ExtensionData = state,
                Links = new(_linkSteps.Select(s => s(state))),
                Embedded = _embeddedResourceSteps.Select(s => s()).ToList()
            };
        }

        /// <inheritdoc/>
        public IHalResourceBuilder WithEmbeddedResources<T>(string name, IEnumerable<T> items, Action<T, IHalEmbeddedResourceBuilder> factory)
        {
            _embeddedResourceSteps.Add(() =>
            {
                var builder = new HalEmbeddedBuilder(name);
                foreach (var item in items)
                    factory(item, builder);
                return builder.Build();
            });

            return this;
        }

        /// <inheritdoc/>
        public IHalResourceBuilder WithState(object state)
        {
            _stateType = state.GetType();
            _stateSteps.Add(state);
            return this;
        }

        private IDictionary<string, object?> CreateState()
        {
            Dictionary<string, object?> result = new(10);
            foreach (var step in _stateSteps)
            {
                var props = step.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
                foreach (var prop in props)
                    result.TryAdd(prop.Name, prop.GetValue(step));
            }
            return result;
        }

        private readonly List<object> _stateSteps = new(10);
        private readonly List<Func<object, HalLink>> _linkSteps = new(10);
        private readonly List<Func<HalEmbedded>> _embeddedResourceSteps = new(10);
        private Type? _stateType;
    }
}
