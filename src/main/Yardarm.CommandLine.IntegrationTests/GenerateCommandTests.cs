namespace Yardarm.CommandLine.IntegrationTests;

public class GenerateCommandTests
{
    private readonly string _now = DateTime.Now.ToString("yyyyMMddTHHmm");
    private readonly ITestOutputHelper _outputHelper;
    private readonly TestApp _testApp;

    public GenerateCommandTests(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
        _testApp = new TestApp(outputHelper);
    }

    [Theory]
    // Currently failing
    [InlineData("PetstoreV2", "https://petstore.swagger.io/v2/swagger.json")]
    [InlineData("PetstoreV3", "https://raw.githubusercontent.com/OAI/OpenAPI-Specification/master/examples/v3.0/petstore.yaml")]
    // Currently succeeding
    [InlineData("TestJson", @"C:\repos\Yardarm\src\main\Yardarm.CommandLine\centeredge-cardsystemapi.json")]
    [InlineData("TestYaml", @"C:\repos\Yardarm\src\main\Yardarm.CommandLine\centeredge-cardsystemapi.yaml")]
    public async Task GeneratePetstore(string @namespace, string url)
    {
        OutputDirectories outputDirectories = new(new DirectoryInfo(Environment.CurrentDirectory).Parent.Parent.Parent.Parent.CreateSubdirectory("output").CreateSubdirectory(_now).CreateSubdirectory(@namespace));
        outputDirectories.EnsureDirectoriesExist();

        FileInfo swaggerFile = DocumentHelper.DownloadToFile(outputDirectories.GetOutputFile("swagger.json"), new Uri(url));
        DirectoryInfo outputDirectory = outputDirectories.Base.CreateSubdirectory("generated");
        FileInfo outputfile = new(Path.Combine(outputDirectory.FullName, @namespace + ".dll"));

        string[] args = { "generate", "-n", @namespace, "-o", outputfile.FullName, "--intermediate-dir", outputDirectories.Intermediate.FullName, "-v", "1.0.0", "-i", swaggerFile.FullName };

        int exitCode = await _testApp.Main(args);

        _outputHelper.WriteLine(exitCode, "Exit Code");
        Assert.Equal(0, exitCode);
    }
}
