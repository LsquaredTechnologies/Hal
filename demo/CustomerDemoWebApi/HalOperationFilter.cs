using Lsquared.AspNetCore.Hal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CustomerDemoWebApi
{
    public sealed class HalOperationFilter : IOperationFilter
    {
        private readonly JsonOptions _options;

        public HalOperationFilter(IOptions<JsonOptions> options) =>
            _options = options.Value;

        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var halSchema = GetOrCreateHalSchema(context.MethodInfo, context);
            var stdSchema = GetOrCreateStdSchema(context.MethodInfo, context);

            var useStdSchema = false;

            foreach (var (_, response) in operation.Responses)
            {
                foreach (var (contentType, mediaType) in response.Content)
                {
                    var isHal = contentType.StartsWith("application/hal+", StringComparison.OrdinalIgnoreCase);
                    if (isHal)
                    {
                        mediaType.Schema = halSchema;
                    }
                    else
                    {
                        mediaType.Schema = stdSchema;
                        useStdSchema = true;
                    }
                }
            }

            if (!useStdSchema && stdSchema?.Reference is not null)
                context.SchemaRepository.Schemas.Remove(stdSchema.Reference.Id);

            var originalReturnType = context.MethodInfo.ReturnType;
            var originalReturnTypeName = originalReturnType.Name;
            if (originalReturnType.IsGenericType && originalReturnType.GetGenericTypeDefinition() == typeof(HalResourceResult<>))
                originalReturnTypeName = $"{originalReturnType.GenericTypeArguments[0].Name}HalResourceResult";

            context.SchemaRepository.Schemas.Remove(originalReturnTypeName);


            /*
                ////var originalSchema = mediaType.Schema.Reference;
                //

                OpenApiSchema? newSchemaReal = newSchema;
                if (newSchema.Reference is not null)
                {
                    var newSchemaRefId = newSchema.Reference.Id;
                    context.SchemaRepository.Schemas.TryGetValue(newSchemaRefId, out newSchemaReal);
                }
                else
                    newSchemaReal = newSchema;

                if (isCollection)
                {
                }
                else
                {
                    newSchemaReal!.Properties.Add("_links", CreateLinksSchema(context, methodLinksAttrs));
                    newSchemaReal!.Properties.Add("_embedded", CreateEmbeddedSchema(context, methodEmbeddedsAttrs));
              }

             mediaType.Schema = newSchema;
             */
        }

        private OpenApiSchema GetOrCreateHalSchema(MethodInfo methodInfo, OperationFilterContext context)
        {
            var returnType = methodInfo.ReturnType;
            if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(HalResourceResult<>))
                returnType = returnType.GenericTypeArguments[0];

            if (returnType != typeof(string) &&
                returnType.IsGenericType &&
                typeof(IEnumerable).IsAssignableFrom(returnType))
            {
                var collectionItemType = returnType.GenericTypeArguments[0];
                var collectionReferenceId = collectionItemType.Name;
                return GetOrCreateHalCollectionSchema(methodInfo, collectionItemType, $"CollectionOf{collectionReferenceId}", context);
            }

            var referenceId = returnType.Name + "Hal";
            return GetOrCreateHalResourceSchema(methodInfo, returnType, referenceId, context);
        }

        private OpenApiSchema GetOrCreateHalCollectionSchema(MethodInfo methodInfo, Type collectionItemType, string referenceId, OperationFilterContext context)
        {
            if (context.SchemaRepository.Schemas.TryGetValue(referenceId, out var halSchema))
                return halSchema;


            var itemSchema = GetOrCreateHalResourceSchema(null, collectionItemType, collectionItemType.Name, context);

            var collectionSchema = new OpenApiSchema
            {
                Items = itemSchema,
            };

            var halEmbeddedAttr = methodInfo.GetCustomAttribute<HalEmbeddedAttribute>();
            var key = halEmbeddedAttr?.Name ?? "items";

            var halCollectionSchema = new OpenApiSchema
            {
                Properties =
                {
                    ["_embedded"] = new OpenApiSchema
                    {
                        Properties =
                        {
                            [key] = collectionSchema
                        }
                    }
                }
            };

            var methodLinksAttrs = methodInfo.GetCustomAttributes<HalLinksAttribute>();
            halCollectionSchema.Properties.Add("_links", CreateInlineLinksSchema(methodLinksAttrs));

            return context.SchemaRepository.AddDefinition(referenceId, halCollectionSchema);
        }

        private OpenApiSchema GetOrCreateHalResourceSchema(MethodInfo? methodInfo, Type type, string referenceId, OperationFilterContext context)
        {
            if (context.SchemaRepository.Schemas.TryGetValue(referenceId, out var halSchema) &&
                context.SchemaRepository.TryLookupByType(type, out var halSchemaRef))
                return halSchemaRef;

            halSchema = new OpenApiSchema();
            var halEmbeddedSchema = new OpenApiSchema();

            var properties = type.GetProperties();
            foreach (var property in properties)
            {
                var propertyName = _options.JsonSerializerOptions.PropertyNamingPolicy?.ConvertName(property.Name) ?? property.Name;
                var isCollectionProperty =
                    property.PropertyType != typeof(string) &&
                    property.PropertyType.IsGenericType &&
                    typeof(IEnumerable).IsAssignableFrom(property.PropertyType);
                if (isCollectionProperty)
                {
                    var collectionItemType = property.PropertyType.GenericTypeArguments[0];
                    var propertySchema = context.SchemaGenerator.GenerateSchema(collectionItemType, context.SchemaRepository);
                    halEmbeddedSchema.Properties.Add(propertyName, propertySchema);
                }
                else
                {
                    var propertySchema = context.SchemaGenerator.GenerateSchema(property.PropertyType, context.SchemaRepository);
                    propertySchema.ApplyCustomAttributes(property.GetCustomAttributes());
                    propertySchema.Nullable = property.PropertyType.IsReferenceOrNullableType();
                    ////if (property.GetCustomAttribute<System.Diagnostics.CodeAnalysis.NotNullAttribute>() is not null)
                    ////    propertySchema.Nullable = false;
                    halSchema.Properties.Add(propertyName, propertySchema);
                }
            }

            if (halEmbeddedSchema.Properties.Count > 0)
                halSchema.Properties.Add("_embedded", halEmbeddedSchema);

            var classLinksAttrs = type.GetCustomAttributes<HalLinksAttribute>();
            var methodLinksAttrs = methodInfo?.GetCustomAttributes<HalLinksAttribute>() ?? Enumerable.Empty<HalLinksAttribute>();
            halSchema.Properties.Add("_links", CreateInlineLinksSchema(Enumerable.Union(methodLinksAttrs, classLinksAttrs)));
            // TODO xml

            halSchemaRef = context.SchemaRepository.AddDefinition(referenceId, halSchema);
            return halSchemaRef!;
        }

        /*

        private OpenApiSchema? GetOrCreateHalSchema(MethodInfo methodInfo, OperationFilterContext context)
        {
            var returnType = methodInfo.ReturnType;
            if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(HalResourceResult<>))
                returnType = returnType.GenericTypeArguments[0];

            var returnTypeName = $"{returnType.Name}Hal"; // TODO change suffix?
            if (context.SchemaRepository.Schemas.TryGetValue(returnTypeName, out var halSchema))
                return halSchema;

            var isCollectionType = returnType.IsGenericType && typeof(IEnumerable).IsAssignableFrom(returnType);
            // TODO




            //var halSchemaRef = context.SchemaGenerator.GenerateSchema(returnType, context.SchemaRepository);
            //if (!context.SchemaRepository.Schemas.TryGetValue(halSchemaRef.Reference.Id, out halSchema))
            //    return null;

            //halSchemaRef = context.SchemaRepository.AddDefinition(returnTypeName, halSchema);



            //var methodEmbeddedsAttrs = methodInfo.GetCustomAttributes<HalEmbeddedAttribute>();
            //// TODO _embedded

            //context.SchemaRepository.Schemas.Remove(halSchemaRef.Reference.Id);
            ////return halSchemaRef;
        }
        */
        private OpenApiSchema? GetOrCreateStdSchema(MethodInfo methodInfo, OperationFilterContext context)
        {
            var returnType = methodInfo.ReturnType;

            if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(HalResourceResult<>))
                returnType = returnType.GenericTypeArguments[0];

            if (returnType != typeof(string) &&
                returnType.IsGenericType &&
                typeof(IEnumerable).IsAssignableFrom(returnType))
                return null;

            if (!context.SchemaRepository.Schemas.TryGetValue(returnType.Name, out var schemaRef))
                schemaRef = context.SchemaGenerator.GenerateSchema(returnType, context.SchemaRepository);

            ////var properties = returnType.GetProperties();
            ////foreach (var property in properties)
            ////{
            ////    if (schemaRef.Properties.TryGetValue(property.Name, out var propertySchema) &&
            ////        property.GetCustomAttribute() is not null)
            ////        propertySchema.Nullable = false;
            ////}

            return schemaRef;
        }


        ////private OpenApiSchema CreateEmbeddedSchema(OperationFilterContext context, IEnumerable<HalEmbeddedAttribute> embeddedsAttrs)
        ////{
        ////    var x = new OpenApiSchema
        ////    {
        ////        Type = "object",
        ////    };

        ////    foreach (var embeddedsAttr in embeddedsAttrs)
        ////    {
        ////        x.Properties.Add(embeddedsAttr.Name, new OpenApiSchema()
        ////        {
        ////            Type = "array",
        ////            Items = context.SchemaGenerator.GenerateSchema(embeddedsAttr.ClrType, context.SchemaRepository)
        ////        });
        ////    }

        ////    return x;
        ////}

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
    }
}
