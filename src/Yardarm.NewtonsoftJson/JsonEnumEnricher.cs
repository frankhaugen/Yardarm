﻿using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Enrichment;
using Yardarm.Generation;
using Yardarm.NewtonsoftJson.Helpers;

namespace Yardarm.NewtonsoftJson
{
    public class JsonEnumEnricher : IEnumEnricher
    {
        public int Priority => 0;

        public EnumDeclarationSyntax Enrich(EnumDeclarationSyntax target,
            LocatedOpenApiElement<OpenApiSchema> context) =>
            context.Element.Type == "string"
                ? target
                    .AddAttributeLists(SyntaxFactory.AttributeList().AddAttributes(
                        SyntaxFactory.Attribute(JsonHelpers.JsonConverterAttributeName()).AddArgumentListArguments(
                            SyntaxFactory.AttributeArgument(SyntaxFactory.TypeOfExpression(JsonHelpers.StringEnumConverterName())))))
                : target;
    }
}