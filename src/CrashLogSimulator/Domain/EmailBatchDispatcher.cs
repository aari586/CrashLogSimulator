namespace CrashLogSimulator.Domain;

public sealed class EmailBatchDispatcher
{
    public void DispatchBatch(IEnumerable<string> recipients)
    {
        _ = SendInternalAsync(recipients);
    }

    private static async Task SendInternalAsync(IEnumerable<string> recipients)
    {
        await Task.Delay(200).ConfigureAwait(false);
        if (recipients.Any())
        {
            throw new InvalidOperationException("SMTP relay rejected connection");
        }
    }
}