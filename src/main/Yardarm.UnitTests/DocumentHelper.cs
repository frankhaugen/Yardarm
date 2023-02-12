using System;
using System.IO;
using System.Net.Http;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;
using Xunit.Abstractions;

namespace Yardarm.UnitTests;

public static class DocumentHelper
{
    public static OpenApiDocument DownloadDocument(Uri uri, ITestOutputHelper outputHelper)
    {
        OpenApiDocument document = DocumentHelper.DownloadDocument(uri, out OpenApiDiagnostic diagnostic);
        if (diagnostic.Errors.Count > 0) outputHelper.WriteLine(diagnostic);
        return document;
    }
    
    public static OpenApiDocument DownloadDocument(Uri uri, out OpenApiDiagnostic diagnostic)
    {
        OpenApiStreamReader reader = new();
        using Stream stream = Download(uri);
        return reader.Read(stream, out diagnostic);
    }

    public static FileInfo DownloadToFile(FileInfo file, Uri uri)
    {
        using Stream stream = Download(uri);
        using FileStream fileStream = file.OpenWrite();
        stream.CopyTo(fileStream);
        return file;
    }

    public static Stream Download(Uri uri) => uri.IsFile ? File.OpenRead(uri.LocalPath) : new HttpClient().GetStreamAsync(uri).GetAwaiter().GetResult();
}
