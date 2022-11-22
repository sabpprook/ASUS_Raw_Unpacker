using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace asus_raw_unpack
{
    internal class Program
    {
        static string ASUS_Magic = "asus package";
        static int ASUS_HDR_Length = 0x2800;
        static List<AsusImage> images = new List<AsusImage>();

        static void Main(string[] args)
        {
            if (args.Length < 2) PrintUsage();
            var option = args[0];
            var rawfile = args[1];
            if (!File.Exists(rawfile)) PrintUsage();

            var result = ParseAsusRaw(rawfile);
            if (!result) return;

            switch (option)
            {
                case "-l":
                case "--list":
                    ListPartitions(rawfile);
                    break;

                case "-d":
                case "--dump":
                    ListPartitions(rawfile);
                    DumpPartitions(rawfile);
                    break;

                default:
                    PrintUsage();
                    break;
            }
        }

        static void PrintUsage()
        {
            var exe = Process.GetCurrentProcess().MainModule.ModuleName;
            Console.WriteLine($"\nASUS ZenFone Raw Unpacker v1.2 by sabpprook\n");
            Console.WriteLine($"Usage: {exe} [option] asus_raw_file\n");
            Console.WriteLine($"Available Options");
            Console.WriteLine($"-l | --list    show partition informations");
            Console.WriteLine($"-d | --dump    dump and print partitions");
            Environment.Exit(0);
        }

        static bool ParseAsusRaw(string rawfile)
        {
            using (var fs = File.OpenRead(rawfile))
            {
                using (var br = new BinaryReader(fs, Encoding.UTF8, true))
                {
                    var magic = Encoding.UTF8.GetString(br.ReadBytes(0xC));
                    if (magic != ASUS_Magic) return false;

                    br.BaseStream.Position = 0x18;

                    var count = (int)br.ReadUInt64();
                    if (count < 1 || count > 100) return false;

                    br.BaseStream.Position = 0x30;

                    var Offset = (ulong)ASUS_HDR_Length;
                    for (int i = 0; i < count; i++)
                    {
                        var Partition = Encoding.Unicode.GetString(br.ReadBytes(0x20)).Replace("\0", "");
                        var FileName = Encoding.UTF8.GetString(br.ReadBytes(0x20)).Replace("\0", "");
                        var Length = br.ReadUInt64();
                        if (Partition.Contains("system") && Offset + Length + 0x100000000 <= (ulong)fs.Length)
                        {
                            Length += 0x100000000;
                        }
                        var Sparsed = br.ReadUInt64() == 1;
                        var Unknown = br.ReadUInt64();
                        var CRC32 = (uint)br.ReadUInt64();
                        images.Add(new AsusImage(Partition, FileName, Length, Sparsed, Unknown, CRC32, Offset));
                        Offset += Length;
                    }
                }
            }
            return true;
        }

        static void Log(string text, bool append = true)
        {
            var file = "output.log";
            if (append)
                File.AppendAllText(file, text);
            else
                File.WriteAllText(file, text);
            Console.Write(text);
        }

        static void ListPartitions(string rawfile)
        {
            Log($"RAW: {Path.GetFileName(rawfile)}\n\n", false);
            foreach (var image in images) Log(image.ToString() + "\n");
        }

        static void DumpPartitions(string rawfile)
        {
            using (var fs = File.OpenRead(rawfile))
            {
                foreach (var image in images)
                {
                    Console.Write($"Process: {image.FileName}...");
                    fs.Position = (long)image.Offset;
                    using (var ofs = new FileStream(image.FileName, FileMode.Create, FileAccess.ReadWrite))
                    {
                        var length = image.Length;
                        while (length > 0)
                        {
                            var size = length > 0x1000000 ? 0x1000000 : (int)length;
                            var buffer = new byte[size];
                            fs.Read(buffer, 0, size);
                            ofs.Write(buffer, 0, size);
                            length -= (ulong)size;
                        }
                    }
                    Console.WriteLine($" Done!");
                    Thread.Sleep(100);
                }
            }
        }
    }
}
