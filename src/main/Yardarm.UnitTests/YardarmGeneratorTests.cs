using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;
using Xunit;
using Xunit.Abstractions;

namespace Yardarm.UnitTests;

public class YardarmGeneratorTests
{
    private readonly ITestOutputHelper _outputHelper;

    public YardarmGeneratorTests(ITestOutputHelper outputHelper) =>
        _outputHelper = outputHelper;


    private OpenApiDocument GetDocument(string url)
    {
        var document = DocumentHelper.CreateDocument(new Uri(url), out var diagnostic);

        if (diagnostic.Errors.Count > 0)
        {
            _outputHelper.WriteLine(diagnostic);
        }

        return document;
    }

    [Fact]
    public async Task Generate()
    {
        var document = GetDocument("https://petstore.swagger.io/v2/swagger.json");
        var generator = new YardarmGenerator(document, new YardarmGenerationSettings
        {
            RootNamespace = "Test",
        });

        var result = await generator.EmitAsync();

        _outputHelper.WriteLine(result);
    }
}


public static class TestOutputHelperExtensions
{
    public static void WriteLine<T>(this ITestOutputHelper outputHelper, T value)
    {
        outputHelper.WriteLine(JsonSerializer.Serialize(value, new JsonSerializerOptions { WriteIndented = true }));
    }
}

public static class DocumentHelper
{
    public static OpenApiDocument CreateDocument(Uri uri, out OpenApiDiagnostic diagnostic)
    {
        var reader = new Microsoft.OpenApi.Readers.OpenApiStreamReader();
        var swagger = new HttpClient().GetStreamAsync(uri).GetAwaiter().GetResult();
        var document = reader.Read(swagger, out diagnostic);
        return document;
    }
}
