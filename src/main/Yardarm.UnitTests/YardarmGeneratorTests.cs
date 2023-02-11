using System;
using System.Collections.Immutable;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;
using NuGet.Frameworks;
using Xunit;
using Xunit.Abstractions;

namespace Yardarm.UnitTests;

public class YardarmGeneratorTests
{
    private readonly ITestOutputHelper _outputHelper;

    public YardarmGeneratorTests(ITestOutputHelper outputHelper) =>
        _outputHelper = outputHelper;


    private static YardarmGenerationSettings GetSettings() => JsonSerializer.Deserialize<YardarmGenerationSettings>("{\"InputFile\":\"C:\\\\repos\\\\frankhaugen\\\\Yardarm\\\\src\\\\main\\\\output\\\\TestJson\\\\swagger.json\",\"Version\":\"1.0.0\",\"KeyFile\":null,\"KeyContainerName\":null,\"PublicSign\":false,\"EmbedAllSources\":false,\"NoRestore\":false,\"References\":[],\"OutputFile\":\"C:\\\\repos\\\\frankhaugen\\\\Yardarm\\\\src\\\\main\\\\output\\\\TestJson\\\\generated\\\\TestJson.dll\",\"OutputXmlFile\":null,\"NoXmlFile\":false,\"OutputDebugSymbols\":null,\"NoDebugSymbols\":false,\"OutputReferenceAssembly\":null,\"NoReferenceAssembly\":false,\"DelaySign\":false,\"OutputPackageFile\":null,\"OutputSymbolsPackageFile\":null,\"NoSymbolsPackageFile\":false,\"RepositoryType\":\"git\",\"RepositoryUrl\":null,\"RepositoryBranch\":null,\"RepositoryCommit\":null,\"AssemblyName\":\"TestJson\",\"RootNamespace\":null,\"TargetFrameworks\":[],\"ExtensionFiles\":[],\"IntermediateOutputPath\":\"C:\\\\repos\\\\frankhaugen\\\\Yardarm\\\\src\\\\main\\\\output\\\\TestJson\\\\intermediate\"}");

    private static DirectoryInfo GetBaseDirectory(string assemblyName)
    {
        DirectoryInfo baseDirectoryInfo = new DirectoryInfo(Environment.CurrentDirectory).Parent.Parent.Parent.Parent.CreateSubdirectory("output").CreateSubdirectory(assemblyName);
        baseDirectoryInfo.Create();
        return baseDirectoryInfo;
    }

    private OpenApiDocument GetDocument(string url)
    {
        OpenApiDocument document = DocumentHelper.DownloadDocument(new Uri(url), out OpenApiDiagnostic diagnostic);

        if (diagnostic.Errors.Count > 0)
        {
            _outputHelper.WriteLine(diagnostic);
        }

        return document;
    }

    [Fact]
    public async Task Generate()
    {
        string assemblyName = "TestJson";
        DirectoryInfo basePath = GetBaseDirectory(assemblyName);
        DirectoryInfo intermediatePath = basePath.CreateSubdirectory("intermediate");
        intermediatePath.Create();

        YardarmGenerationSettings yardarmGenerationSettings = new()
        {
            AssemblyName = assemblyName,
            Version = new Version(1, 0),
            Author = "anonymous",
            BasePath = basePath.FullName,
            // DllOutput = new MemoryStream(),
            // PdbOutput = new MemoryStream(),
            XmlDocumentationOutput = new MemoryStream(),
            IntermediateOutputPath = intermediatePath.FullName,
            TargetFrameworkMonikers = new[] { "net6.0" }.ToImmutableArray(),
            CompilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        };
        YardarmGenerationSettings settings = yardarmGenerationSettings;

        settings.RootNamespace = "TestJson";

        YardarmGenerator generator = new(GetDocument(@"C:\repos\Yardarm\src\main\Yardarm.CommandLine\centeredge-cardsystemapi.json"), settings);
        // await generator.RestoreAsync();
        CSharpCompilation result = await generator.BuildCSharpForTargetFrameworkAsync(new NuGetFramework("net6.0"));

        _outputHelper.WriteLine(result.ToString());

        intermediatePath.Delete(true);
    }
}
