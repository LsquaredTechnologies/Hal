//using System.Text.Json;
//using Lsquared.AspNetCore.Hal;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.Extensions.Options;
//using Microsoft.OpenApi.Any;
//using Microsoft.OpenApi.Models;
//using Xunit;

//namespace Lsquared.Foundation.Net.Hal.Tests
//{
//    public sealed class GenerateOpenApiTests
//    {
//        public HalOpenApiGenerator Sut { get; }

//        public GenerateOpenApiTests()
//        {
//            JsonOptions options = new();
//            options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
//            Sut = new HalOpenApiGenerator(Options.Create(options));
//        }

//        [Fact]
//        public void GenerateSchema_ForByteType()
//        {
//            OpenApiSchema schema = Sut.GenerateSchema(typeof(byte));
//            Assert.Equal("integer", schema.Type);
//            Assert.Equal("int32", schema.Format);
//            Assert.Equal(0, schema.Minimum);
//            Assert.Equal(255, schema.Maximum);
//            Assert.Equal(0, ((OpenApiInteger)schema.Default).Value);
//        }

//        [Fact]
//        public void GenerateSchema_ForInt32Type()
//        {
//            OpenApiSchema schema = Sut.GenerateSchema(typeof(int));
//            Assert.Equal("integer", schema.Type);
//            Assert.Equal("int32", schema.Format);
//            Assert.Equal(0, ((OpenApiInteger)schema.Default).Value);
//        }

//        [Fact]
//        public void GenerateSchema_ForStringType()
//        {
//            OpenApiSchema schema = Sut.GenerateSchema(typeof(string));
//            Assert.Equal("string", schema.Type);
//        }

//        [Fact]
//        public void GenerateSchema_ForArrayOfStringType()
//        {
//            OpenApiSchema schema = Sut.GenerateSchema(typeof(string[]));
//            Assert.Equal("array", schema.Type);
//            Assert.Equal("string", schema.Items.Type);
//        }

//        [Fact]
//        public void GenerateSchema_ForArrayOfSingleType()
//        {
//            OpenApiSchema schema = Sut.GenerateSchema(typeof(float[]));
//            Assert.Equal("array", schema.Type);
//            Assert.Equal("number", schema.Items.Type);
//            Assert.Equal("float", schema.Items.Format);
//        }

//        [Fact]
//        public void GenerateSchema_ForResourceType()
//        {
//            OpenApiSchema schema = Sut.GenerateSchema(typeof(SimpleResource));
//            Assert.Equal("object", schema.Type);
//            Assert.Equal(4, schema.Properties.Count);
//            Assert.Equal("object", schema.Properties["_links"].Type);
//            Assert.Equal("integer", schema.Properties["id"].Type);
//            Assert.Equal("string", schema.Properties["title"].Type);
//            Assert.Equal("string", schema.Properties["author"].Type);
//        }

//        [Fact]
//        public void Foo()
//        {
//            OpenApiSchema schema = Sut.GenerateSchema(typeof(Orders));
//            /*
//            <resource rel="self" href="/orders">
//                <link rel="next" href="/orders?page=2"/>
//                <link rel="find" href="/orders{/id}" templated="true"/>
//                <resource rel="order" href="/orders/523">
//                    <link rel="basket" href="/baskets/56"/>
//                    <link rel="customer" href="/customer/873"/>
//                    <currency>USD</currency>
//                    <status>shipped</status>
//                    <total>10.20</total>
//                    <resource rel="customer" href="/customer/873">
//                        <name>Dupond</name>
//                    </resource>
//                </resource>
//            </resource>
//            */
//        }
//    }

//    ////public sealed class HalOpenApiGenerator
//    ////{
//    ////    public HalOpenApiGenerator(IOptions<JsonOptions> options) =>
//    ////        _options = options.Value;

//    ////    //public OpenApiSchema GenerateCollectionResourceSchema()
//    ////    //{
//    ////    //}

//    ////    public OpenApiSchema GenerateResourceSchema(Type type, MethodInfo? methodInfo, OperationFilterContext context)
//    ////    {
//    ////        var halResourceSchema = new OpenApiSchema();

//    ////        // Add _links property
//    ////        var classLinksAttrs = type.GetCustomAttributes<HalLinksAttribute>();
//    ////        var methodLinksAttrs = methodInfo?.GetCustomAttributes<HalLinksAttribute>() ?? Enumerable.Empty<HalLinksAttribute>();
//    ////        halResourceSchema.Properties.Add("_links", CreateInlineLinksSchema(Enumerable.Union(methodLinksAttrs, classLinksAttrs)));

//    ////        // Add "native" properties
//    ////        var properties = type.GetProperties();
//    ////        foreach (var property in properties)
//    ////        {
//    ////            var propertyName = _options.JsonSerializerOptions.PropertyNamingPolicy?.ConvertName(property.Name) ?? property.Name;
//    ////            var isCollectionProperty =
//    ////                property.PropertyType != typeof(string) &&
//    ////                property.PropertyType.IsGenericType &&
//    ////                typeof(IEnumerable).IsAssignableFrom(property.PropertyType);
//    ////            if (isCollectionProperty)
//    ////            {
//    ////                var collectionItemType = property.PropertyType.GenericTypeArguments[0];
//    ////                var propertySchema = CreateCollection context.SchemaGenerator.GenerateSchema(collectionItemType, context.SchemaRepository);
//    ////                halEmbeddedSchema.Properties.Add(propertyName, propertySchema);
//    ////            }
//    ////            else if (!property.PropertyType.IsByRef || property.PropertyType == typeof(string))
//    ////            {
//    ////                var propertySchema = context.SchemaGenerator.GenerateSchema(property.PropertyType, context.SchemaRepository);
//    ////                propertySchema.ApplyCustomAttributes(property.GetCustomAttributes());
//    ////                propertySchema.Nullable = property.PropertyType.IsReferenceOrNullableType();
//    ////                ////if (property.GetCustomAttribute<System.Diagnostics.CodeAnalysis.NotNullAttribute>() is not null)
//    ////                ////    propertySchema.Nullable = false;
//    ////                halResourceSchema.Properties.Add(propertyName, propertySchema);
//    ////            }
//    ////        }

//    ////        return halResourceSchema;
//    ////    }

//    ////    private OpenApiSchema CreateInlineLinksSchema(IEnumerable<HalLinksAttribute> linksAttrs)
//    ////    {
//    ////        var halLinkValueSchema = new OpenApiSchema();
//    ////        halLinkValueSchema.Properties.Add("deprecation", new OpenApiSchema() { Type = "string", Nullable = true });
//    ////        halLinkValueSchema.Properties.Add("href", new OpenApiSchema() { Type = "string" });
//    ////        halLinkValueSchema.Properties.Add("hrefLang", new OpenApiSchema() { Type = "string", Nullable = true });
//    ////        halLinkValueSchema.Properties.Add("name", new OpenApiSchema() { Type = "string", Nullable = true });
//    ////        halLinkValueSchema.Properties.Add("profile", new OpenApiSchema() { Type = "string", Nullable = true });
//    ////        halLinkValueSchema.Properties.Add("templated", new OpenApiSchema() { Type = "boolean", Default = new OpenApiBoolean(false) });
//    ////        halLinkValueSchema.Properties.Add("title", new OpenApiSchema() { Type = "string", Nullable = true });
//    ////        halLinkValueSchema.Properties.Add("type", new OpenApiSchema() { Type = "string", Nullable = true });
//    ////        halLinkValueSchema.AdditionalPropertiesAllowed = true;

//    ////        ////halLinkSchema.Xml = new() { Name = "_links" };

//    ////        var halLinkSchema = new OpenApiSchema()
//    ////        {
//    ////            OneOf = new List<OpenApiSchema>() {
//    ////                new OpenApiSchema()
//    ////                {
//    ////                    Type = "array",
//    ////                    Items = halLinkValueSchema
//    ////                },
//    ////                halLinkValueSchema
//    ////            }
//    ////        };

//    ////        var halLinksSchema = new OpenApiSchema
//    ////        {
//    ////            Type = "object",
//    ////            AdditionalPropertiesAllowed = true,
//    ////        };

//    ////        OpenApiObject example = new();

//    ////        foreach (var linkAttr in linksAttrs)
//    ////        {
//    ////            halLinksSchema.Properties.Add(linkAttr.Rel, halLinkSchema);
//    ////            example.Add(linkAttr.Rel, new OpenApiObject
//    ////            {
//    ////                ["href"] = new OpenApiString($"string"),
//    ////            });
//    ////            foreach (var rel in linkAttr.AdditionalRels)
//    ////            {
//    ////                halLinksSchema.Properties.Add(rel, halLinkSchema);
//    ////                example.Add(rel, new OpenApiObject
//    ////                {
//    ////                    ["href"] = new OpenApiString($"string"),
//    ////                });
//    ////            }
//    ////        }

//    ////        halLinksSchema.Example = example;

//    ////        return halLinksSchema;
//    ////    }


//    ////    private readonly JsonOptions _options;
//    ////    private readonly Dictionary<string, OpenApiSchema> _cache = new();
//    ////}
//}
