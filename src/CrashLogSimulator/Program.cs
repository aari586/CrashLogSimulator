using CrashLogSimulator.Crashes;
using CrashLogSimulator.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

return await RunAsync(args);

static async Task<int> RunAsync(string[] args)
{
    var host = Host.CreateDefaultBuilder(args)
        .UseContentRoot(AppContext.BaseDirectory)
        .ConfigureAppConfiguration(static (context, config) =>
        {
            config.SetBasePath(AppContext.BaseDirectory);
            config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
        })
        .ConfigureServices(static (context, services) =>
        {
            services.Configure<FileSystemLogSinkOptions>(
                context.Configuration.GetSection(FileSystemLogSinkOptions.SectionName));
            services.AddSingleton<ILogSink, FileSystemLogSink>();
            services.AddSingleton<CompositeLogDispatcher>(sp =>
            {
                var sinks = sp.GetServices<ILogSink>();
                var correlationId = Guid.NewGuid().ToString("N");
                return new CompositeLogDispatcher(sinks, correlationId);
            });
        })
        .Build();

    var log = host.Services.GetRequiredService<CompositeLogDispatcher>();
    var fileOptions = host.Services.GetRequiredService<IOptions<FileSystemLogSinkOptions>>().Value;

    AppDomain.CurrentDomain.UnhandledException += (_, e) =>
    {
        if (e.ExceptionObject is Exception ex)
        {
            log.CriticalAsync("Unhandled domain exception", ex).GetAwaiter().GetResult();
            log.FlushAllAsync().GetAwaiter().GetResult();
        }
    };

    TaskScheduler.UnobservedTaskException += (_, e) =>
    {
        log.CriticalAsync("Unobserved task exception", e.Exception).GetAwaiter().GetResult();
        log.FlushAllAsync().GetAwaiter().GetResult();
        e.SetObserved();
    };

    if (args.Length == 0 || args.Contains("--help", StringComparer.OrdinalIgnoreCase) || args.Contains("-h", StringComparer.OrdinalIgnoreCase))
    {
        PrintHelp();
        return 0;
    }

    if (args.Contains("--list", StringComparer.OrdinalIgnoreCase))
    {
        ListScenarios();
        return 0;
    }

    var scenarioArg = args.FirstOrDefault(a => !a.StartsWith('-'));
    if (string.IsNullOrWhiteSpace(scenarioArg))
    {
        Console.Error.WriteLine("Specify a scenario id or use --list.");
        return 2;
    }

    ICrashScenario scenario;
    if (scenarioArg.Equals("random", StringComparison.OrdinalIgnoreCase))
    {
        scenario = CrashScenarioCatalog.PickRandom();
        await log.InfoAsync($"Random pick: {scenario.Id}", properties: new Dictionary<string, object?> { ["pickedScenario"] = scenario.Id }).ConfigureAwait(false);
    }
    else
    {
        var found = CrashScenarioCatalog.Find(scenarioArg);
        if (found is null)
        {
            Console.Error.WriteLine($"Unknown scenario '{scenarioArg}'. Use --list.");
            return 2;
        }

        scenario = found;
    }

    await log.InfoAsync(
        "CrashLogSimulator starting",
        scenario.Id,
        new Dictionary<string, object?>
        {
            ["machine"] = Environment.MachineName,
            ["os"] = Environment.OSVersion.VersionString,
            ["dotnet"] = Environment.Version.ToString(),
            ["logDirectory"] = fileOptions.Directory,
            ["agentHint"] = scenario.HintForAgent,
            ["fixFile"] = scenario.FixFile,
            ["fixMember"] = scenario.FixMember,
        }).ConfigureAwait(false);

    await log.InfoAsync($"Executing scenario: {scenario.Name} — {scenario.Description}", scenario.Id).ConfigureAwait(false);

    try
    {
        await scenario.RunAsync(log).ConfigureAwait(false);
        await log.InfoAsync("Scenario completed without throwing (check exit code / logs).", scenario.Id).ConfigureAwait(false);
    }
    catch (Exception ex)
    {
        await log.CriticalAsync($"Scenario '{scenario.Id}' faulted", ex, scenario.Id).ConfigureAwait(false);
        Environment.ExitCode = 1;
    }
    finally
    {
        await log.FlushAllAsync().ConfigureAwait(false);
        if (host.Services.GetRequiredService<ILogSink>() is IAsyncDisposable disposable)
        {
            await disposable.DisposeAsync().ConfigureAwait(false);
        }
    }

    return Environment.ExitCode;
}

static void PrintHelp()
{
    Console.WriteLine("""
        CrashLogSimulator — generates realistic crash logs for AI repair demos.

        Usage:
          dotnet run --project src/CrashLogSimulator -- <scenario-id>
          dotnet run --project src/CrashLogSimulator -- random
          dotnet run --project src/CrashLogSimulator -- --list

        Categories: null/ref, index, divide, cast, IO, async, LINQ, crypto, logic (no throw), and more.

        Logs (default sink): JSON lines under ./logs/ (see appsettings.json).

        Add more ILogSink implementations and register them in DI for plug-and-play targets.
        """);
}

static void ListScenarios()
{
    Console.WriteLine("Available crash scenarios (use id as argument):\n");
    Console.WriteLine($"  {"random",-14} Pick one scenario from the safe pool");
    foreach (var s in CrashScenarioCatalog.All)
    {
        Console.WriteLine($"  {s.Id,-14} {s.Name}");
        Console.WriteLine($"               {s.Description}");
    }

    Console.WriteLine("\nQuick variety (PowerShell) — one log line-set per run:");
    Console.WriteLine(@"  foreach ($id in 'nullref','json','aggregate','linq','crypto','logic') {");
    Console.WriteLine(@"    dotnet run --project src/CrashLogSimulator -- $id }");
}