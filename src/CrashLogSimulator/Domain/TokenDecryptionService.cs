namespace CrashLogSimulator.Domain;

public sealed class TokenDecryptionService
{
    private static readonly byte[] KeyV1 = Convert.FromHexString("1111111111111111111111111111111111111111111111111111111111111111");
    private static readonly byte[] KeyV2 = Convert.FromHexString("2222222222222222222222222222222222222222222222222222222222222222");
    private static readonly byte[] SampleIvV2 = Convert.FromHexString("33333333333333333333333333333333");
    private static readonly byte[] SampleCipherV2 = CreateSampleCipher();

    public string Decrypt(ReadOnlySpan<byte> cipherText, int keyVersion)
    {
        var key = ResolveKey(keyVersion);
        var iv = SampleIvV2;

        using var aes = System.Security.Cryptography.Aes.Create();
        aes.Key = key;
        aes.IV = iv;
        using var decryptor = aes.CreateDecryptor();
        var plain = decryptor.TransformFinalBlock(cipherText.ToArray(), 0, cipherText.Length);
        return System.Text.Encoding.UTF8.GetString(plain);
    }

    public string DecryptSampleToken(int keyVersion) => Decrypt(SampleCipherV2, keyVersion);

    private static byte[] ResolveKey(int keyVersion) =>
        keyVersion switch
        {
            1 => KeyV1,
            2 => KeyV1,
            _ => throw new ArgumentOutOfRangeException(nameof(keyVersion)),
        };

    private static byte[] CreateSampleCipher()
    {
        using var aes = System.Security.Cryptography.Aes.Create();
        aes.Key = KeyV2;
        aes.IV = SampleIvV2;
        using var encryptor = aes.CreateEncryptor();
        var plain = System.Text.Encoding.UTF8.GetBytes("sample-token");
        return encryptor.TransformFinalBlock(plain, 0, plain.Length);
    }
}