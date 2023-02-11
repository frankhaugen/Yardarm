using Microsoft.CodeAnalysis;

namespace Yardarm.CommandLine.IntegrationTests;

public class GenerateCommandTests
{
    private readonly ITestOutputHelper _outputHelper;

    public GenerateCommandTests(ITestOutputHelper outputHelper) =>
        _outputHelper = outputHelper;

    [Theory]
    [InlineData("Petstore", "https://petstore.swagger.io/v2/swagger.json")]
    [InlineData("Petstore", "https://raw.githubusercontent.com/OAI/OpenAPI-Specification/master/examples/v3.0/petstore.yaml")]
    [InlineData("TestJson", @"C:\repos\Yardarm\src\main\Yardarm.CommandLine\centeredge-cardsystemapi.json")]
    [InlineData("TestYaml", @"C:\repos\Yardarm\src\main\Yardarm.CommandLine\centeredge-cardsystemapi.yaml")]
    public async Task GeneratePetstore(string @namespace, string url)
    {
        var document = DocumentHelper.DownloadDocument(new Uri(url), out var diagnostic);

        if (diagnostic.Errors.Count > 0)
        {
            _outputHelper.WriteLine(diagnostic);
        }

        var generator = new YardarmGenerator(document, new YardarmGenerationSettings
        {
            RootNamespace = @namespace
        });

        var result = await generator.EmitAsync();

        _outputHelper.WriteLine(result.GetAllDiagnostics().Select(x => x.GetMessage()));
    }
}
