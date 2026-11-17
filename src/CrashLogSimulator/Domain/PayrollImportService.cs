namespace CrashLogSimulator.Domain;

public sealed class PayrollImportService
{
    public DateTime ParsePeriodEnd(string raw, bool useAustralianFormat)
    {
        var culture = useAustralianFormat
            ? System.Globalization.CultureInfo.GetCultureInfo("en-AU")
            : System.Globalization.CultureInfo.InvariantCulture;

        var format = useAustralianFormat ? "MM/dd/yyyy" : "yyyy-MM-dd";
        return DateTime.ParseExact(raw, format, culture);
    }
}