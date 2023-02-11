using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit.Abstractions;

namespace Yardarm.UnitTests;

public static class TestOutputHelperExtensions
{
    private static readonly JsonSerializerOptions _options = new() { WriteIndented = true, ReferenceHandler = ReferenceHandler.IgnoreCycles };

    public static void WriteLine<T>(this ITestOutputHelper outputHelper, T value, string sectionName)
    {
        outputHelper.WriteLine($"=== {sectionName} ===");
        outputHelper.WriteLine(JsonSerializer.Serialize(value, _options));
        outputHelper.WriteLine("=== End ===");
        outputHelper.WriteLine("");
    }

    public static void WriteLine<T>(this ITestOutputHelper outputHelper, T value) => outputHelper.WriteLine(JsonSerializer.Serialize(value, _options));
}
