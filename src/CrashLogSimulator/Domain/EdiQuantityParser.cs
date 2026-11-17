namespace CrashLogSimulator.Domain;

public sealed class EdiQuantityParser
{
    public int ParseQuantity(string raw) =>
        int.Parse(raw, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture);
}