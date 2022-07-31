namespace Packnic.Core;

/// <summary>
/// MurmurHash by jibit
/// </summary>
/// <remarks>
/// https://github.com/jitbit/MurmurHash.net/
/// </remarks>
public static class MurmurHash
{
    private const uint M = 0x5bd1e995;
    private const int R = 24;

    private static uint Hash(byte[] data, uint seed, int length)
    {
        if (length == 0)
            return 0;
        uint h = seed ^ (uint)length;
        int currentIndex = 0;
        while (length >= 4)
        {
            uint k = (uint)(data[currentIndex++] | data[currentIndex++] << 8 | data[currentIndex++] << 16 | data[currentIndex++] << 24);
            k *= M;
            k ^= k >> R;
            k *= M;

            h *= M;
            h ^= k;
            length -= 4;
        }

        switch (length)
        {
            case 3:
                h ^= (ushort)(data[currentIndex++] | data[currentIndex++] << 8);
                h ^= (uint)(data[currentIndex] << 16);
                h *= M;
                break;
            case 2:
                h ^= (ushort)(data[currentIndex++] | data[currentIndex] << 8);
                h *= M;
                break;
            case 1:
                h ^= data[currentIndex];
                h *= M;
                break;
        }

        h ^= h >> 13;
        h *= M;
        h ^= h >> 15;

        return h;
    }

    public static uint HashNormal(byte[] array)
    {
        List<byte> normalArray = new();

        for (int i = 0; i < array.Length; i++)
        {
            byte b = array[i];

            if (!(b == 9 || b == 10 || b == 13 || b == 32))
            {
                normalArray.Add(b);
            }
        }

        return Hash(normalArray.ToArray(), 1, normalArray.Count);
    }
}