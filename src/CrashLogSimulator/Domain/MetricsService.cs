namespace CrashLogSimulator.Domain;

public sealed class MetricsService
{
    public int JobsPerSecond(int completedJobs, int totalDurationSeconds) =>
        completedJobs / totalDurationSeconds;
}