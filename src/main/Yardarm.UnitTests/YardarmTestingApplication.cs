using System;
using System.Collections.Immutable;
using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit.Abstractions;

namespace Yardarm.UnitTests;

public class YardarmTestingApplication
{
    private readonly ITestOutputHelper _outputHelper;
    private readonly TestContext _context;

    public YardarmTestingApplication(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
        _context = GetContext(nameof(YardarmTestingApplication));
    }

    private static TestContext GetContext(string assemblyName)
    {
        var baseDirectoryInfo = new DirectoryInfo(Environment.CurrentDirectory).Parent.Parent.Parent.Parent.CreateSubdirectory("output").CreateSubdirectory(assemblyName);
        baseDirectoryInfo.Create();
        var intermediatePath = baseDirectoryInfo.CreateSubdirectory("intermediate");
        intermediatePath.Create();

        return new TestContext(assemblyName, baseDirectoryInfo, intermediatePath);
    }

    private static YardarmGenerationSettings GetSettings(TestContext context)
    {
        var yardarmGenerationSettings = new YardarmGenerationSettings()
        {
            AssemblyName = context.AssemblyName,
            BasePath = context.BasePath.FullName,
            RootNamespace = context.AssemblyName,
            IntermediateOutputPath = context.IntermediatePath.FullName,
            // EmbedAllSources = false,
            Version = new Version(1, 0),
            Author = "anonymous",
            TargetFrameworkMonikers = new[] { "net6.0" }.ToImmutableArray(),
            CompilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary),
            DllOutput = new MemoryStream(),
            PdbOutput = new MemoryStream(),
            XmlDocumentationOutput = new MemoryStream(),
            NuGetOutput = new MemoryStream(),
            NuGetSymbolsOutput = new MemoryStream(),
        };

        return yardarmGenerationSettings;
    }


    private record TestContext(string AssemblyName, DirectoryInfo BasePath, DirectoryInfo IntermediatePath);

    public void Dispose() => _context.IntermediatePath.Delete(true);
}
