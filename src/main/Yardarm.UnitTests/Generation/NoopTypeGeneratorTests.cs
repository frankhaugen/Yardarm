using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;
using Xunit;
using Xunit.Abstractions;

namespace Yardarm.UnitTests.Generation;

public class NoopTypeGeneratorTests
{
    private readonly ITestOutputHelper _outputHelper;

    public NoopTypeGeneratorTests(ITestOutputHelper outputHelper) => _outputHelper = outputHelper;

    [Fact]
    public void Test()
    {
        OpenApiDocument document = DocumentHelper.DownloadDocument(new Uri("https://raw.githubusercontent.com/OAI/OpenAPI-Specification/master/examples/v3.0/petstore.yaml"), out OpenApiDiagnostic diagnostic);

        if (diagnostic != null)
        {
            _outputHelper.WriteLine(diagnostic.ToString());
        }

        GenerationContext context = new(new ServiceCollection().BuildServiceProvider());

        // var generator = new NoopTypeGenerator<Microsoft.OpenApi.Models.OpenApiXml>(contex, new RootNamespace(new YardarmGenerationSettings("Test")));


        _outputHelper.WriteLine("Hello, world!");
    }
}
