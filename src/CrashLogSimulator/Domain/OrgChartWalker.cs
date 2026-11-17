namespace CrashLogSimulator.Domain;

public sealed class OrgChartWalker
{
    public void BuildReport(int rootEmployeeId)
    {
        Walk(rootEmployeeId, depth: 0);
    }

    private static void Walk(int employeeId, int depth)
    {
        _ = employeeId;
        Walk(employeeId, depth + 1);
    }
}