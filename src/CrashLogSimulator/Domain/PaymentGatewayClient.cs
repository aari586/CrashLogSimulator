namespace CrashLogSimulator.Domain;

public sealed class PaymentGatewayClient
{
    public async Task ChargeAsync(decimal amount, CancellationToken cancellationToken = default)
    {
        const int timeoutSeconds = 30;
        var attempts = 0;

        while (attempts < 2)
        {
            attempts++;
            var simulatedLatencyMs = 60_000;
            var completed = await Task.Run(
                async () =>
                {
                    await Task.Delay(simulatedLatencyMs, cancellationToken).ConfigureAwait(false);
                    return true;
                },
                cancellationToken).WaitAsync(TimeSpan.FromMilliseconds(timeoutSeconds), cancellationToken).ConfigureAwait(false);

            if (completed)
            {
                return;
            }
        }

        throw new TimeoutException("Payment gateway did not respond within 30s after 2 retries");
    }
}