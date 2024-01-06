
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Org.BouncyCastle.Math;

public class SwizzleService
{
    private IServiceProvider _services;

    private ulong _swizzle;
    private ulong _swizzle_inv;

    public SwizzleService(IServiceProvider services)
    {
        _services = services;
    }

    public async Task InitializeAsync()
    {
        await using var scope = _services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ComfyContext>();

        var swizzlerConfig = await db.ComfyConfigs.FindAsync("swizzler");

        if (swizzlerConfig == null)
        {
            swizzlerConfig = new ComfyConfig { Key = "swizzler", Value = Generate() };
            db.ComfyConfigs.Add(swizzlerConfig);
            await db.SaveChangesAsync();
            Console.WriteLine($"No swizzler config found, generated a fresh one.");
        }

        if (string.IsNullOrWhiteSpace(swizzlerConfig.Value))
        {
            swizzlerConfig.Value = Generate();
            await db.SaveChangesAsync();
            Console.WriteLine($"Swizzler config was null, generated a fresh one.");
        }

        string swizzler = swizzlerConfig.Value;
        
        var bytes = Convert.FromHexString(swizzler);
        _swizzle = BitConverter.ToUInt64(bytes.Reverse().ToArray());

        var s = new BigInteger(1, bytes);
        var p64 = BigInteger.ValueOf(2).Pow(64);
        var inv = s.ModInverse(p64);

        _swizzle_inv = BitConverter.ToUInt64(inv.ToByteArrayUnsigned().Reverse().ToArray());

        Console.WriteLine($"SwizzleService loaded swizzler.");

        var v = 0ul;
        while (v == 0) v = (ulong)Random.Shared.NextInt64();

        var swizzle_v = Swizzle(v);

        var unswizzle_v = UnSwizzle(swizzle_v);


        if (v != unswizzle_v)
        {
            Console.WriteLine($"    Swizzle test failed. Please regenerate the swizzle value.");
            Console.WriteLine($"    Bad swizzler value: {swizzler}");
            Console.WriteLine($"    Swizzler coprimes: {_swizzle}, {_swizzle_inv}");
            Console.WriteLine($"    Random test: {v} => {swizzle_v} => {unswizzle_v}");
            throw new Exception("Swizzle test failed.");
        }
        else
        {
            Console.WriteLine("    Swizzle test passed.");
        }
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