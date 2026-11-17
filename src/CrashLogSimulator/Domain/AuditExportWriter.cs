namespace CrashLogSimulator.Domain;

public sealed class AuditExportWriter
{
    public long WriteAndMeasure(Stream stream)
    {
        using (var writer = new StreamWriter(stream, leaveOpen: true))
        {
            writer.WriteLine("audit");
        }

        stream.Dispose();
        return stream.Length;
    }
}