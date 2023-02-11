using CommandLine;
using Serilog;

namespace Yardarm.CommandLine.IntegrationTests;

public class TestApp
{
    private readonly ITestOutputHelper _outputHelper;

    public TestApp(ITestOutputHelper outputHelper)
    {
        Log.Logger = new LoggerConfiguration().WriteTo.Logger(outputHelper.CreateTestLogger()).CreateLogger();

        _outputHelper = outputHelper;
    }

    public async Task<int> Main(string[] args)
    {
        CancellationTokenSource? cts = new();

        int exitCode;
        bool completedGracefully = false;

        AppDomain.CurrentDomain.ProcessExit += (_, _) =>
        {
            // ReSharper disable once AccessToModifiedClosure
            if (!completedGracefully)
            {
                Cancel();
            }
        };
        Console.CancelKeyPress += (_, e) =>
        {
            Cancel();
            // Don't terminate immediately, wait for cancellation to propagate
            e.Cancel = true;
        };

        try
        {
            exitCode = await Parser.Default
                .ParseArguments<GenerateOptions, RestoreOptions, CollectDependenciesOptions>(args)
                .MapResult(
                    (GenerateOptions options) => new GenerateCommand(options).ExecuteAsync(cts.Token),
                    (RestoreOptions options) => new RestoreCommand(options).ExecuteAsync(cts.Token),
                    (CollectDependenciesOptions options) => new CollectDependenciesCommand(options).ExecuteAsync(cts.Token),
                    errs => Task.FromResult(10 + errs.Count()));

            completedGracefully = true;
        }
        catch (OperationCanceledException)
        {
            exitCode = 2;
        }

        await Log.CloseAndFlushAsync();

        return exitCode;

        void Cancel()
        {
            if (!cts.IsCancellationRequested)
            {
                Console.WriteLine("Cancelling...");
                cts.Cancel();
            }
        }
    }
}
