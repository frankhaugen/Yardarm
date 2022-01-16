﻿using System;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Enrichment;
using Yardarm.Generation;
using Yardarm.SystemTextJson.Helpers;
using Yardarm.SystemTextJson.Internal;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.SystemTextJson
{
    public class JsonDiscriminatorEnricher : IOpenApiSyntaxNodeEnricher<InterfaceDeclarationSyntax, OpenApiSchema>
    {
        protected GenerationContext Context { get; }
        protected ITypeGeneratorRegistry<OpenApiSchema, SystemTextJsonGeneratorCategory> TypeGeneratorRegistry { get; }

        public JsonDiscriminatorEnricher(GenerationContext context,
            ITypeGeneratorRegistry<OpenApiSchema, SystemTextJsonGeneratorCategory> typeGeneratorRegistry)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
            TypeGeneratorRegistry = typeGeneratorRegistry ?? throw new ArgumentNullException(nameof(typeGeneratorRegistry));
        }

        public InterfaceDeclarationSyntax Enrich(InterfaceDeclarationSyntax target,
            OpenApiEnrichmentContext<OpenApiSchema> context) =>
            context.Element.Discriminator?.PropertyName != null
                ? AddJsonConverter(target, context)
                : target;

        protected virtual InterfaceDeclarationSyntax AddJsonConverter(InterfaceDeclarationSyntax target,
            OpenApiEnrichmentContext<OpenApiSchema> context)
        {
            var converter = TypeGeneratorRegistry.Get(context.LocatedElement);

            var attribute = Attribute(SystemTextJsonTypes.Serialization.JsonConverterAttributeName,
                AttributeArgumentList(SingletonSeparatedList(AttributeArgument(TypeOfExpression(converter.TypeInfo.Name)))));

            return target.AddAttributeLists(AttributeList(SingletonSeparatedList(attribute)));
        }
    }
}