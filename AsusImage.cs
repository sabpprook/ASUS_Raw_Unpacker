using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace asus_raw_unpack
{
    public class AsusImage
    {
        public string Partition { get; set; }
        public string FileName { get; set; }
        public UInt64 Length { get; set; }
        public bool Sparsed { get; set; }
        public UInt64 Unknown { get; set; }
        public UInt32 CRC32 { get; set; }
        public UInt64 Offset { get; set; }

        public AsusImage(string Partition, string FileName, UInt64 Length, bool Sparsed, UInt64 Unknown, UInt32 CRC32, UInt64 Offset)
        {
            this.Partition = Partition;
            this.FileName = FileName;
            this.Length = Length;
            this.Sparsed = Sparsed;
            this.Unknown = Unknown;
            this.CRC32 = CRC32;
            this.Offset = Offset;
        }

        public override string ToString()
        {
            var text = string.Empty;
            text += $"{SplitText(70, '─')}\n";
            text += $"Name:     {Partition}".PadRight(30);
            text += $"File:     {FileName}\n";
            text += $"{SplitText(70, '─')}\n";
            text += $"Size:     {Length:X16}".PadRight(30);
            text += $"Offset:   {Offset:X16}\n";
            text += $"Sparsed:  {Sparsed}".PadRight(30);
            text += $"CRC32:    {CRC32:X8}\n";
            text += $"Unknown:  {Unknown:X}\n";
            return text;
        }

        private string SplitText(int count, char ch)
        {
            var text = string.Empty;
            for (int i = 0; i < count; i++) text += ch;
            return text;
        }
    }
}
