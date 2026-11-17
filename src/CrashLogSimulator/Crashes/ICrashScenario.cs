using CrashLogSimulator.Logging;

namespace CrashLogSimulator.Crashes;

public interface ICrashScenario
{
    string Id { get; }
    string Name { get; }
    string Description { get; }
    string HintForAgent { get; }

    /// <summary>Primary source file the agent should open first (path relative to project).</summary>
    string FixFile { get; }

    /// <summary>Typical member containing the defect.</summary>
    string FixMember { get; }

    /// <summary>Writes breadcrumbs, then triggers the crash (may not return).</summary>
    Task RunAsync(CompositeLogDispatcher log, CancellationToken cancellationToken = default);
}