using Awd.MVVM;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Data;

namespace Awd
{
    public class BiosImage
    {
        public byte[] Data { get; }
        private IChecksumFixer checksumFixer;

        public ObservableCollection<IBiosModule> ModulesView { get; } = new ObservableCollection<IBiosModule>();

        public BiosImage(byte[] data, string fileName)
        {
            Lzh.lzhInit();
            this.Data = data;
            FileName = fileName;
            this.IsValid = IsValidFile();
        }

        public bool IsValid { get; }
        public string FileName { get; }
        public int ImageSize => Data.Length;

        public Layout TableLayout { get; private set; }
        public int? TableOffset { get; private set; }
        public int MaxTableSize { get; private set; }
        public AwdbeBIOSVersion ImageVersion { get; private set; }
        public byte ChecksumSeed { get; set; }

        private bool IsValidFile()
        {

            var xEa = Data[Data.Length - 16];
            var mRb = Encoding.ASCII.GetString(Data, Data.Length - 11, 4);

            if (xEa != 0xEA || mRb != "*MRB" || Data.Length % 1024 != 0 || Data.Length > (1024 * 1024))
                return false;

            return true;
        }

        private const string BootBlockSignature = "Award BootBlock Bios";
        private const string DecompBlockSignature = "= Award Decompression Bios =";
        private const int TYPEID_DECOMPBLOCK = 0x0001;
        private const int TYPEID_BOOTBLOCK = 0x0002;

        public void Load()
        {
            var modules = new List<IBiosModule>();
            var count = ImageSize - BootBlockSignature.Length;
            while (count-- != 0)
            {
                if (Encoding.ASCII.GetString(Data, count, BootBlockSignature.Length).Equals(BootBlockSignature, StringComparison.InvariantCultureIgnoreCase))
                {
                    var bootBlock = new BiosModule(Data.Skip(count).ToArray(), count, BootBlockSignature, TYPEID_BOOTBLOCK);
                    modules.Add(bootBlock);
                    break;
                }
            }
            count = ImageSize - DecompBlockSignature.Length;
            while (count-- != 0)
            {
                if (Encoding.ASCII.GetString(Data, count, DecompBlockSignature.Length).Equals(DecompBlockSignature, StringComparison.InvariantCultureIgnoreCase))
                {
                    break;
                }
            }
            var endOfDecompBlock = modules.FirstOrDefault(m => m.Name == BootBlockSignature)?.Offset ?? ImageSize;
            while (count-- > 32)
            {
                var blockOf32 = Data.Skip(count - 32).Take(32).ToArray();
                if (blockOf32.All(b => b == 0xFF))
                    break;
            }
            var decompressionBlock = new BiosModule(Data.Skip(count).Take(endOfDecompBlock - count).ToArray(), count, DecompBlockSignature, TYPEID_DECOMPBLOCK);
            modules.Add(decompressionBlock);

            // load the file table

            for (int i = 0; i < Data.Length - 5; i++)
            {
                if (StringCompare(Data, i + 2, "-lh"))
                {
                    TableOffset = (i);
                    break;
                }
            }

            if (TableOffset == default)
            {
                Console.WriteLine("Unable to locate a file table within the BIOS image!");
                return;
            }

            this.MaxTableSize = ImageSize - TableOffset.GetValueOrDefault() - (int)modules.Sum(m => m.Data.Length);

            // --- 5. Parse file table and decompress entries ---
            count = (int)TableOffset;
            int fileCount = 0;

            // First pass: count files and determine layout
            while (count < Data.Length)
            {
                var lzhhdr = LzhHeader.FromBytes(Data, count, true);
                if (lzhhdr.headerSize == 0 || lzhhdr.headerSize == 0xFF)
                    break;

                var lzhhdra = LzhHeaderAfterFilename.FromBytes(Data, count + lzhhdr.headerSize - 3); // adjust offset as needed

                modules.Add(new LzhModule(lzhhdr, lzhhdra));
                fileCount++;
                count += 2 + lzhhdr.headerSize + (int)lzhhdr.compressedSize;

                if (fileCount == 1)
                {
                    if (StringCompare(Data, count + 4, "-lh"))
                    {
                        TableLayout = Layout.Layout_2_2_2;
                        count += 2;
                    }
                    else if (StringCompare(Data, count + 3, "-lh"))
                    {
                        TableLayout = Layout.Layout_1_1_1;
                        count += 1;
                    }
                }
                else
                {
                    if (StringCompare(Data, count + 4, "-lh"))
                    {
                        if (TableLayout == Layout.Layout_2_2_2)
                        {
                            count += 2;
                        }
                        else
                        {
                            TableLayout = Layout.Layout_UNKNOWN;
                        }
                    }
                    else if (StringCompare(Data, count + 3, "-lh"))
                    {
                        if (TableLayout == Layout.Layout_2_2_2)
                        {
                            if (fileCount == 2)
                            {
                                TableLayout = Layout.Layout_2_1_1;
                                count++;
                            }
                            else
                            {
                                TableLayout = Layout.Layout_UNKNOWN;
                            }
                        }
                        else if (TableLayout == Layout.Layout_2_1_1)
                        {
                            count++;
                        }
                        else if (TableLayout == Layout.Layout_1_1_1)
                        {
                            count++;
                        }
                    }
                    else
                    {
                        switch (TableLayout)
                        {
                            case Layout.Layout_2_2_2:
                                if (Data[count + 2] == 0xFF || Data[count + 2] == 0x00)
                                {
                                    count += 2;
                                }
                                else
                                {
                                    TableLayout = Layout.Layout_UNKNOWN;
                                }
                                break;

                            case Layout.Layout_2_1_1:
                            case Layout.Layout_1_1_1:
                                if (Data[count + 1] == 0xFF || Data[count + 1] == 0x00)
                                {
                                    count++;
                                }
                                else
                                {
                                    TableLayout = Layout.Layout_UNKNOWN;
                                }
                                break;
                        }
                    }
                }
            }

            if (TableLayout == Layout.Layout_UNKNOWN)
            {
                Console.WriteLine("Unable to determine the layout of the file table!");
                return;
            }

            // add fixed offset modules
            while (count < Math.Min(Data.Length - 6, (decompressionBlock?.Offset).GetValueOrDefault()))
            {
                if (StringCompare(Data, count + 2, "-lh"))
                {
                    var lzhhdr = LzhHeader.FromBytes(Data, count, true);
                    if (lzhhdr.headerSize == 0 || lzhhdr.headerSize == 0xFF)
                        break;

                    var lzhhdra = LzhHeaderAfterFilename.FromBytes(Data, count + lzhhdr.headerSize - 3); // adjust offset as needed

                    modules.Add(new LzhModule(lzhhdr, lzhhdra) { IsFixedOffset = true });
                    fileCount++;
                    count += 2 + lzhhdr.headerSize + (int)lzhhdr.compressedSize;
                }
                count++;
            }

            foreach (var mod in modules.OfType<LzhModule>())
            {
                mod.Decompress();
            }


            this.ModulesView.AddRange(modules.OrderBy(m => m.Offset));
            var systemBios = modules.FirstOrDefault(m => m.Type == 0x5000);
            this.ImageVersion = GetImageVersion(systemBios);

            this.checksumFixer = ChecksumFixer.GetChecksumFixer(this);
        }

        private AwdbeBIOSVersion GetImageVersion(IBiosModule systemBios)
        {
            if (systemBios != null)
            {
                if (systemBios.Decompressed.Length < 0x1E060 + 8)
                    return AwdbeBIOSVersion.Unknown;

                int offset = 0x1E060;
                int len = systemBios.Decompressed[offset] - 1;
                int pos = offset + 1;

                while (len-- > 0 && pos + 7 <= systemBios.Decompressed.Length)
                {
                    if (Encoding.ASCII.GetString(systemBios.Decompressed, pos, 7) == "v4.51PG")
                        return AwdbeBIOSVersion.Ver451PG;
                    if (Encoding.ASCII.GetString(systemBios.Decompressed, pos, 7) == "v6.00PG")
                        return AwdbeBIOSVersion.Ver600PG;
                    if (Encoding.ASCII.GetString(systemBios.Decompressed, pos, 4) == "v6.0")
                        return AwdbeBIOSVersion.Ver60;
                    pos++;
                }
            }
            return AwdbeBIOSVersion.Unknown;
        }

        private bool StringCompare(byte[] data, int position, string compareString)
        {
            if (position + compareString.Length >= data.Length)
                return false;

            return Encoding.ASCII.GetString(data, position, compareString.Length).Equals(compareString, StringComparison.InvariantCultureIgnoreCase);
        }

        internal void Save(string fileName, bool skipChecksum)
        {
            var dataList = new List<byte>();
            foreach (var mod in ModulesView.OfType<LzhModule>().Where(m => !m.IsFixedOffset).OrderBy(m => m.Offset).Select((module, i) => new { i, module }))
            {
                dataList.AddRange(mod.module.Data);
                AddSeparator(dataList, mod.i, mod.module);
            }

            var data = Enumerable.Repeat((byte)0xFF, ImageSize).ToArray();
            Array.Copy(dataList.ToArray(), 0, data, TableOffset.GetValueOrDefault(), Math.Min(dataList.Count, MaxTableSize));

            // modules at fixed offsets
            foreach (var mod in ModulesView.OfType<LzhModule>().Where(m => m.IsFixedOffset).OrderBy(m => m.Offset))
            {
                var end = Math.Min(mod.Data.Length, data.Length - mod.Offset.Value);
                Array.Copy(mod.Data, 0, data, mod.Offset.Value, end);
                var list = new List<byte>();
                AddSeparator(list, ModulesView.IndexOf(mod), mod);

                byte sum = 0;
                foreach (var b in mod.Data)
                    sum += b;
                list.Add(sum);
                Array.Copy(list.ToArray(), 0, data, mod.Offset.Value + end, list.Count);
            }

            foreach (var mod in ModulesView.OfType<BiosModule>().OrderBy(m => m.Offset))
            {
                Array.Copy(mod.Data, 0, data, mod.Offset.Value, Math.Min(mod.Data.Length, data.Length - mod.Offset.Value));
            }

            if (!skipChecksum)
                this.checksumFixer.FixChecksum(data);

            File.WriteAllBytes(fileName, data);
        }

        private void AddSeparator(List<byte> dataList, int i, LzhModule mod)
        {
            switch (TableLayout)
            {
                case Layout.Layout_2_2_2:
                    dataList.Add(0x00);
                    dataList.Add(GetSum(mod.Data));
                    break;
                case Layout.Layout_2_1_1:
                    dataList.Add(0x00);
                    if (i == 0)
                        dataList.Add(GetSum(mod.Data));
                    break;
                case Layout.Layout_1_1_1:
                    dataList.Add(0x00);
                    break;
            }
        }

        private byte GetSum(byte[] data)
        {
            return (byte)(data.Sum(b => b) & 0xFF);
        }


    }


    public class LzhModule : ViewModelBase, IBiosModule
    {
        private LzhHeader lzhhdr;
        private LzhHeaderAfterFilename lzhhdra;

        public LzhModule(LzhHeader lzhhdr, LzhHeaderAfterFilename lzhhdra)
        {
            this.lzhhdr = lzhhdr;
            this.lzhhdra = lzhhdra;
            this.Data = lzhhdr.fullImage.Skip(Offset.GetValueOrDefault()).Take(lzhhdr.compressedSize + (lzhhdra.lzhStartOffset - Offset.GetValueOrDefault() + 1)).ToArray();
        }

        public LzhModule()
        {
        }

        public byte[] Decompressed { get; private set; }
        public byte[] Data { get; private set; }
        public int? Offset { get { return lzhhdr.offset; } set { lzhhdr.offset = value.GetValueOrDefault(); } }
        public string Name { get { return System.Text.Encoding.ASCII.GetString(lzhhdr.filename, 0, lzhhdr.filenameLen); } }
        public int CompressedSize { get { return lzhhdr.compressedSize; } }
        public int ExpectedCRC { get { return lzhhdra.crc; } }
        public int RealCRC { get; private set; }

        public int Type
        {
            get { return lzhhdr.fileType; }
            set
            {
                lzhhdr.UpdateFileType((ushort)value);
                lzhhdr.UpdateHeaderSum(GetSum());
            }
        }

        private byte GetSum()
        {
            byte sum = 0;
            for (int i = 2; i < lzhhdra.lzhStartOffset - Offset; i++)
            {
                if (i != 1) // skip checksum byte
                    sum += Data[i];
            }
            return sum;
        }

        public bool IsFixedOffset { get; internal set; }

        internal void Decompress()
        {
            var payload = Data.Skip(lzhhdra.lzhStartOffset).ToArray();

            unsafe
            {
                var outbufSize = (int)lzhhdr.originalSize;
                var managedBuffer = new byte[outbufSize];
                var outbuf = Marshal.AllocHGlobal(outbufSize);
                var headerPtr = Marshal.AllocHGlobal(Data.Length);

                try
                {
                    Marshal.Copy(managedBuffer, 0, outbuf, managedBuffer.Length);
                    Marshal.Copy(Data, 0, headerPtr, Data.Length);

                    ushort crc = 0;
                    var result = Lzh.lzhExpand(headerPtr, outbuf, (uint)outbufSize, ref crc);

                    if (result != LzhErr.LZHERR_OK)
                    {
                        Console.WriteLine($"Error decompressing {Name}: {result}");
                        return;
                    }
                    this.RealCRC = crc;

                    if (crc != lzhhdra.crc)
                        return;

                    Marshal.Copy(outbuf, managedBuffer, 0, outbufSize);
                    Decompressed = managedBuffer;
                }
                finally
                {
                    Marshal.FreeHGlobal(headerPtr);
                    Marshal.FreeHGlobal(outbuf);
                }
            }
        }

        internal void Replace(string fileName)
        {
            var data = File.ReadAllBytes(fileName);
            var offset = this.lzhhdr.offset;
            Compress(Path.GetFileName(fileName), data);
            this.lzhhdr = LzhHeader.FromBytes(this.Data, 0, true);
            this.lzhhdra = LzhHeaderAfterFilename.FromBytes(this.Data, lzhhdr.headerSize - 3); // adjust offset as needed
            this.lzhhdr.offset = offset;
        }

        internal void Compress(string fileName, byte[] data)
        {
            unsafe
            {
                var dataBuf = Marshal.AllocHGlobal(data.Length);
                var outbuf = Marshal.AllocHGlobal(data.Length);
                var fileNameBuf = Marshal.AllocHGlobal(fileName.Length);

                try
                {
                    Marshal.Copy(Encoding.ASCII.GetBytes(fileName), 0, fileNameBuf, fileName.Length);
                    Marshal.Copy(data, 0, dataBuf, data.Length);

                    uint usedsize = 0;
                    var result = Lzh.lzhCompress(fileNameBuf, (uint)fileName.Length, dataBuf, (uint)data.Length, outbuf, (uint)data.Length, ref usedsize, (ushort)Type);

                    var managedBuffer = new byte[usedsize];
                    Marshal.Copy(outbuf, managedBuffer, 0, (int)usedsize);

                    this.Decompressed = data;
                    this.Data = managedBuffer;
                }
                finally
                {
                    Marshal.FreeHGlobal(dataBuf);
                    Marshal.FreeHGlobal(outbuf);
                    Marshal.FreeHGlobal(fileNameBuf);
                }
            }
            lzhhdr = LzhHeader.FromBytes(this.Data, 0);
            lzhhdra = LzhHeaderAfterFilename.FromBytes(this.Data, lzhhdr.headerSize - 3); // adjust offset as needed
            this.RealCRC = lzhhdra.crc;
        }

        public void Refresh()
        {
            RaisePropertyChanged(null);
        }
    }

    public class BiosModule : ViewModelBase, IBiosModule
    {
        public byte[] Data { get; }
        public int? Offset { get; }
        public string Name { get; }

        public byte[] Decompressed => Data;

        public int CompressedSize => Data.Length;

        public int ExpectedCRC => 0;

        public int RealCRC => 0;

        public int Type { get; }

        public BiosModule(byte[] data, int? offset = null, string name = "", int type = 0)
        {
            this.Data = data;
            Offset = offset;
            Name = name;
            Type = type;
        }

        public void Refresh()
        {
            RaisePropertyChanged();
        }
    }

    public enum Layout
    {
        Layout_UNKNOWN = 0,
        Layout_1_1_1,
        Layout_2_1_1,
        Layout_2_2_2
    }

    public enum AwdbeBIOSVersion
    {
        Unknown = 0,
        Ver451PG,
        Ver600PG,
        Ver60
    }

}
