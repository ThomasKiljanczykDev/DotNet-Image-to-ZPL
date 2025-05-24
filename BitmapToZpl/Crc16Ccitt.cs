namespace BitmapToZpl;

/// <summary>
/// Based on <see href="http://sanity-free.com/133/crc_16_ccitt_in_csharp.html" />.
/// </summary>
public class Crc16Ccitt
{
    private const ushort Poly = 4129;
    private readonly ushort[] _table = new ushort[256];

    public ushort ComputeChecksum(byte[] bytes)
    {
        return bytes.Aggregate(
            (ushort)0,
            (current, b) => (ushort)((current << 8) ^ _table[(current >> 8) ^ (0xff & b)])
        );
    }

    public Crc16Ccitt()
    {
        for (var i = 0; i < _table.Length; ++i)
        {
            ushort temp = 0;
            var a = (ushort)(i << 8);
            for (var j = 0; j < 8; ++j)
            {
                if (((temp ^ a) & 0x8000) != 0)
                {
                    temp = (ushort)((temp << 1) ^ Poly);
                }
                else
                {
                    temp <<= 1;
                }

                a <<= 1;
            }

            _table[i] = temp;
        }
    }
}