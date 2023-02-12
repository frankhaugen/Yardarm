﻿using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using Xunit.Abstractions;

namespace Yardarm.UnitTests;

public class YardarmGeneratorTests : IDisposable
{
    private readonly ITestOutputHelper _outputHelper;
    private readonly TestContext _context;

    public YardarmGeneratorTests(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
        _context = GetContext(nameof(YardarmGeneratorTests));
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


    [Fact]
    public async Task Generate()
    {
        var generator = new YardarmSyntaGenerator(DocumentHelper.DownloadDocument(new Uri(@"C:\temp\swagger.semine.json"), _outputHelper), GetSettings(_context));
        CSharpCompilation result = await generator.CompileAsync();

        var codeGroups = result.SyntaxTrees.GroupBy(x => x.FilePath);

        foreach (var group in codeGroups)
        {
            var filePath = Path.Combine(_context.BasePath.FullName, group.Key);
            var directory = Path.GetDirectoryName(filePath);
            Directory.CreateDirectory(directory);
            var code = string.Join(Environment.NewLine, group.Select(t => t.ToString()));
            _outputHelper.WriteLine(group.Key);
            _outputHelper.WriteLine(code);

            File.WriteAllText(filePath, code);
        }
    }

    private record TestContext(string AssemblyName, DirectoryInfo BasePath, DirectoryInfo IntermediatePath);

    public void Dispose() => _context.IntermediatePath.Delete(true);
}
