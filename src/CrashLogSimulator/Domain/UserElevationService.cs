namespace CrashLogSimulator.Domain;

public sealed class UserElevationService
{
    public UserRecord FindBreakGlassAdmin(IEnumerable<UserRecord> users) =>
        users.Where(u => u.Role == "Admin").Single();
}