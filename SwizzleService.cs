
using Microsoft.Extensions.Configuration;
using Org.BouncyCastle.Math;

public class SwizzleService
{
    private ulong _swizzle;
    private ulong _swizzle_inv;

    public void Initialize(string? swizzler)
    {
        if (string.IsNullOrWhiteSpace(swizzler))
        {
            swizzler = Generate();
            Console.WriteLine($"No swizzler found, using: {swizzler} (SAVE THIS TO CONFIG)");
        }

        var bytes = Convert.FromHexString(swizzler);
        _swizzle = BitConverter.ToUInt64(bytes.Reverse().ToArray());

        var s = new BigInteger(1, bytes);
        var p64 = BigInteger.ValueOf(2).Pow(64);
        var inv = s.ModInverse(p64);

        _swizzle_inv = BitConverter.ToUInt64(inv.ToByteArrayUnsigned().Reverse().ToArray());


        Console.WriteLine($"SwizzleService loaded swizzler {_swizzle}, {_swizzle_inv}");
        var v = (ulong)Random.Shared.NextInt64();
        Console.WriteLine($"    Random test: {v} => {Swizzle(v)} => {UnSwizzle(Swizzle(v))}");
    }

    public string Swizzle(ulong v)
    {
        return Convert.ToHexString(BitConverter.GetBytes(v * _swizzle));
    }

    public ulong UnSwizzle(string s)
    {
        var v = BitConverter.ToUInt64(Convert.FromHexString(s));
        return v * _swizzle_inv;
    }

    public string Generate()
    {
        var bytes = new byte[8];
        while (true)
        {
            Random.Shared.NextBytes(bytes);
            var value = new BigInteger(1, bytes);
            if (!value.IsProbablePrime(1024))
                continue;
            if (!value.Gcd(BigInteger.ValueOf(2).Pow(64)).Equals(BigInteger.One))
                continue;
            return Convert.ToHexString(bytes);
        }
    }
}