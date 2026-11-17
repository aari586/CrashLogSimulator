namespace CrashLogSimulator.Crashes;

public static class CrashScenarioCatalog
{
    public static IReadOnlyList<ICrashScenario> All { get; } =
    [
        new NullReferenceScenario(),
        new IndexOutOfRangeScenario(),
        new DivideByZeroScenario(),
        new InvalidCastScenario(),
        new FileNotFoundScenario(),
        new UnobservedTaskExceptionScenario(),
        new StackOverflowScenario(),
        new WrongConnectionStringScenario(),
        new OffByOneInventoryScenario(),
        new DoubleDisposeScenario(),
        new KeyNotFoundScenario(),
        new FormatExceptionScenario(),
        new JsonExceptionScenario(),
        new AggregateExceptionScenario(),
        new LinqSingleScenario(),
        new OverflowScenario(),
        new ArgumentScenario(),
        new TimeoutScenario(),
        new UnauthorizedScenario(),
        new IOExceptionLockScenario(),
        new NotImplementedScenario(),
        new CryptographicScenario(),
    ];

    /// <summary>Scenarios that are safe for unattended batch runs (no stack overflow, etc.).</summary>
    public static IReadOnlyList<ICrashScenario> SafeForRandomPool { get; } =
        All.Where(s => s.Id is not ("stackoverflow" or "task")).ToList();

    public static ICrashScenario? Find(string id) =>
        All.FirstOrDefault(s => s.Id.Equals(id, StringComparison.OrdinalIgnoreCase));

    public static ICrashScenario PickRandom() =>
        SafeForRandomPool[Random.Shared.Next(SafeForRandomPool.Count)];
}