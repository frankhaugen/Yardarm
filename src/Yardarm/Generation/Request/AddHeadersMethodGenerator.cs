﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Generation.MediaType;
using Yardarm.Helpers;
using Yardarm.Names;
using Yardarm.Spec;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.Generation.Request
{
    public class AddHeadersMethodGenerator : IAddHeadersMethodGenerator
    {
        public const string AddHeadersMethodName = "AddHeaders";
        public const string RequestMessageParameterName = "requestMessage";

        protected IMediaTypeSelector MediaTypeSelector { get; }
        protected INameFormatterSelector NameFormatterSelector { get; }

        public AddHeadersMethodGenerator(IMediaTypeSelector mediaTypeSelector, INameFormatterSelector nameFormatterSelector)
        {
            MediaTypeSelector = mediaTypeSelector ?? throw new ArgumentNullException(nameof(mediaTypeSelector));
            NameFormatterSelector = nameFormatterSelector ?? throw new ArgumentNullException(nameof(nameFormatterSelector));
        }

        public MethodDeclarationSyntax Generate(LocatedOpenApiElement<OpenApiOperation> operation) =>
            MethodDeclaration(
                    PredefinedType(Token(SyntaxKind.VoidKeyword)),
                    AddHeadersMethodName)
                .AddModifiers(Token(SyntaxKind.PublicKeyword))
                .AddParameterListParameters(
                    Parameter(Identifier(RequestMessageParameterName))
                        .WithType(WellKnownTypes.System.Net.Http.HttpRequestMessage.Name))
                .WithBody(Block(GenerateStatements(operation)));

        protected virtual IEnumerable<StatementSyntax> GenerateStatements(
            LocatedOpenApiElement<OpenApiOperation> operation)
        {
            LocatedOpenApiElement<OpenApiResponses> responseSet = operation.GetResponseSet();
            LocatedOpenApiElement<OpenApiResponse> primaryResponse = responseSet.Element
                .OrderBy(p => p.Key)
                .Select(p => responseSet.CreateChild(p.Value, p.Key))
                .First();

            LocatedOpenApiElement<OpenApiMediaType>? mediaType = MediaTypeSelector.Select(primaryResponse);
            if (mediaType != null)
            {
                yield return ExpressionStatement(InvocationExpression(
                        SyntaxHelpers.MemberAccess(RequestMessageParameterName, "Headers", "Accept", "Add"))
                    .AddArgumentListArguments(
                        Argument(ObjectCreationExpression(WellKnownTypes.System.Net.Http.Headers.MediaTypeWithQualityHeaderValue.Name)
                            .AddArgumentListArguments(
                                Argument(SyntaxHelpers.StringLiteral(mediaType.Key))))));
            }

            var propertyNameFormatter = NameFormatterSelector.GetFormatter(NameKind.Property);
            foreach (var headerParameter in operation.Element.Parameters.Where(p => p.In == ParameterLocation.Header))
            {
                string propertyName = propertyNameFormatter.Format(headerParameter.Name);

                StatementSyntax statement = ExpressionStatement(InvocationExpression(
                        SyntaxHelpers.MemberAccess(RequestMessageParameterName, "Headers", "Add"))
                    .AddArgumentListArguments(
                        Argument(SyntaxHelpers.StringLiteral(headerParameter.Name)),
                        Argument(InvocationExpression(
                            MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                IdentifierName(propertyName),
                                IdentifierName("ToString"))))));

                if (!headerParameter.Required)
                {
                    statement = MethodHelpers.IfNotNull(
                        IdentifierName(propertyName),
                        Block(statement));
                }

                yield return statement;
            }
        }

        public static InvocationExpressionSyntax InvokeAddHeaders(ExpressionSyntax requestInstance,
            ExpressionSyntax requestMessageInstance) =>
            InvocationExpression(
                    MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                        requestInstance,
                        IdentifierName(AddHeadersMethodName)))
                .AddArgumentListArguments(Argument(requestMessageInstance));
    }
}
