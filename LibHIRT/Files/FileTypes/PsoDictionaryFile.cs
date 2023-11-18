using LibHIRT.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace LibHIRT.Files.FileTypes
{

    //.bin
    public class PsoDictionaryEntry
    {
        Int32 ukInt0;
        Int32 ukInt1;
        Int32 ukInt2;
        Int32 count0;
        Int32 count1;
        Int32 count2;
        Int32 count3;
        Int32 gloIdRS;
        Int32 gloIdBCP;
        Int32 gloIdBCV;
        Int32 gloIduk0;
        Int32 gloIduk1;
        Int32 gloIduk2;
        Int32 gloIduk3;
        string path = "";
        List<byte> padding = new List<byte>();

        public Int32 UkInt0 { get => ukInt0; set => ukInt0 = value; }
        public Int32 UkInt1 { get => ukInt1; set => ukInt1 = value; }
        public Int32 UkInt2 { get => ukInt2; set => ukInt2 = value; }
        public Int32 Count0 { get => count0; set => count0 = value; }
        public Int32 Count1 { get => count1; set => count1 = value; }
        public Int32 Count2 { get => count2; set => count2 = value; }
        public Int32 Count3 { get => count3; set => count3 = value; }
        public Int32 GloIdRS { get => gloIdRS; set => gloIdRS = value; }
        public Int32 GloIdBCP { get => gloIdBCP; set => gloIdBCP = value; }
        public Int32 GloIdBCV { get => gloIdBCV; set => gloIdBCV = value; }
        public Int32 GloIduk0 { get => gloIduk0; set => gloIduk0 = value; }
        public Int32 GloIduk1 { get => gloIduk1; set => gloIduk1 = value; }
        public Int32 GloIduk2 { get => gloIduk2; set => gloIduk2 = value; }
        public Int32 GloIduk3 { get => gloIduk3; set => gloIduk3 = value; }
        public string Path { get => path; set => path = value; }
        public List<byte> Padding { get => padding; set => padding = value; }
        public byte StrPadCount { get => (byte)(path.Length + Padding.Count); }
        public int Index { get; set; }
    }


    [FileExtension(".bin")]
    public class PsoDictionaryFile : SSpaceFile
    {
        private uint _count = 0;
        List<PsoDictionaryEntry> psoDictionaryEntries = new List<PsoDictionaryEntry>();
        public PsoDictionaryFile(string name, ISSpaceFile parent = null) : base(name, parent)
        {
        }

        public override string FileTypeDisplay => "PsoDictionaryFile (.bin)";

        public List<PsoDictionaryEntry> PsoDictionaryEntries { get => psoDictionaryEntries; set => psoDictionaryEntries = value; }

        public void ReadFile()
        {
            if (BaseStream == null || !BaseStream.CanRead)
                InitializeStream(HIRTExtractedFileStream.FromFile(this.InDiskPath), 0, 0);
            //BaseStream = ;
            _count = Reader.ReadUInt32();
            psoDictionaryEntries.Clear();
            for (int i = 0; i < _count; i++)
            {
                var entry = new PsoDictionaryEntry
                {
                    UkInt0 = Reader.ReadInt32(),
                    UkInt1 = Reader.ReadInt32(),
                    UkInt2 = Reader.ReadInt32(),
                    Count0 = Reader.ReadInt32(),
                    Count1 = Reader.ReadInt32(),
                    Count2 = Reader.ReadInt32(),
                    Count3 = Reader.ReadInt32(),
                    GloIdRS = Reader.ReadInt32(),
                    GloIdBCP = Reader.ReadInt32(),
                    GloIdBCV = Reader.ReadInt32(),
                    GloIduk0 = Reader.ReadInt32(),
                    GloIduk1 = Reader.ReadInt32(),
                    GloIduk2 = Reader.ReadInt32(),
                    GloIduk3 = Reader.ReadInt32(),
                    Index = i
                };
                byte[] bytes= Reader.ReadBytes(64);
                entry.Path = bytes.ReadStringNullTerminated(0);
                //entry.Path = Reader.ReadStringNullTerminatedRejectLast();
                /*byte temp = Reader.ReadByte();
                while (temp == 0x00 && Reader.BaseStream.Position < Reader.BaseStream.Length)
                {
                    entry.Padding.Add(temp);
                    temp = Reader.ReadByte();
                }
                Reader.BaseStream.Seek(Reader.BaseStream.Position - 1, SeekOrigin.Begin);
                */

                psoDictionaryEntries.Add(entry);

            }
            BaseStream.Close();
        }
        
    }
}
