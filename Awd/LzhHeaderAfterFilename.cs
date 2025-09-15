namespace Awd
{
    public struct LzhHeaderAfterFilename
    {
        public int crc;
        public byte _0x20;
        public int extendedHeaderSize;
        public int lzhStartOffset;

        public LzhHeaderAfterFilename(int lzhStartOffset) : this()
        {
            this.lzhStartOffset = lzhStartOffset;
        }

        public static LzhHeaderAfterFilename FromBytes(byte[] data, int offset)
        {
            var hdr = new LzhHeaderAfterFilename
            {
                crc = BitConverter.ToUInt16(data, offset),
                _0x20 = data[offset + 2],
                extendedHeaderSize = BitConverter.ToUInt16(data, offset + 3),
                lzhStartOffset = offset + 4 + BitConverter.ToUInt16(data, offset + 3),
            };
            return hdr;
        }
    }
}