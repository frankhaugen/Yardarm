using System.Text.Json;
using System.Text.Json.Serialization;

namespace Yardarm.CommandLine.IntegrationTests;

public static class TestOutputHelperExtensions
{
    public static void WriteLine<T>(this ITestOutputHelper outputHelper, T value)
    {
        outputHelper.WriteLine(JsonSerializer.Serialize(value, new JsonSerializerOptions { WriteIndented = true, ReferenceHandler = ReferenceHandler.IgnoreCycles}));
    }
}
