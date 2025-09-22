namespace Awd
{
    public class FileType
    {

        public FileType(string type, int typeId)
        {
            Type = type;
            TypeId = typeId;
        }

        public string Type { get; }
        public int TypeId { get; }

        override public string ToString() => $"{Type} (0x{TypeId:X4})";
    }
}