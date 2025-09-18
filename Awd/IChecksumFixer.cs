namespace Awd
{
    public interface IChecksumFixer
    {
        void FixChecksum(byte[] data);
    }

    public static class ChecksumFixer
    {
        public static IChecksumFixer GetChecksumFixer(BiosImage bios)
        {
            switch (bios.ImageVersion)
            {
                case AwdbeBIOSVersion.Ver600PG:
                case AwdbeBIOSVersion.Ver451PG:
                    return new DecompressionBlockChecksumFixer(bios);
                default:
                    return new DefaultChecksumFixer();
            }
        }
    }
}
