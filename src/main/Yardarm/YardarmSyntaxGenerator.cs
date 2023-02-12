using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using NuGet.Frameworks;
using Yardarm.Enrichment;
using Yardarm.Enrichment.Compilation;
using Yardarm.Internal;
using Yardarm.Packaging.Internal;

namespace Yardarm
{
    /// <summary>
    /// Generator creates an assembly from an OpenApiDocument.
    /// </summary>
    public class YardarmSyntaGenerator : YardarmProcessor
    {
        private readonly OpenApiDocument _document;

        public YardarmSyntaGenerator(OpenApiDocument document, YardarmGenerationSettings settings)
            : base(settings, settings.BuildServiceProvider(document))
        {
            ArgumentNullException.ThrowIfNull(document);

            _document = document;
        }

        public async Task<CSharpCompilation> CompileAsync(CancellationToken cancellationToken = default)
        {
            var toDispose = new List<IDisposable>();
            try
            {
                var context = ServiceProvider.GetRequiredService<GenerationContext>();

                await PerformRestoreAsync(context, Settings.NoRestore, cancellationToken);

                var targetFramework = NuGetFramework.Parse(Settings.TargetFrameworkMonikers[0]);

                // Perform the compilation
                var (emitResult, compilation, additionalDiagnostics) = await BuildForTargetFrameworkAsync(
                    context, targetFramework,
                    cancellationToken).ConfigureAwait(false);

                return compilation;
            }
            finally
            {
                foreach (var disposable in toDispose)
                {
                    disposable.Dispose();
                }
            }
        }

        private async Task<(NuGetFramework, CSharpCompilation, ImmutableArray<Diagnostic>)> BuildForTargetFrameworkAsync(
            GenerationContext context, NuGetFramework targetFramework,
            CancellationToken cancellationToken = default)
        {
            context.CurrentTargetFramework = targetFramework;

            // Create the empty compilation
            var compilation = CSharpCompilation.Create(Settings.AssemblyName)
                .WithOptions(Settings.CompilationOptions);

            // Run all enrichers against the compilation
            var enrichers = context.GenerationServices.GetRequiredService<IEnumerable<ICompilationEnricher>>();
            compilation = await compilation.EnrichAsync(enrichers, cancellationToken);

            ImmutableArray<Diagnostic> additionalDiagnostics;
            var assemblyLoadContext = new YardarmAssemblyLoadContext();
            try
            {
                var sourceGenerators = context.GenerationServices.GetRequiredService<NuGetRestoreProcessor>()
                    .GetSourceGenerators(context.NuGetRestoreInfo.Providers, context.NuGetRestoreInfo.LockFile,
                        targetFramework, assemblyLoadContext);

                // Execute the source generators
                compilation = ExecuteSourceGenerators(compilation,
                    sourceGenerators,
                    out additionalDiagnostics,
                    cancellationToken);
            }
            finally
            {
                assemblyLoadContext.Unload();
            }

            return (targetFramework, compilation, additionalDiagnostics);
        }

        private async IAsyncEnumerable<EmbeddedText> GetEmbeddedTextsAsync(
            CSharpCompilation compilation, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            if (!Settings.EmbedAllSources)
            {
                yield break;
            }

            foreach (var syntaxTree in compilation.SyntaxTrees
                         .Where(static p => p.FilePath != "")
                         .Cast<CSharpSyntaxTree>())
            {
                var content = (await syntaxTree.GetRootAsync(cancellationToken))
                    .GetText(Encoding.UTF8, SourceHashAlgorithm.Sha1);

                if (content.CanBeEmbedded)
                {
                    yield return EmbeddedText.FromSource(syntaxTree.FilePath, content);
                }
            }
        }

        private CSharpCompilation ExecuteSourceGenerators(CSharpCompilation compilation, IReadOnlyList<ISourceGenerator>? generators,
            out ImmutableArray<Diagnostic> diagnostics, CancellationToken cancellationToken = default)
        {
            if (generators is null || generators.Count == 0)
            {
                diagnostics = ImmutableArray<Diagnostic>.Empty;
                return compilation;
            }

            var driver = CSharpGeneratorDriver.Create(generators);

            driver.RunGeneratorsAndUpdateCompilation(compilation, out var newCompilation, out diagnostics, cancellationToken);

            return (CSharpCompilation)newCompilation;
        }
    }
}
