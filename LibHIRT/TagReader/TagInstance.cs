using Bond.IO.Unsafe;
using LibHIRT.Common;
using LibHIRT.TagReader.Headers;
using LibHIRT.Utils;
using Newtonsoft.Json;
using System.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

using TagStruct = LibHIRT.TagReader.Headers.TagStruct;

namespace LibHIRT.TagReader
{
    public class RefItCount
    {
        public int i = 0;
        public int f = 0;
        public int r = 0;
    }
    public interface ITagInstance
    {
        static event EventHandler<ITagInstance> OnInstanceLoadEvent;
        public long GetTagSize { get; }

        public void ReadIn(BinaryReader f, TagHeader? header = null);
        public void ReadIn(TagHeader? header = null);

        public string ToJson();
        public TagInstance GetObjByPath(string path);

        public object AccessValue { get; }
        public string FieldName { get; set; }

        public TagLayouts.C TagDef { get; }


    }
    public class TagInstance : ITagInstance
    {

        protected TagLayouts.C tagDef;
        protected long addressStart;
        protected long offset;
        protected HeaderTableEntry? entry = null;
        protected TagStruct? content_entry = null;

        protected TagInstance? parent = null;
        protected long inst_parent_offset = -1;
        protected long inst_address = -1;
        private long inst_global_address = -1;
        protected Dictionary<string, Object> extra_data = new Dictionary<string, object>();

        public static event EventHandler<ITagInstance> OnInstanceLoadEvent;

        public TagInstance(TagLayouts.C tagDef, long addressStart, long offset)
        {
            this.tagDef = tagDef;
            this.addressStart = addressStart;
            this.offset = offset;
        }

        public HeaderTableEntry? Entry { get => entry; set => entry = value; }
        public TagLayouts.C TagDef { get => tagDef;}
        public TagStruct? Content_entry { get => content_entry; set => content_entry = value; }
        public long Offset { get => offset; set => offset = value; }
        public TagInstance? Parent { get => parent; set { parent = value;} }
        public long Inst_parent_offset { get => inst_parent_offset; set => inst_parent_offset = value; }

        public long GetTagSize => throw new NotImplementedException();

        public virtual object AccessValue => new { AddressStart= addressStart, Offset= offset };
        public virtual object AccessValueExtra => new {
            Value = AccessValue,
            AddressStart = addressStart,
            Offset = offset,
            InstOffset = parent == null? Offset : parent.Offset + Offset
        };

        public long Inst_global_address { get {
              
                return inst_global_address; 
            }
        }

        

        public string FieldName { get ; set ; }
        public TagInstance Self => this;

        public virtual TagInstance GetObjByPath(string path)
        {
            throw new NotImplementedException();
        }

        public virtual TagInstance this[string path]
        {
            get => GetObjByPath(path);
        }

        protected void ExeTagInstance() {
            if (OnInstanceLoadEvent != null)
                OnInstanceLoadEvent.Invoke(this, this);
        }

        protected void ExeTagInstance(ITagInstance parent)
        {
            if (OnInstanceLoadEvent != null)
                OnInstanceLoadEvent.Invoke(this, parent);
        }

        public virtual void ReadIn(BinaryReader f, TagHeader? header = null)
        {
            inst_address = f.BaseStream.Position;
            inst_parent_offset = inst_address - addressStart;
        }

        public virtual void ReadIn(TagHeader? header = null)
        {
            throw new NotImplementedException();
        }

        public virtual string ToJson()
        {
            
            return JsonConvert.SerializeObject(AccessValueExtra);
        }

    }
    #region Atomic
    public class AtomicTagInstace : TagInstance
    {
        public AtomicTagInstace(TagLayouts.C tagDef, long addressStart, long offset) : base(tagDef, addressStart, offset)
        {
        }

        public override TagInstance GetObjByPath(string path)
        {
            return this;
        }
    }

    public class ValueTagInstace<T> : AtomicTagInstace
    {
        protected T value;

        public ValueTagInstace(TagLayouts.C tagDef, long addressStart, long offset) : base(tagDef, addressStart, offset)
        {
        }

        public override void ReadIn(BinaryReader f, TagHeader? header = null)
        {
            base.ReadIn(f, header);
            f.BaseStream.Seek(addressStart + offset, SeekOrigin.Begin);
        }

        

        public T Value { get => value; set => this.value = value; }

        public override object AccessValue => Value;

        
    }



    public class DebugDataBlock : ValueTagInstace<string>
    {
        
        public DebugDataBlock(TagLayouts.C tagDef, long addressStart, long offset) : base(tagDef, addressStart, offset)
        {
            value = tagDef.N;
        }

        public override void ReadIn(BinaryReader f, TagHeader? header = null)
        {
            base.ReadIn(f, header);
            value = TagDef.N;
            ExeTagInstance();
        }

    }
    public class Comment : AtomicTagInstace
    {
        
        public Comment(TagLayouts.C tagDef, long addressStart, long offset) : base(tagDef, addressStart, offset)
        {
        }

        public override void ReadIn(BinaryReader f, TagHeader? header = null)
        {
            base.ReadIn(f, header);
            //value = TagDef.N;
            ExeTagInstance();
        }
    }

    public class GenericBlock : ValueTagInstace<string>
    {
        public GenericBlock(TagLayouts.C tagDef, long addressStart, long offset) : base(tagDef, addressStart, offset)
        {
        }

        public override void ReadIn(BinaryReader f, TagHeader? header = null)
        {
            base.ReadIn(f, header);
            // get the hex representation of the readed bytes
            value = BitConverter.ToString(f.ReadBytes((int)TagDef.S)).Replace("-", "");
            ExeTagInstance();
        }

        
    }


    public class EnumGroup : ValueTagInstace<int>
    {
        List<string> options = new List<string>();
        int _selectedIndex = -1;
        string selected;
        public EnumGroup(TagLayouts.C tagDef, long addressStart, long offset) : base(tagDef, addressStart, offset)
        {
        }

        public override object AccessValue => new { Selected = selected, Options = options };

        public List<string> Options { get => options; }
        public int SelectedIndex { get => _selectedIndex; }
        public string Selected { get => selected; }

        public override void ReadIn(BinaryReader f, TagHeader? header = null)
        {
            base.ReadIn(f, header);
            if (TagDef.GetType() != typeof(TagLayouts.EnumGroupTL))
                return;
            TagLayouts.EnumGroupTL tempEnum = (TagDef as TagLayouts.EnumGroupTL);
            foreach (int gvsdahb in tempEnum.STR.Keys)
            {
                options.Add(tempEnum.STR[gvsdahb]);
            }
            f.BaseStream.Seek(addressStart + offset, SeekOrigin.Begin);
            switch (tempEnum.A)
            {
                case 1:
                    _selectedIndex = f.ReadByte();
                    break;
                case 2:
                    _selectedIndex = f.ReadInt16();
                    break;
                case 4:
                    _selectedIndex = f.ReadInt32();
                    break;
                default:
                    break;
            }
            if (options.Count > _selectedIndex && _selectedIndex > -1)
            {
                selected = options[_selectedIndex];
            }
            else
            {
                // debug
            }
            ExeTagInstance();
        }
    }
    public class FourByte : ValueTagInstace<Int32>
    {

        public FourByte(TagLayouts.C tagDef, long addressStart, long offset) : base(tagDef, addressStart, offset)
        {
        }

        public override void ReadIn(BinaryReader f, TagHeader? header = null)
        {
            base.ReadIn(f, header);
            value = f.ReadInt32();
            ExeTagInstance();
        }
    }
    public class TwoByte : ValueTagInstace<Int16>
    {
        public TwoByte(TagLayouts.C tagDef, long addressStart, long offset) : base(tagDef, addressStart, offset)
        {
        }

        public override void ReadIn(BinaryReader f, TagHeader? header = null)
        {
            base.ReadIn(f, header);
            value = f.ReadInt16();
            ExeTagInstance();
        }
    }
    
    public class UTwoByte : ValueTagInstace<UInt16>
    {
        public UTwoByte(TagLayouts.C tagDef, long addressStart, long offset) : base(tagDef, addressStart, offset)
        {
        }

        public override void ReadIn(BinaryReader f, TagHeader? header = null)
        {
            base.ReadIn(f, header);
            value = f.ReadUInt16();
            ExeTagInstance();
        }
    }
    public class Byte : ValueTagInstace<sbyte>
    {
        public Byte(TagLayouts.C tagDef, long addressStart, long offset) : base(tagDef, addressStart, offset)
        {
        }
        public override void ReadIn(BinaryReader f, TagHeader? header = null)
        {
            base.ReadIn(f, header);
            value = f.ReadSByte();
            ExeTagInstance();
        }

    }
    public class Float : ValueTagInstace<float>
    {

        public Float(TagLayouts.C tagDef, long addressStart, long offset) : base(tagDef, addressStart, offset)
        {
        }
        public override void ReadIn(BinaryReader f, TagHeader? header = null)
        {
            try
            {
                base.ReadIn(f, header);
                value = f.ReadSingle();
                ExeTagInstance();
            }
            catch (Exception ex)
            {

                throw ex;
            }
            
        }
    }
    public class TagRef : ValueTagInstace<int>
    {
        string in_game_path = "";
        string path = "";
        // parse;
        TagReferenceFixup tag_ref;
        //self.ref_id_center = None
        //self.ref_id_sub = None
        //self.ref_id = None
        Int64 global_handle = -1;
        Int32 ref_id_int = -1;
        private string ref_id;
        Int32 ref_id_sub_int = -1;
        Int32 ref_id_center_int = -1;

        Int32 local_handle = -1;
        string tagGroup = "";
        string tagGroupRev = "";
        public TagRef(TagLayouts.C tagDef, long addressStart, long offset) : base(tagDef, addressStart, offset)
        {
        }

        public TagReferenceFixup Tag_ref { get => tag_ref; set => tag_ref = value; }

        internal void loadPath()
        {
            ;
        }
        public override void ReadIn(BinaryReader f, TagHeader? header = null)
        {
            base.ReadIn(f, header);
            //f.BaseStream.Seek(addressStart + offset, SeekOrigin.Begin);
            global_handle = f.ReadInt64();
            //ref_id = f.read(4)
            ref_id_int = f.ReadInt32();
            ref_id = Mmr3HashLTU.getMmr3HashFromInt(ref_id_int);
            //ref_id_sub = f.read(4)
            ref_id_sub_int = f.ReadInt32();
            //ref_id_sub = self.ref_id_sub.hex().upper()
            //ref_id_center = f.read(4)
            ref_id_center_int = f.ReadInt32();
            //ref_id_center = self.ref_id_center.hex().upper()
            char[] tag = f.ReadChars(4);
            tagGroup = new string(tag);
            Array.Reverse(tag);// 0x14
            tagGroupRev = new string(tag);
            local_handle = f.ReadInt32();
            ExeTagInstance();
        }

        public override object AccessValue => new { 
            Global_handle = global_handle,
            Ref_id_int = ref_id_int,
            Ref_id_sub_int = ref_id_sub_int,
            Ref_id_center_int = ref_id_center_int,
            TagGroupRev = tagGroupRev,
            Local_handle = local_handle,
            Path = tag_ref?.StrPath
        };

        public long Global_handle { get => global_handle; set => global_handle = value; }
        public int Ref_id_int { get => ref_id_int; set => ref_id_int = value; }
        public int InvalidRef_id_int { get => -1;}
        public int Ref_id_sub_int { get => ref_id_sub_int; set => ref_id_sub_int = value; }
        public int Ref_id_center_int { get => ref_id_center_int; set => ref_id_center_int = value; }
        public string TagGroupRev { get => tagGroupRev; set => tagGroupRev = value; }
        public string Path { get => tag_ref?.StrPath; }
        public string Ref_id { get => ref_id; set => ref_id = value; }
    }
    public class Pointer : ValueTagInstace<Int64>
    {
        public Pointer(TagLayouts.C tagDef, long addressStart, long offset) : base(tagDef, addressStart, offset)
        {
        }

        public override void ReadIn(BinaryReader f, TagHeader? header = null)
        {
            base.ReadIn(f, header);
            value = f.ReadInt64();
            ExeTagInstance();
        }
    }

    public class String : ValueTagInstace<string>
    {
        public String(TagLayouts.C tagDef, long addressStart, long offset) : base(tagDef, addressStart, offset)
        {
        }

        public override void ReadIn(BinaryReader f, TagHeader? header = null)
        {
            base.ReadIn(f, header);
            //value = new string(f.ReadChars((int)TagDef.S));
            value = f.ReadStringNullTerminated((int)TagDef.S);
            if (Mmr3HashLTU.ForceFillData) {
                if (!string.IsNullOrEmpty(value))
                {
                    Mmr3HashLTU.AddUniqueStrValue(value);
                }
                
            }
            ExeTagInstance();
        }
    }
    public class StringTag : ValueTagInstace<string>
    {
        string tag_string_rev = "";
        public StringTag(TagLayouts.C tagDef, long addressStart, long offset) : base(tagDef, addressStart, offset)
        {
        }

        public override void ReadIn(BinaryReader f, TagHeader? header = null)
        {
            base.ReadIn(f, header);
            value = new string(f.ReadChars(4));
            tag_string_rev = value.Reverse().ToString();
            if (Mmr3HashLTU.ForceFillData)
            {
                if (!string.IsNullOrEmpty(value))
                    Mmr3HashLTU.AddUniqueStrValue(value);
                    
                
                if (!string.IsNullOrEmpty(tag_string_rev))
                    Mmr3HashLTU.AddUniqueStrValue(tag_string_rev);


            }
            ExeTagInstance();
        }
    }
    public class Flags : ValueTagInstace<byte>
    {
        public Flags(TagLayouts.C tagDef, long addressStart, long offset) : base(tagDef, addressStart, offset)
        {
        }

        public override void ReadIn(BinaryReader f, TagHeader? header = null)
        {
            base.ReadIn(f, header);
            value = (f.ReadByte());
            ExeTagInstance();
        }
    }
    
    public class Flag
    {
        public string Label { get; set; }
        public string Value { get; set; }
        public List<Flag>? Childrens{ get; set; }
}
    public class FlagGroup : ValueTagInstace<List<bool>>
    {
        
        List<string> options = new List<string>();
        List<bool> options_v = new List<bool>();

        public override object AccessValue => new { options, options_v};

        public FlagGroup(TagLayouts.C tagDef, long addressStart, long offset) : base(tagDef, addressStart, offset)
        {
        }

        public override void ReadIn(BinaryReader f, TagHeader? header = null)
        {
            base.ReadIn(f, header);
            if (TagDef.GetType() != typeof(TagLayouts.FlagGroupTL))
                return;
            TagLayouts.FlagGroupTL tempEnum = (TagDef as TagLayouts.FlagGroupTL);
            generateBits(addressStart+offset, tempEnum.A, tempEnum.MB, tempEnum.STR, f);
            foreach (int gvsdahb in tempEnum.STR.Keys)
            {
                options.Add(tempEnum.STR[gvsdahb]);
            }
            ExeTagInstance();
        }

        public List<Flag> Flags
        {
            get {
                List<Flag> r_options = new List<Flag>();
                for (int i = 0; i < options.Count; i++)
                {
                    r_options.Add(new Flag()
                    {
                        Label = options[i],
                        Value = options_v[i].ToString(),
                    });
                }
                List<Flag> salida = new List<Flag>();
                string result = "";
                for (int i = 0; i < options_v.Count; i++)
                {

                    if (options_v[i])
                    {
                        result = result + "1";
                    }
                    else
                    {
                        result = result + "0";
                    }
                }
                salida.Add(new Flag() {
                    Label = "Bin val",
                    Value = result,
                    Childrens = r_options
                });
                return salida;
            }
            
        }

        public List<bool> Options_v { get => options_v; set => options_v = value; }

        public void generateBits(long startAddress, int amountOfBytes, int maxBit, Dictionary<int, string>? descriptions = null, BinaryReader? f = null)
        {
            /*
            this.startAddress = startAddress;
            this.amountOfBytes = amountOfBytes;
            this.maxBit = maxBit;
            */
            if (maxBit == 0)
            {
                maxBit = maxBit = amountOfBytes * 8;
            }

            //spBitCollection.Children.Clear();

            int maxAmountOfBytes = Math.Clamp((int)Math.Ceiling((double)maxBit / 8), 0, amountOfBytes);
            int bitsLeft = maxBit - 1; // -1 to start at 

            for (int @byte = 0; @byte < maxAmountOfBytes; @byte++)
            {
                if (bitsLeft < 0)
                {
                    continue;
                }

                int amountOfBits = @byte * 8 > maxBit ? ((@byte * 8) - maxBit) : 8;
                long addr = startAddress + @byte;
                f.BaseStream.Seek(addr, SeekOrigin.Begin);
                byte flags_value = (byte)f.ReadByte();//(addr).ToString("X")
                
                for (int bit = 0; bit < amountOfBits; bit++)
                {
                    int currentBitIndex = (@byte * 8) + bit;
                    if (bitsLeft < 0)
                    {
                        continue;
                    }
                    options_v.Add(UtilBinaryReader.GetBit(flags_value, currentBitIndex));

                    bitsLeft--;
                }
            }
        }

    }
    public class Mmr3Hash : ValueTagInstace<Int32>
    {
        string str_value = "";
        public Mmr3Hash(TagLayouts.C tagDef, long addressStart, long offset) : base(tagDef, addressStart, offset)
        {
        }

        public string Str_value { get => str_value; set => str_value = value; }

        public override void ReadIn(BinaryReader f, TagHeader? header = null)
        {
            base.ReadIn(f, header);
            value = f.ReadInt32();
            if (Mmr3HashLTU.ForceFillData)
            {
                Mmr3HashLTU.AddUniqueIntHash(value);
            }
            if (!Mmr3HashLTU.Mmr3lTU.TryGetValue(value, out str_value)) {
                str_value = Mmr3HashLTU.getMmr3HashFromInt(value);
            }
            
            ExeTagInstance();
        }
    }
    public class RGB : AtomicTagInstace
    {
        float r_value = -1;
        float g_value = -1;
        float b_value = -1;
        public RGB(TagLayouts.C tagDef, long addressStart, long offset) : base(tagDef, addressStart, offset)
        {
        }

        public override void ReadIn(BinaryReader f, TagHeader? header = null)
        {
            base.ReadIn(f, header);
            f.BaseStream.Seek(addressStart + offset, SeekOrigin.Begin);
            r_value = f.ReadSingle();
            g_value = f.ReadSingle();
            b_value = f.ReadSingle();
            ExeTagInstance();
        }

        public override object AccessValue => new {R_value = r_value, G_value = g_value, B_value = b_value };

        public float R_value { get => r_value; set => r_value = value; }
        public float B_value { get => b_value; set => b_value = value; }
        public float G_value { get => g_value; set => g_value = value; }
    }
    public class ARGB : AtomicTagInstace
    {
        float a_value = -1;
        float r_value = -1;
        float g_value = -1;
        float b_value = -1;
        public ARGB(TagLayouts.C tagDef, long addressStart, long offset) : base(tagDef, addressStart, offset)
        {
        }

        public override void ReadIn(BinaryReader f, TagHeader? header = null)
        {
            base.ReadIn(f, header);
            f.BaseStream.Seek(addressStart + offset, SeekOrigin.Begin);
            a_value = f.ReadSingle();
            r_value = f.ReadSingle();
            g_value = f.ReadSingle();
            b_value = f.ReadSingle();
            ExeTagInstance();
        }
        
        public override object AccessValue => new { A_value = a_value, R_value = r_value, G_value = g_value, B_value = b_value };

        public float A_value { get => a_value; set => a_value = value; }
        public float R_value { get => r_value; set => r_value = value; }
        public float G_value { get => g_value; set => g_value = value; }
        public float B_value { get => b_value; set => b_value = value; }
    }
    public class BoundsFloat : AtomicTagInstace
    {
        float min = -1;
        float max = -1;
        public BoundsFloat(TagLayouts.C tagDef, long addressStart, long offset) : base(tagDef, addressStart, offset)
        {
        }

        public override void ReadIn(BinaryReader f, TagHeader? header = null)
        {
            base.ReadIn(f, header);
            f.BaseStream.Seek(addressStart + offset, SeekOrigin.Begin);
            min = f.ReadSingle();
            max = f.ReadSingle();
            ExeTagInstance();
        }
        
        public override object AccessValue => new { Min = min, Max = max };
    }
    public class Bounds2Byte : AtomicTagInstace
    {
        Int16 min = -1;
        Int16 max = -1;
        public Bounds2Byte(TagLayouts.C tagDef, long addressStart, long offset) : base(tagDef, addressStart, offset)
        {
        }

        public override void ReadIn(BinaryReader f, TagHeader? header = null)
        {
            base.ReadIn(f, header);
            f.BaseStream.Seek(addressStart + offset, SeekOrigin.Begin);
            min = f.ReadInt16();
            max = f.ReadInt16();
            ExeTagInstance();
        }
        
        public override object AccessValue => new { Min = min, Max = max };
    }
    public class Point2DFloat : AtomicTagInstace
    {
        float x = -1;
        float y = -1;
        public Point2DFloat(TagLayouts.C tagDef, long addressStart, long offset) : base(tagDef, addressStart, offset)
        {
        }

        public override void ReadIn(BinaryReader f, TagHeader? header = null)
        {
            base.ReadIn(f, header);
            f.BaseStream.Seek(addressStart + offset, SeekOrigin.Begin);
            x = f.ReadSingle();
            y = f.ReadSingle();
            ExeTagInstance();
        }
        
        public override object AccessValue => new { X = x, Y = y };

        public float X { get => x; set => x = value; }
        public float Y { get => y; set => y = value; }
    }
    public class Point2D2Byte : AtomicTagInstace
    {
        Int16 x = -1;
        Int16 y = -1;
        public Point2D2Byte(TagLayouts.C tagDef, long addressStart, long offset) : base(tagDef, addressStart, offset)
        {
        }

        public override void ReadIn(BinaryReader f, TagHeader? header = null)
        {
            base.ReadIn(f, header);
            f.BaseStream.Seek(addressStart + offset, SeekOrigin.Begin);
            x = f.ReadInt16();
            y = f.ReadInt16();
            ExeTagInstance();
        }

        public override object AccessValue => new { X = x, Y = y };
    }
    public class Point3D : AtomicTagInstace
    {
        float x = -1;
        float y = -1;
        float z = -1;
        public Point3D(TagLayouts.C tagDef, long addressStart, long offset) : base(tagDef, addressStart, offset)
        {
        }

        public override void ReadIn(BinaryReader f, TagHeader? header = null)
        {
            base.ReadIn(f, header);
            f.BaseStream.Seek(addressStart + offset, SeekOrigin.Begin);
            x = f.ReadSingle();
            y = f.ReadSingle();
            z = f.ReadSingle();
            ExeTagInstance();
        }

        public override object AccessValue => new { X = x, Y = y, Z = z};

        public float X { get => x; set => x = value; }
        public float Y { get => y; set => y = value; }
        public float Z { get => z; set => z = value; }
    }
    public class Plane3D : AtomicTagInstace
    {
        float x = -1;
        float y = -1;
        float z = -1;
        float point = -1;

        public Plane3D(TagLayouts.C tagDef, long addressStart, long offset) : base(tagDef, addressStart, offset)
        {
        }
        public override void ReadIn(BinaryReader f, TagHeader? header = null)
        {
            base.ReadIn(f, header);
            f.BaseStream.Seek(addressStart + offset, SeekOrigin.Begin);
            x = f.ReadSingle();
            y = f.ReadSingle();
            z = f.ReadSingle();
            point = f.ReadSingle();
            ExeTagInstance();
        }

        public override object AccessValue => new { X = x, Y = y, Z = z, Point = point };
    }
    public class Quaternion : AtomicTagInstace
    {
        float x = -1;
        float y = -1;
        float z = -1;
        float w = -1;
        public Quaternion(TagLayouts.C tagDef, long addressStart, long offset) : base(tagDef, addressStart, offset)
        {
        }

        public override object AccessValue => new { X =x,Y = y, Z=z, W= w};

        public float X { get => x; set => x = value; }
        public float Y { get => y; set => y = value; }
        public float Z { get => z; set => z = value; }
        public float W { get => w; set => w = value; }

        public override void ReadIn(BinaryReader f, TagHeader? header = null)
        {
            base.ReadIn(f, header);
            f.BaseStream.Seek(addressStart + offset, SeekOrigin.Begin);
            x = f.ReadSingle();
            y = f.ReadSingle();
            z = f.ReadSingle();
            w = f.ReadSingle();
            ExeTagInstance();
        }
        
    }
    public class FUNCTION : ValueTagInstace<int>
    {
        private ulong functAddress;
        private ulong functAddress_2;
        private int byteOffset;
        private int byteLengthCount;
        private byte _1st_byte;
        private byte _2nd_byte;
        private byte _3rd_byte;
        private byte _4th_byte;
        private float min_float;
        private float max_float;
        private float unknown1;
        private float unknown2;
        private float unk_min;
        private float unk_max;
        private int leftover_bytes;
        private byte[] curvature_bytes;
        BinaryReader _f;
        public FUNCTION(TagLayouts.C tagDef, long addressStart, long offset) : base(tagDef, addressStart, offset)
        {
        }

        public DataReference Data_reference { get; set; }

        public override object AccessValue => byteLengthCount;

        public override void ReadIn(BinaryReader f, TagHeader? header = null)
        {
            base.ReadIn(f, header);
            //f.BaseStream.Seek(addressStart + offset, SeekOrigin.Begin);
            functAddress = f.ReadUInt64();
            functAddress_2 = f.ReadUInt64();
            byteOffset = f.ReadInt32();
            byteLengthCount = f.ReadInt32();
            //Debug.Assert(functAddress == Constants.ADDRESS_UNSET || functAddress == 0);  
            /*
            if (functAddress != Constants.ADDRESS_UNSET && functAddress != 0) // && f.BaseStream.Position + functAddress < 
            {
                f.BaseStream.Seek((long)functAddress, SeekOrigin.Begin);
                _1st_byte = f.ReadByte();
                _2nd_byte = f.ReadByte();
                _3rd_byte = f.ReadByte();
                _4th_byte = f.ReadByte();
                min_float = f.ReadSingle();
                max_float = f.ReadSingle();
                unknown1 = f.ReadSingle();
                unknown2 = f.ReadSingle();
                unk_min = f.ReadSingle();
                unk_max = f.ReadSingle();
                leftover_bytes = f.ReadInt32();
                if (leftover_bytes > 0)
                    curvature_bytes = f.ReadBytes(leftover_bytes);

            }*/
            _f = f;
            ExeTagInstance();
        }

        public byte[] ReadBuffer()
        {
            if (byteLengthCount != 0 && Data_reference != null && Data_reference.Field_data_block == null)
            {
            }
            if (byteLengthCount != 0 && Data_reference != null)
            {
                var temp = Data_reference.readBinData(byteOffset, byteLengthCount);

                return temp;
            }
            return new byte[0];
        }
    }

    #endregion

    #region Compound
    public abstract class CompoundTagInstance : TagInstance
    {
        protected CompoundTagInstance(TagLayouts.C tagDef, long addressStart, long offset) : base(tagDef, addressStart, offset)
        {
        }

        public abstract void AddChild(TagInstance item);

        public abstract List<TagInstance>? Childrens { get ;  }


    }
    public class ParentTagInstance : CompoundTagInstance, IDictionary<string, TagInstance>
    {
        protected Dictionary<string, TagInstance> keyValues;

        protected string item_name = "";
        protected string item_type = "";

        public override object AccessValue => getAccessValues();
        public override object AccessValueExtra => getAccessValuesExtra();

        public ICollection<string> Keys => ((IDictionary<string, TagInstance>)keyValues).Keys;

        public ICollection<TagInstance> Values => ((IDictionary<string, TagInstance>)keyValues).Values;

        public int Count => ((ICollection<KeyValuePair<string, TagInstance>>)keyValues).Count;

        public bool IsReadOnly => ((ICollection<KeyValuePair<string, TagInstance>>)keyValues).IsReadOnly;

        public override List<TagInstance>? Childrens => new List<TagInstance>(Values);

        public override TagInstance this[string path] => keyValues[path];

        TagInstance IDictionary<string, TagInstance>.this[string key] { get => ((IDictionary<string, TagInstance>)keyValues)[key]; set => ((IDictionary<string, TagInstance>)keyValues)[key] = value; }

        public ParentTagInstance(TagLayouts.C tagDef, long addressStart, long offset) : base(tagDef, addressStart, offset)
        {
        }

        

        Dictionary<string, object> getAccessValues() {
            var result = new Dictionary<string, object>();
            foreach (var key in keyValues.Keys) {
                result[key] = keyValues[key].AccessValue;
            }
            return result;  
        }
        Dictionary<string, object> getAccessValuesExtra() {
            var result = new Dictionary<string, object>();
            foreach (var key in keyValues.Keys) {
                result[key] = keyValues[key].AccessValueExtra;
            }
            return result;  
        }



        public override void ReadIn(BinaryReader f, TagHeader? header = null)
        {
            base.ReadIn(f, header);
            //readTagBloks(f, header);
        }

        public List<ListTagInstance> readTagBloks(BinaryReader f, ref RefItCount refItCount, TagHeader? header)
        {
            if (refItCount == null)
                refItCount = new RefItCount();
            List<ListTagInstance> tagBlocks = new List<ListTagInstance>();
            f.BaseStream.Seek(addressStart + offset, SeekOrigin.Begin);
            foreach (var entry in TagDef.B.Keys)
            {
                TagInstance temp = TagInstanceFactory.Create(TagDef.B[entry], addressStart, entry + offset);
                temp.Parent = this;
                temp.Content_entry = Content_entry;
                temp.ReadIn(f, header);




                switch (TagDef.B[entry].T)
                {
                    case TagElemntType.FUNCTION:
                        if (Content_entry != null)
                            (temp as FUNCTION).Data_reference = Content_entry.L_function[refItCount.f];
                        else
                        {
                        }
                        refItCount.f += 1;
                        break;
                    /*
                    temp.data_reference = parent.content_entry.l_function[RefItCount['f']]
                    if temp.data_reference.unknown_property != 0:
                        debug = 0

                    assert self.full_header.file_header.data_reference_count != 0
                    assert len(parent.content_entry.l_function[RefItCount['f']].bin_data) == tagInstanceTemp[
                        key].byteLengthCount

                    self.hasFunction += 1
                    RefItCount['f'] += 1
                    self.OnInstanceLoad(temp)
                     */
                    case TagElemntType.TagRef:
                        if (Content_entry != null)
                        {
                            (temp as TagRef).Tag_ref = this.Content_entry.L_tag_ref[refItCount.r];
                            (temp as TagRef).loadPath();
                        }
                        refItCount.r += 1;
                        //self.OnInstanceLoad(temp)
                        break;
                    case TagElemntType.TagStructData:
                        if (Content_entry != null && Content_entry.Childs.Count > refItCount.i)
                        {
                            var temp_entry = this.Content_entry.Childs[refItCount.i];
                            /*
                             assert not (temp_entry.type_id_tg == TagStructType.NoDataStartBlock and len(
                            temp_entry.bin_datas) != 0), \
                            f'Error in {self.filename}'
                             */

                            if (this.Content_entry.Childs[refItCount.i].TypeIdTg == TagStructType.NoDataStartBlock)
                            {
                                tagBlocks.Add(temp as ListTagInstance);
                                refItCount.i += 1;

                            }
                            else
                            {
                                //self.OnInstanceLoad(temp)
                            }

                        }
                        break;
                    case TagElemntType.Tagblock:
                        tagBlocks.Add(temp as Tagblock);
                        refItCount.i += 1;
                        break;
                    case TagElemntType.ResourceHandle:
                        tagBlocks.Add(temp as ResourceHandle);
                        refItCount.i += 1;
                        break;
                    default:
                        temp.Parent = this;
                        //self.OnInstanceLoad(tagInstanceTemp[key])
                        break;
                }
                if (temp.GetType().IsSubclassOf(typeof(ParentTagInstance)))
                {
                    //n_items = (temp as ParentTagInstance).TagBlocks.Count;
                    tagBlocks.AddRange((temp as ParentTagInstance).readTagBloks(f, ref refItCount, header));
                }
                AddChild(temp);
            }
            return tagBlocks;
        }


        public override void AddChild(TagInstance tagInstance)
        {
            if (keyValues == null)
                keyValues = new Dictionary<string, TagInstance>();
            string key = tagInstance.TagDef.N;
            string item_key = tagInstance.TagDef.xmlPath.Item2;
            string parent_key = tagInstance.Parent == null? "#document\\" : tagInstance.Parent.TagDef.xmlPath.Item2;
            string sub_key = item_key.Replace(parent_key, "");
            if (key == "")
                key = sub_key;
            
            key = key.Replace(@"\_", "T_");
            while (keyValues.ContainsKey(key))
            {
                key = key + "_";
            }

            tagInstance.FieldName = key;
            keyValues[key] = tagInstance;
        }

        public override TagInstance GetObjByPath(string path)
        {
            if (!string.IsNullOrEmpty(path)) { 
                var a_split = path.Split('.');
                if (keyValues.ContainsKey(a_split[0])) {
                    if (a_split.Length == 1) {
                        var r = keyValues[a_split[0]];
                        if (r as AtomicTagInstace == null)
                        {
                            return r;
                        }
                    }
                        
                    string sub_path = string.Join('.', a_split.Skip(1));
                    return keyValues[a_split[0]].GetObjByPath(sub_path);
                }
                    
            }    
            return null;
        }

        public void Add(string key, TagInstance value)
        {
            ((IDictionary<string, TagInstance>)keyValues).Add(key, value);
        }

        public bool ContainsKey(string key)
        {
            return ((IDictionary<string, TagInstance>)keyValues).ContainsKey(key);
        }

        public bool Remove(string key)
        {
            return ((IDictionary<string, TagInstance>)keyValues).Remove(key);
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out TagInstance value)
        {
            return ((IDictionary<string, TagInstance>)keyValues).TryGetValue(key, out value);
        }

        public void Add(KeyValuePair<string, TagInstance> item)
        {
            ((ICollection<KeyValuePair<string, TagInstance>>)keyValues).Add(item);
        }

        public void Clear()
        {
            ((ICollection<KeyValuePair<string, TagInstance>>)keyValues).Clear();
        }

        public bool Contains(KeyValuePair<string, TagInstance> item)
        {
            return ((ICollection<KeyValuePair<string, TagInstance>>)keyValues).Contains(item);
        }

        public void CopyTo(KeyValuePair<string, TagInstance>[] array, int arrayIndex)
        {
            ((ICollection<KeyValuePair<string, TagInstance>>)keyValues).CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<string, TagInstance> item)
        {
            return ((ICollection<KeyValuePair<string, TagInstance>>)keyValues).Remove(item);
        }

        public IEnumerator<KeyValuePair<string, TagInstance>> GetEnumerator()
        {
            return ((IEnumerable<KeyValuePair<string, TagInstance>>)keyValues).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)keyValues).GetEnumerator();
        }

        
    }
    public class RootTagInstance : ParentTagInstance
    {

        public RootTagInstance(TagLayouts.C tagDef, long addressStart, int offset) : base(tagDef, addressStart, offset)
        {
            FieldName = "Root";
        }



        public void ReadInSave(TagHeader? header = null)
        {
            MemoryStream bin_stream = new MemoryStream(Content_entry.Bin_datas[0].ToArray<byte>());
            BinaryReader f = new BinaryReader(bin_stream);
            base.ReadIn(f, header);

            RefItCount refItCount = new RefItCount();
            List<ListTagInstance> tagBlocks = readTagBloks(f, ref refItCount, header);
            int n_items = tagBlocks.Count;
            if (Content_entry == null)
                return;
            Debug.Assert(Content_entry.Childs.Count == n_items);
            for (int i = 0; i < Content_entry.Childs.Count; i++)
            {
                var entry = Content_entry.Childs[i];
                var tag_child_inst = tagBlocks[i];
                if (tag_child_inst == null)
                    continue;
                if (tag_child_inst.GetType() == typeof(TagStructData))
                {
                    /*
                     assert entry.type_id_tg == TagStructType.NoDataStartBlock, 'Coinciden en tipo NoDataStartBlock'
                assert len(
                    entry.bin_datas) == 0, f'Error in {self.filename},  {instance_parent.tagDef.N}, {tag_child_inst.tagDef.N}'
            
                     */
                }
                else if (tag_child_inst.GetType() == typeof(ResourceHandle))
                {
                    //assert entry.type_id_tg == TagStructType.ResourceHandle or entry.type_id_tg == TagStructType.ExternalFileDescriptor, f'Coinciden en tipo ResourceHandle in {self.filename},  {instance_parent.tagDef.N}'
                    if (entry.TypeIdTg == TagStructType.ResourceHandle)
                    {

                    }
                }
                else
                {

                }
                tag_child_inst.Content_entry = entry;
                tag_child_inst.Parent = this;
                if (tag_child_inst.GetType().IsSubclassOf(typeof(ListTagInstance))) {
                    if (tag_child_inst as ArrayFixLen != null)
                    { 
                    }
                    ((ListTagInstance)tag_child_inst).FillChilds(ref refItCount);
                }
                
                else
                {

                }

            }
            ExeTagInstance();
        }
    }
    public class RenderGeometryTag : TagStructData
    {
        public RenderGeometryTag(TagLayouts.C tagDef, long addressStart, long offset) : base(tagDef, addressStart, offset)
        {
        }
    }
    public class TagStructData : ParentTagInstance
    {
        //bool generateEntry = false;
        //string comment = "";
        public TagStructData(TagLayouts.C tagDef, long addressStart, long offset) : base(tagDef, addressStart, offset)
        {
            //  generateEntry = (bool)tagDef.P["generateEntry"];
        }

        public override void ReadIn(BinaryReader f, TagHeader? header = null)
        {
            base.ReadIn(f, header);
            ExeTagInstance();

        }

    }
    public class ListTagInstance : CompoundTagInstance, IList<TagInstance>
    {
        protected List<TagInstance> childs = new List<TagInstance>();
        protected int childrenCount = 0;

        public ListTagInstance(TagLayouts.C tagDef, long addressStart, long offset) : base(tagDef, addressStart, offset)
        {
        }

        public List<TagInstance> Childs { get => childs; set => childs = value; }
        

        public override object AccessValue => getAccessValues();
        public override object AccessValueExtra => getAccessValuesExtras();

        public int Count => ((ICollection<TagInstance>)childs).Count;

        public bool IsReadOnly => ((ICollection<TagInstance>)childs).IsReadOnly;

        public int ChildrenCount { get => childrenCount; set => childrenCount = value; }

        public TagInstance this[int index] { get => ((IList<TagInstance>)childs)[index]; set => ((IList<TagInstance>)childs)[index] = value; }
        public override List<TagInstance>? Childrens
        {
            get
            {
                return childs;
            }
        }

        List<object> getAccessValues()
        {
            var result = new List<object>();
            foreach (var key in childs)
            {
                result.Add(key.AccessValue);
            }
            return result;
        }
        List<object> getAccessValuesExtras()
        {
            var result = new List<object>();
            foreach (var key in childs)
            {
                result.Add(key.AccessValueExtra);
            }
            return result;
        }

        public override TagInstance GetObjByPath(string path)
        {
            if (!string.IsNullOrEmpty(path) && (path.Contains('.') || path.StartsWith("[")))
            {
                var a_split = path.Split('.');
                if (a_split[0].StartsWith("[") && a_split[0].EndsWith("]") && a_split[0].Length > 2)
                {
                    int index = int.Parse(a_split[0].Replace("[", "").Replace("]", ""));
                    if (index >= 0 && index < childs.Count) {
                        string sub_path = string.Join('.', a_split.Skip(1));
                        if (a_split.Length == 1)
                            return childs[index];
                        return childs[index].GetObjByPath(sub_path);
                    }
                        
                }
                
            }
            return null;
        }

        public virtual void FillChilds(ref RefItCount refItCount, TagHeader? header = null)
        {
            if (childrenCount != Content_entry.Bin_datas.Count)
            {
                if (Content_entry.Field_data_block != null)
                {
                }
            }
            if (TagDef.T == TagElemntType.ResourceHandle)
            {
            }
            //Debug.Assert(childrenCount == Content_entry.Bin_datas.Count);
            var temp_offset = 0;
            List<TagInstance> tagBlocks = new List<TagInstance>();

            var tempRef = new RefItCount();
            for (int i = 0; i < Content_entry.Bin_datas.Count; i++)
            {
                MemoryStream bin_stream = new MemoryStream(Content_entry.Bin_datas[i].ToArray<byte>());
                ParentTagInstance temp = new ParentTagInstance(TagDef, 0, temp_offset);
                var f = new BinaryReader(bin_stream);
                temp.Content_entry = Content_entry;
                temp.ReadIn(f, header);
                childs.Add(temp);
                tagBlocks.AddRange(temp.readTagBloks(f, ref tempRef, header));
            }
            Debug.Assert(tagBlocks.Count == Content_entry.Childs.Count);
            for (int i = 0; i < Content_entry.Childs.Count; i++)
            {
                var entry = Content_entry.Childs[i];
                var tag_child_inst = tagBlocks[i];
                if (tag_child_inst == null)
                    continue;
                if (tag_child_inst.GetType() == typeof(TagStructData))
                {
                    /*
                     assert entry.type_id_tg == TagStructType.NoDataStartBlock, 'Coinciden en tipo NoDataStartBlock'
                assert len(
                    entry.bin_datas) == 0, f'Error in {self.filename},  {instance_parent.tagDef.N}, {tag_child_inst.tagDef.N}'
            
                     */
                }
                else if (tag_child_inst.GetType() == typeof(ResourceHandle))
                {
                    //assert entry.type_id_tg == TagStructType.ResourceHandle or entry.type_id_tg == TagStructType.ExternalFileDescriptor, f'Coinciden en tipo ResourceHandle in {self.filename},  {instance_parent.tagDef.N}'
                    if (entry.TypeIdTg == TagStructType.ResourceHandle)
                    {

                    }
                }
                else
                {

                }
                tag_child_inst.Content_entry = entry;
                tag_child_inst.Parent = this;
                if (tag_child_inst.GetType().IsSubclassOf(typeof(ListTagInstance)))
                    (tag_child_inst as ListTagInstance).FillChilds(ref refItCount);
                else
                {

                }
            }
        }

        public int IndexOf(TagInstance item)
        {
            return ((IList<TagInstance>)childs).IndexOf(item);
        }

        public void Insert(int index, TagInstance item)
        {
            ((IList<TagInstance>)childs).Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            ((IList<TagInstance>)childs).RemoveAt(index);
        }

        public void Add(TagInstance item)
        {
            ((ICollection<TagInstance>)childs).Add(item);
        }

        public void Clear()
        {
            ((ICollection<TagInstance>)childs).Clear();
        }

        public bool Contains(TagInstance item)
        {
            return ((ICollection<TagInstance>)childs).Contains(item);
        }

        public void CopyTo(TagInstance[] array, int arrayIndex)
        {
            ((ICollection<TagInstance>)childs).CopyTo(array, arrayIndex);
        }

        public bool Remove(TagInstance item)
        {
            return ((ICollection<TagInstance>)childs).Remove(item);
        }

        public IEnumerator<TagInstance> GetEnumerator()
        {
            return ((IEnumerable<TagInstance>)childs).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)childs).GetEnumerator();
        }

        public override void AddChild(TagInstance item)
        {
            item.FieldName = "[" + childs.Count.ToString() + "]";
            Add(item);
        }
    }

    public class ExternalFileDescriptor : ResourceHandle
    {

        private long newAddress;
        private int int_value;
        private string str_value;

        public ExternalFileDescriptor(TagLayouts.C tagDef, long addressStart, long offset) : base(tagDef, addressStart, offset)
        {
        }

        public long NewAddress { get => newAddress; set => newAddress = value; }

        public override void FillChilds(ref RefItCount refItCount, TagHeader? header = null)
        {
            base.FillChilds(ref refItCount, header);
        }

        public override void ReadIn(BinaryReader f, TagHeader? header = null)
        {
            base.ReadIn(f, header);
            f.BaseStream.Seek(addressStart + offset, SeekOrigin.Begin);
            newAddress = f.ReadInt64();
            int_value = f.ReadInt32();
            str_value = Mmr3HashLTU.getMmr3HashFromInt(int_value);
            childrenCount = f.ReadInt32();
            ExeTagInstance();
        }


    }
    public class ResourceHandle : ListTagInstance
    {

        private long newAddress;
        private int int_value;
        private string str_value;

        public ResourceHandle(TagLayouts.C tagDef, long addressStart, long offset) : base(tagDef, addressStart, offset)
        {
        }

        public long NewAddress { get => newAddress; set => newAddress = value; }

        public override void FillChilds(ref RefItCount refItCount, TagHeader? header = null)
        {
            base.FillChilds(ref refItCount, header);
        }

        public override void ReadIn(BinaryReader f, TagHeader? header = null)
        {
            base.ReadIn(f, header);
            f.BaseStream.Seek(addressStart + offset, SeekOrigin.Begin);
            newAddress = f.ReadInt64();
            int_value = f.ReadInt32();
            str_value = Mmr3HashLTU.getMmr3HashFromInt(int_value);
            childrenCount = f.ReadInt32();
            ExeTagInstance();
        }


    }
    public class Tagblock : ListTagInstance
    {
        private long newAddress;
        private long stringAddress;

        public Tagblock(TagLayouts.C tagDef, long addressStart, long offset) : base(tagDef, addressStart, offset)
        {
        }

        public long NewAddress { get => newAddress; set => newAddress = value; }
        public long StringAddress { get => stringAddress; set => stringAddress = value; }

        public override void FillChilds(ref RefItCount refItCount, TagHeader? header = null)
        {
            base.FillChilds(ref refItCount, header);
        }
        public override void ReadIn(BinaryReader f, TagHeader? header = null)
        {
            base.ReadIn(f, header);
            f.BaseStream.Seek(addressStart + offset, SeekOrigin.Begin);
            newAddress = f.ReadInt64();
            stringAddress = f.ReadInt64();
            childrenCount = f.ReadInt32();
            ExeTagInstance();
        }


    }


    public class ArrayFixLen : ListTagInstance
    {
        string comment = "";

        public ArrayFixLen(TagLayouts.C tagDef, long addressStart, long offset) : base(tagDef, addressStart, offset)
        {
        }
        public override void FillChilds(ref RefItCount refItCount, TagHeader? header = null)
        {
            
        }

        public override void ReadIn(BinaryReader f, TagHeader? header = null)
        {
            base.ReadIn(f, header);
            comment = TagDef.N;          
            ExeTagInstance();
        }

    }

    #endregion
    public class TagStructBlock : TagInstance
    {
        //TODO No se usa

        string comment = "";
        Dictionary<string, TagInstance> keyValues = new Dictionary<string, TagInstance>();
        public TagStructBlock(TagLayouts.C tagDef, long addressStart, long offset) : base(tagDef, addressStart, offset)
        {
            throw new Exception("REvisar el uso");
        }
    }

    public static class TagInstanceFactory
    {
        public static TagInstance Create(TagLayouts.C tagDef, long addressStart, long offset)
        {
            switch (tagDef.T)
            {
                case TagElemntType.Comment:
                    return new Comment(tagDef, addressStart, offset);
                case TagElemntType.ArrayFixLen:
                    return new ArrayFixLen(tagDef, addressStart, offset);
                case TagElemntType.GenericBlock:
                    return new GenericBlock(tagDef, addressStart, offset);
                case TagElemntType.TagStructData:
                    if (tagDef.E != null && tagDef.E.ContainsKey("hash") && tagDef.E["hash"].ToString() == "E423D497BA42B08FA925E0B06C3C363A")
                        return new RenderGeometryTag(tagDef, addressStart, offset);
                    return new TagStructData(tagDef, addressStart, offset);
                case TagElemntType.FUNCTION:
                    return new FUNCTION(tagDef, addressStart, offset);
                case TagElemntType.EnumGroup:
                    return new EnumGroup(tagDef, addressStart, offset);
                case TagElemntType.FourByte:
                    return new FourByte(tagDef, addressStart, offset);
                case TagElemntType.TwoByte:
                    return new TwoByte(tagDef, addressStart, offset);
                case TagElemntType.UTwoByte:
                    return new UTwoByte(tagDef, addressStart, offset);
                case TagElemntType.Byte:
                    return new Byte(tagDef, addressStart, offset);
                case TagElemntType.Float:
                    return new Float(tagDef, addressStart, offset);
                case TagElemntType.TagRef:
                    return new TagRef(tagDef, addressStart, offset);
                case TagElemntType.Pointer:
                    return new Pointer(tagDef, addressStart, offset);
                case TagElemntType.Tagblock:
                    return new Tagblock(tagDef, addressStart, offset);
                case TagElemntType.ResourceHandle:
                    return new ResourceHandle(tagDef, addressStart, offset);
                case TagElemntType.TagStructBlock:
                    return new TagStructBlock(tagDef, addressStart, offset);
                case TagElemntType.String:
                    return new String(tagDef, addressStart, offset);
                case TagElemntType.StringTag:
                    return new StringTag(tagDef, addressStart, offset);
                case TagElemntType.Flags:
                    return new Flags(tagDef, addressStart, offset);
                case TagElemntType.FlagGroup:
                    return new FlagGroup(tagDef, addressStart, offset);
                case TagElemntType.Mmr3Hash:
                    return new Mmr3Hash(tagDef, addressStart, offset);
                case TagElemntType.RGB:
                    return new RGB(tagDef, addressStart, offset);
                case TagElemntType.ARGB:
                    return new ARGB(tagDef, addressStart, offset);
                case TagElemntType.Bounds2Byte:
                    return new Bounds2Byte(tagDef, addressStart, offset);
                case TagElemntType.BoundsFloat:
                    return new BoundsFloat(tagDef, addressStart, offset);
                case TagElemntType.Point2D2Byte:
                    return new Point2D2Byte(tagDef, addressStart, offset);
                case TagElemntType.Point2DFloat:
                    return new Point2DFloat(tagDef, addressStart, offset);
                case TagElemntType.Point3D:
                    return new Point3D(tagDef, addressStart, offset);
                case TagElemntType.Quaternion:
                    return new Quaternion(tagDef, addressStart, offset);
                case TagElemntType.Plane3D:
                    return new Plane3D(tagDef, addressStart, offset);

                default:
                    return new TagInstance(tagDef, addressStart, offset);
            }
        }
    }
}
