using System.Text;

namespace Awd
{
    internal class DecompressionBlockChecksumFixer : IChecksumFixer
    {
        private BiosImage bios;
        private int checksumPosition;

        public DecompressionBlockChecksumFixer(BiosImage bios)
        {
            this.bios = bios;
            this.checksumPosition = FindChecksumPosition(bios.Data);
            bios.ChecksumSeed = (byte)((bios.Data[checksumPosition + 1] - bios.Data[checksumPosition]) & 0xFF);
        }

        public void FixChecksum(byte[] data)
        {
            FixChecksum(data, bios.ChecksumSeed);
        }

        public static void FixChecksum(byte[] rom, byte csum2Seed)
        {
            var pos = FindChecksumPosition(rom);

            byte csum1 = 0x00;
            byte csum2 = csum2Seed; // common seeds: 0xF8 (typical), 0x6E (seen in some builds)
            var i = pos;

            while (i-- > 0)
            {
                csum1 += rom[i];
                csum2 += rom[i];
            }

            rom[pos] = csum1;
            rom[pos + 1] = csum2;
        }

        public static int FindChecksumPosition(byte[] rom)
        {
            var needle = Encoding.ASCII.GetBytes("Award Decompression");
            for (int i = 0; i <= rom.Length - needle.Length; i++)
            {
                int j = 0;
                while (j < needle.Length && rom[i + j] == needle[j]) j++;
                if (j == needle.Length)
                {
                    int pageBase = i & ~0xFFF;   // align down to 4 KiB page
                    int pos = pageBase + 0x0FFE;           // where the two checksum bytes live
                    if (pos + 1 >= rom.Length)
                        throw new InvalidOperationException("Calculated checksum position out of bounds.");

                    return pos;
                }
            }
            throw new InvalidOperationException("Could not locate decompressor (\"Award Decompression\"). Provide decompStartOverride.");
        }
    }
}