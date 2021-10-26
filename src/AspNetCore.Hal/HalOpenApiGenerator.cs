using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

namespace Lsquared.AspNetCore.Hal
{
    public sealed class HalOpenApiGenerator
    {
        public HalOpenApiGenerator(IOptions<JsonOptions> options) =>
            _options = options.Value;

        public OpenApiSchema GenerateSchema(Type type, bool nullable = false) => type switch
        {
            _ when type.IsPrimitive => GeneratePrimitiveSchema(type, nullable),
            _ when type == typeof(decimal) => GeneratePrimitiveSchema(type, nullable),
            _ when type == typeof(Guid) => GeneratePrimitiveSchema(type, nullable),
            _ when type == typeof(TimeSpan) => GeneratePrimitiveSchema(type, nullable),
            _ when type == typeof(DateTime) => GeneratePrimitiveSchema(type, nullable),
            _ when type == typeof(DateTimeOffset) => GeneratePrimitiveSchema(type, nullable),
            _ when type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>) => GenerateSchema(type.GenericTypeArguments[0], true),
            _ when type == typeof(string) => GeneratePrimitiveSchema(type, true),
            _ when type == typeof(char) => GeneratePrimitiveSchema(type, nullable),
            _ when type == typeof(Uri) => GeneratePrimitiveSchema(type, true),
            _ when type == typeof(UriBuilder) => GeneratePrimitiveSchema(type, true),
            _ when type.IsArray => GenerateArray(type.GetElementType()!),
            _ when type.IsGenericType && type.GetGenericTypeDefinition().FullName!.StartsWith("System.Tuple") => GenerateTuple(type, true),
            _ when type.IsGenericType && type.GetGenericTypeDefinition().FullName!.StartsWith("System.ValueTuple") => GenerateTuple(type, false),
            _ when type.IsGenericType && typeof(IEnumerable).IsAssignableFrom(type) => GenerateArray(type.GenericTypeArguments[0]),
            _ when typeof(IEnumerable).IsAssignableFrom(type) => throw new NotSupportedException(),
            _ => GenerateObject(type)
        };

        private OpenApiSchema GenerateTuple(Type tupleType, bool nullable) =>
            throw new NotSupportedException();

        private OpenApiSchema GenerateObject(Type type)
        {
            OpenApiSchema schema = new() { Type = "object" };
            OpenApiSchema halEmbeddedSchema = new();

            // Add _links property
            var classLinksAttrs = type.GetCustomAttributes<HalLinksAttribute>();
            schema.Properties.Add("_links", CreateInlineLinksSchema(classLinksAttrs));


            var properties = type.GetProperties();
            foreach (var property in properties)
            {
                var propertyName = _options.JsonSerializerOptions.PropertyNamingPolicy?.ConvertName(property.Name) ?? property.Name;

                var propertySchema = GenerateSchema(property.PropertyType);
                if (propertySchema.Type == "array")
                    halEmbeddedSchema.Properties.Add(propertyName, propertySchema);
                else
                    schema.Properties.Add(propertyName, propertySchema);
            }

            if (halEmbeddedSchema.Properties.Count > 0)
                schema.Properties.Add("_embedded", halEmbeddedSchema);

            return schema;
        }

        private OpenApiSchema GenerateArray(Type itemType) =>
            new()
            {
                Type = "array",
                Items = GenerateSchema(itemType),
                Nullable = true,
            };

        private OpenApiSchema GeneratePrimitiveSchema(Type type, bool nullable)
        {
            if (!_primitiveFactories.TryGetValue(type, out var factory))
                throw new NotSupportedException($"Cannot generate schema for type: {type.Name}");

            var schema = factory();
            if (nullable)
                schema.Nullable = true;
            return schema;
        }

        private OpenApiSchema CreateInlineLinksSchema(IEnumerable<HalLinksAttribute> linksAttrs)
        {
            var halLinkValueSchema = new OpenApiSchema();
            halLinkValueSchema.Properties.Add("deprecation", new OpenApiSchema() { Type = "string", Nullable = true });
            halLinkValueSchema.Properties.Add("href", new OpenApiSchema() { Type = "string" });
            halLinkValueSchema.Properties.Add("hrefLang", new OpenApiSchema() { Type = "string", Nullable = true });
            halLinkValueSchema.Properties.Add("name", new OpenApiSchema() { Type = "string", Nullable = true });
            halLinkValueSchema.Properties.Add("profile", new OpenApiSchema() { Type = "string", Nullable = true });
            halLinkValueSchema.Properties.Add("templated", new OpenApiSchema() { Type = "boolean", Default = new OpenApiBoolean(false) });
            halLinkValueSchema.Properties.Add("title", new OpenApiSchema() { Type = "string", Nullable = true });
            halLinkValueSchema.Properties.Add("type", new OpenApiSchema() { Type = "string", Nullable = true });
            halLinkValueSchema.AdditionalPropertiesAllowed = true;

            ////halLinkSchema.Xml = new() { Name = "_links" };

            var halLinkSchema = new OpenApiSchema()
            {
                OneOf = new List<OpenApiSchema>() {
                        new OpenApiSchema()
                        {
                            Type = "array",
                            Items = halLinkValueSchema
                        },
                        halLinkValueSchema
                    }
            };

            var halLinksSchema = new OpenApiSchema
            {
                Type = "object",
                AdditionalPropertiesAllowed = true,
                Xml = new()
                {
                    Name = "link",
                    Wrapped = false
                }
            };

            OpenApiObject example = new();

            foreach (var linkAttr in linksAttrs)
            {
                halLinksSchema.Properties.Add(linkAttr.Rel, halLinkSchema);
                example.Add(linkAttr.Rel, new OpenApiObject
                {
                    ["href"] = new OpenApiString($"string"),
                });
                foreach (var rel in linkAttr.AdditionalRels)
                {
                    halLinksSchema.Properties.Add(rel, halLinkSchema);
                    example.Add(rel, new OpenApiObject
                    {
                        ["href"] = new OpenApiString($"string"),
                    });
                }
            }

            halLinksSchema.Example = example;

            return halLinksSchema;
        }

        private readonly JsonOptions _options;
        private readonly Dictionary<Type, Func<OpenApiSchema>> _primitiveFactories = new()
        {
            [typeof(string)] = () => new OpenApiSchema { Type = "string", Default = new OpenApiString("string") },
            [typeof(char)] = () => new OpenApiSchema { Type = "string", MaxLength = 1, MinLength = 1, Default = new OpenApiString("A") },
            [typeof(bool)] = () => new OpenApiSchema { Type = "boolean", Default = new OpenApiBoolean(false) },
            [typeof(byte)] = () => new OpenApiSchema { Type = "integer", Format = "int32", Minimum = byte.MinValue, Maximum = byte.MaxValue, Default = new OpenApiInteger(0) },
            [typeof(short)] = () => new OpenApiSchema { Type = "integer", Format = "int32", Minimum = short.MinValue, Maximum = short.MaxValue, Default = new OpenApiInteger(0) },
            [typeof(int)] = () => new OpenApiSchema { Type = "integer", Format = "int32", Default = new OpenApiInteger(0) },
            [typeof(long)] = () => new OpenApiSchema { Type = "integer", Format = "int64", Default = new OpenApiInteger(0) },
            [typeof(sbyte)] = () => new OpenApiSchema { Type = "integer", Format = "int32", Minimum = sbyte.MinValue, Maximum = sbyte.MaxValue, Default = new OpenApiInteger(0) },
            [typeof(ushort)] = () => new OpenApiSchema { Type = "integer", Format = "int32", Minimum = 0, Maximum = ushort.MaxValue, Default = new OpenApiInteger(0) },
            [typeof(uint)] = () => new OpenApiSchema { Type = "integer", Format = "int32", Minimum = 0, Default = new OpenApiInteger(0) },
            [typeof(ulong)] = () => new OpenApiSchema { Type = "integer", Format = "int64", Minimum = 0, Default = new OpenApiInteger(0) },
            [typeof(float)] = () => new OpenApiSchema { Type = "number", Format = "float", Default = new OpenApiInteger(0) },
            [typeof(double)] = () => new OpenApiSchema { Type = "number", Format = "double", Default = new OpenApiInteger(0) },
            [typeof(decimal)] = () => new OpenApiSchema { Type = "number", Format = "double", Default = new OpenApiInteger(0) },
            [typeof(Guid)] = () => new OpenApiSchema { Type = "string", Format = "uuid", Example = new OpenApiString(Guid.NewGuid().ToString()) },
            [typeof(TimeSpan)] = () => new OpenApiSchema { Type = "string", Format = "time", Example = new OpenApiString(TimeSpan.Zero.ToString()) },
            [typeof(DateTime)] = () => new OpenApiSchema { Type = "string", Format = "date-time", Example = new OpenApiString(DateTime.UtcNow.ToString()) },
            [typeof(DateTimeOffset)] = () => new OpenApiSchema { Type = "string", Format = "date-time", Example = new OpenApiString(DateTimeOffset.UtcNow.ToString()) },
            [typeof(Uri)] = () => new OpenApiSchema { Type = "string", Format = "uri", Example = new OpenApiString("/") },
            [typeof(UriBuilder)] = () => new OpenApiSchema { Type = "string", Format = "uri", Example = new OpenApiString("/") },
        };
    }
}
