namespace Awd
{
    public struct LzhHeader
    {
        public byte[] fullImage;
        public int offset;
        public byte headerSize;
        public byte headerSum;
        public byte[] method = new byte[5];
        public int compressedSize;
        public ulong originalSize;
        public ushort _unknown;
        public ushort fileType;
        public byte _0x20;
        public byte _0x01;
        public byte filenameLen;
        public byte[] filename = new byte[256];

        public LzhHeader(byte[] fullImage, int offset)
        {
            this.fullImage = fullImage;
            this.offset = offset;
        }

        public static LzhHeader FromBytes(byte[] data, int offset, bool fromFullImage = false)
        {
            var hdr = new LzhHeader(data, offset);
            if (!fromFullImage)
                offset = 0;
            hdr.headerSize = data[offset + 0];
            hdr.headerSum = data[offset + 1];
            Array.Copy(data, offset + 2, hdr.method, 0, 5);
            hdr.compressedSize = (int)BitConverter.ToUInt32(data, offset + 7);
            hdr.originalSize = BitConverter.ToUInt32(data, offset + 11);
            hdr._unknown = BitConverter.ToUInt16(data, offset + 15);
            hdr.fileType = BitConverter.ToUInt16(data, offset + 17);
            hdr._0x20 = data[offset + 19];
            hdr._0x01 = data[offset + 20];
            hdr.filenameLen = data[offset + 21];
            Array.Copy(data, offset + 22, hdr.filename, 0, hdr.filenameLen);
            return hdr;
        }

        internal void UpdateFileType(ushort value)
        {
            if (fileType != value)
            {
                fileType = value;
                fullImage[offset + 17] = (byte)(value & 0xFF);
                fullImage[offset + 18] = (byte)((value >> 8) & 0xFF);
            }

        }

        internal void UpdateHeaderSum(byte sum)
        {
            headerSum = sum;
            fullImage[offset + 1] = headerSum;
        }
    }
}