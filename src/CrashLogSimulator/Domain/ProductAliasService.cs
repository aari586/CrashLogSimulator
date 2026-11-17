namespace CrashLogSimulator.Domain;

public sealed class ProductAliasService
{
    public void CreateAlias(string sku, string alias)
    {
        if (string.IsNullOrWhiteSpace(sku))
        {
            throw new ArgumentException("SKU is required", nameof(sku));
        }

        _ = alias;
    }

    public void CreateAliasFromImportRow(IReadOnlyDictionary<string, string> row)
    {
        var sku = row.GetValueOrDefault("SkuCode");
        CreateAlias(sku!, row["Alias"]);
    }
}