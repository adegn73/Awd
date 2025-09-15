using System.Runtime.InteropServices;

namespace Awd
{
    public static class Lzh
    {
        [DllImport("awdbedit.dll", EntryPoint = "?lzhInit@@YAXXZ")]
        public static extern void lzhInit();

        [DllImport("awdbedit.dll", EntryPoint = "?lzhExpand@@YA?AW4lzhErr@@PAUlzhHeader@@PAXKPAG@Z")]
        public static extern LzhErr lzhExpand(IntPtr lzhptr, IntPtr outbuf, uint outbufsize, ref ushort crc);

        [DllImport("awdbedit.dll", EntryPoint = "?lzhCompress@@YA?AW4lzhErr@@PAXK0K0KPAKG@Z")]
        public static extern LzhErr lzhCompress(IntPtr fname, uint fnamelen, IntPtr inbuf, uint inbufsize, IntPtr outbuf, uint outbufsize, ref uint usedsize, ushort fileType);
    }
}
