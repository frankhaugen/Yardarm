﻿using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Generation;

namespace Yardarm.Enrichment
{
    public interface IOperationInterfaceMethodEnricher : IEnricher<MethodDeclarationSyntax, LocatedOpenApiElement<OpenApiOperation>>
    {
    }
}
