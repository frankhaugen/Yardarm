﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Yardarm
{
    public class YardarmGenerationResult
    {
        public GenerationContext Context { get; }

        public IList<YardarmCompilationResult> CompilationResults { get; }

        public bool Success => CompilationResults.All(p => p.EmitResult.Success);

        public YardarmGenerationResult(GenerationContext context, IList<YardarmCompilationResult> compilationResults)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
            CompilationResults = compilationResults ?? throw new ArgumentNullException(nameof(compilationResults));
        }

        public IEnumerable<Diagnostic> GetAllDiagnostics() =>
            CompilationResults.SelectMany(p =>
            {
                if (!p.AdditionalDiagnostics.IsDefaultOrEmpty)
                {
                    return p.AdditionalDiagnostics.Concat(p.EmitResult.Diagnostics);
                }
                else
                {
                    return p.EmitResult.Diagnostics;
                }
            });
    }
}
