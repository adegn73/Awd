namespace Awd
{
    public interface IBiosModule
    {
        byte[] Data { get; }
        string Name { get; }
        int? Offset { get; }
        byte[] Decompressed { get; }
        int CompressedSize { get; }
        int ExpectedCRC { get; }
        int RealCRC { get; }
        int Type { get; }

        void Refresh();
    }
}