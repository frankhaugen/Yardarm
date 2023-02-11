using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;

namespace Yardarm.CommandLine.IntegrationTests;

public static class DocumentHelper
{
    public static OpenApiDocument DownloadPetstoreDocument(out OpenApiDiagnostic diagnostic) => DownloadDocument(new Uri("https://petstore.swagger.io/v2/swagger.json"), out diagnostic);

    public static OpenApiDocument DownloadDocument(Uri uri, out OpenApiDiagnostic diagnostic)
    {
        var reader = new OpenApiStreamReader();
        if (uri.IsFile)
        {
            using var stream = File.OpenRead(uri.LocalPath);
            return reader.Read(stream, out diagnostic);
        }
        else
        {
            using var stream = new HttpClient().GetStreamAsync(uri).GetAwaiter().GetResult();
            return reader.Read(stream, out diagnostic);
        }
    }
}
