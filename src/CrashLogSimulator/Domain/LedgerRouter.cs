namespace CrashLogSimulator.Domain;

public sealed class LedgerRouter
{
    public void PostJournalEntry(string ledgerVersion, decimal amount)
    {
        switch (ledgerVersion)
        {
            case "v1":
                PostV1(amount);
                break;
            case "v2":
                PostV2(amount);
                break;
        }
    }

    private static void PostV1(decimal amount) => _ = amount;

    private static void PostV2(decimal amount) =>
        throw new NotImplementedException("Ledger v2 posting is not implemented in this build");
}