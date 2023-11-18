using LibHIRT.Common;
using LibHIRT.TagReader.Headers;
using LibHIRT.Utils;
using Newtonsoft.Json;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Text.Json.Serialization;
using TagStruct = LibHIRT.TagReader.Headers.TagStruct;

namespace LibHIRT.TagReader
{
    public class RefItCount
    {
        public int i = 0;
        public int f = 0;
        public int r = 0;
    }
   
    public class TagInstance : ITagInstance, INotifyPropertyChanged, IDisposable
    {

        protected Template tagDef;
        protected long addressStart;
        protected long offset;
        protected long inFileOffset=-1;
        protected HeaderTableEntry? entry = null;
        protected TagStruct? content_entry = null;

        protected TagInstance? parent = null;
        protected long inst_parent_offset = -1;
        protected long inst_address = -1;
        private long inst_global_address = -1;
        protected Dictionary<string, Object> extra_data = new Dictionary<string, object>();
        private bool _noAllowEdit = true;

        public static event EventHandler<ITagInstance> OnInstanceLoadEvent;
        public event PropertyChangedEventHandler? PropertyChanged;

        public TagInstance(Template tagDef, long addressStart, long offset)
        {
            this.tagDef = tagDef;
            this.addressStart = addressStart;
            this.offset = offset;
        }
        [JsonInclude]
        public HeaderTableEntry? Entry { get => entry; set => entry = value; }
        [JsonInclude]
        public Template TagDef { get => tagDef; }
        [System.Text.Json.Serialization.JsonIgnore]
        public TagStruct? Content_entry { get => content_entry; set => content_entry = value; }
        [JsonInclude]
        public long Offset { get => offset; set => offset = value; }
        [JsonInclude]
        public TagInstance? Parent { get => parent; set { parent = value; } }

        [JsonInclude]
        public long Inst_parent_offset { get => inst_parent_offset; set => inst_parent_offset = value; }
        [JsonInclude]
        public long GetTagSize => TagDef?.S ?? 0;
        [System.Text.Json.Serialization.JsonIgnore]
        public virtual object AccessValue { get => new { AddressStart = addressStart, Offset = offset }; set {; } }
        [System.Text.Json.Serialization.JsonIgnore]
        public virtual object AccessValueExtra => new
        {
            Value = AccessValue,
            AddressStart = addressStart,
            Offset = offset,
            InstOffset = parent == null ? Offset : parent.Offset + Offset
        };
        [JsonInclude]
        public long Inst_global_address
        {
            get
            {

                return inst_global_address;
            }
        }

        protected virtual long GetInFileOffset()
        {
            return inFileOffset!=-1? inFileOffset:((Content_entry?.Field_data_block?.OffsetPlus ?? 0) + InstanceParentOffset);
        }
        [JsonInclude]
        public string FieldName { get; set; }
        public TagInstance Self => this;
        [JsonInclude]
        public long InFileOffset { get => GetInFileOffset(); set => inFileOffset = value; }
        [JsonInclude]
        public long InstanceParentOffset { get; set; }

        protected long Inst_address { get => inst_address; set => inst_address = value; }
        [JsonInclude]
        public bool NoAllowEdit
        {
            get
            {
                return _noAllowEdit;
            }
            set
            {
                _noAllowEdit = value;
                OnPropertyChanged("NoAllowEdit");
            }
        }



        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public virtual TagInstance GetObjByPath(string path)
        {
            throw new NotImplementedException();
        }

        public virtual TagInstance this[string path]
        {
            get => GetObjByPath(path);
            set => new NotImplementedException();
        }

        protected void ExeTagInstance()
        {
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

        public virtual void WriteIn(Stream f, long offset = -1, TagHeader? header = null)
        {
            throw new NotImplementedException();
        }

        public virtual void Dispose()
        {
            ;
        }

        public virtual byte[] GetBytes()
        {
            throw new NotImplementedException();
        }
    }
    #region Atomic
    public class AtomicTagInstace : TagInstance
    {
        public AtomicTagInstace(Template tagDef, long addressStart, long offset) : base(tagDef, addressStart, offset)
        {
        }

        public override TagInstance GetObjByPath(string path)
        {
            return this;
        }

        public override object AccessValue { get => base.AccessValue; set => base.AccessValue = value; }

    }

    public class ValueTagInstace<T> : AtomicTagInstace
    {
        protected T value;

        protected Stack<object> stackChange = new Stack<object>();

        public ValueTagInstace(Template tagDef, long addressStart, long offset) : base(tagDef, addressStart, offset)
        {
        }

        public override void ReadIn(BinaryReader f, TagHeader? header = null)
        {
            base.ReadIn(f, header);
            f.BaseStream.Seek(addressStart + offset, SeekOrigin.Begin);
        }

        public override void WriteIn(Stream f, long offset = -1, TagHeader? header = null)
        {
            var toReturn = f.Position;
            f.Seek(offset == -1 ? InFileOffset : offset, SeekOrigin.Begin);
            f.Write(GetBytes());
            f.Seek(toReturn, SeekOrigin.Begin);
        }


        public T Value { get => value; set => this.value = value; }

        public override object AccessValue
        {
            get { return stackChange.Count == 0 ? Value : stackChange.Peek(); }
            set
            {
                if (Value is string) {
                    this.value = (T?)value;
                } else {
                    var stringParseMeth = typeof(T).GetMethod("Parse", new Type[] { typeof(string) });
                    if (stringParseMeth!=null)
                        this.value = (T?)stringParseMeth.Invoke(this.value, new object[] { value.ToString() });
                }
                
                stackChange.Push(value);
            }
        }


    }



    public class DebugDataBlock : ValueTagInstace<string>
    {

        public DebugDataBlock(Template tagDef, long addressStart, long offset) : base(tagDef, addressStart, offset)
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

        public Comment(Template tagDef, long addressStart, long offset) : base(tagDef, addressStart, offset)
        {
        }

        public override void ReadIn(BinaryReader f, TagHeader? header = null)
        {
            base.ReadIn(f, header);
            //value = TagDef.N;
            ExeTagInstance();
        }
    }
    public class Explanation : AtomicTagInstace
    {

        public Explanation(Template tagDef, long addressStart, long offset) : base(tagDef, addressStart, offset)
        {
        }

        public override void ReadIn(BinaryReader f, TagHeader? header = null)
        {
            base.ReadIn(f, header);
            //value = TagDef.N;
            ExeTagInstance();
        }
    }
    public class CustomLikeGrouping : AtomicTagInstace
    {

        public CustomLikeGrouping(Template tagDef, long addressStart, long offset) : base(tagDef, addressStart, offset)
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
        public GenericBlock(Template tagDef, long addressStart, long offset) : base(tagDef, addressStart, offset)
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
        public EnumGroup(Template tagDef, long addressStart, long offset) : base(tagDef, addressStart, offset)
        {
        }

        public override object AccessValue => new { Selected = selected, Options = options };

        public List<string> Options { get => options; }
        public int SelectedIndex { get => _selectedIndex; }
        public string Selected { get => selected; }

        public override void ReadIn(BinaryReader f, TagHeader? header = null)
        {
            base.ReadIn(f, header);
            Dictionary<int, string> STR = null;
            int size = -1;
            if (TagDef.GetType() == typeof(TagLayouts.EnumGroupTL))
            {
                TagLayouts.EnumGroupTL tempEnum = (TagDef as TagLayouts.EnumGroupTL);
                STR = tempEnum.STR;
                size = tempEnum.A;
            }
            else if (TagDef.GetType() == typeof(TagLayoutsV2.E))
            {
                TagLayoutsV2.E tempEnum = (TagDef as TagLayoutsV2.E);
                STR = tempEnum.STR;
                size = tempEnum.S;
            }
            else
                return;

            foreach (int gvsdahb in STR.Keys)
            {
                options.Add(STR[gvsdahb]);
            }
            f.BaseStream.Seek(addressStart + offset, SeekOrigin.Begin);
            switch (size)
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

        public override byte[] GetBytes()
        {
            return BitConverter.GetBytes(this.value);
        }
    }
    public class FourByte : ValueTagInstace<Int32>
    {

        public FourByte(Template tagDef, long addressStart, long offset) : base(tagDef, addressStart, offset)
        {
        }

        public override void ReadIn(BinaryReader f, TagHeader? header = null)
        {
            base.ReadIn(f, header);
            value = f.ReadInt32();
            ExeTagInstance();
        }

        public override object AccessValue
        {
            get { return base.AccessValue; }
            set
            {
                base.AccessValue = value;
            }
        }

        public override byte[] GetBytes()
        {
            return BitConverter.GetBytes(this.value);
        }
    }
    public class TwoByte : ValueTagInstace<Int16>
    {
        public TwoByte(Template tagDef, long addressStart, long offset) : base(tagDef, addressStart, offset)
        {
        }

        public override void ReadIn(BinaryReader f, TagHeader? header = null)
        {
            base.ReadIn(f, header);
            value = f.ReadInt16();
            ExeTagInstance();
        }

        public override byte[] GetBytes()
        {
            return BitConverter.GetBytes(this.value);
        }
    }

    public class UTwoByte : ValueTagInstace<UInt16>
    {
        public UTwoByte(Template tagDef, long addressStart, long offset) : base(tagDef, addressStart, offset)
        {
        }

        public override void ReadIn(BinaryReader f, TagHeader? header = null)
        {
            base.ReadIn(f, header);
            value = f.ReadUInt16();
            ExeTagInstance();
        }

        public override byte[] GetBytes()
        {
            return BitConverter.GetBytes(this.value);
        }
    }
    public class Byte : ValueTagInstace<sbyte>
    {
        public Byte(Template tagDef, long addressStart, long offset) : base(tagDef, addressStart, offset)
        {
        }
        public override void ReadIn(BinaryReader f, TagHeader? header = null)
        {
            base.ReadIn(f, header);
            value = f.ReadSByte();
            ExeTagInstance();
        }

        public override byte[] GetBytes()
        {
            return BitConverter.GetBytes(this.value);
        }

    }
    public class Float : ValueTagInstace<float>
    {

        public Float(Template tagDef, long addressStart, long offset) : base(tagDef, addressStart, offset)
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
        public override byte[] GetBytes()
        {
            return BitConverter.GetBytes(this.value);
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
        char[] tag;
        private string ref_id;
        Int32 ref_id_sub_int = -1;
        Int32 ref_id_center_int = -1;

        Int32 local_handle = -1;
        string tagGroup = "";
        string tagGroupRev = "";
        public TagRef(Template tagDef, long addressStart, long offset) : base(tagDef, addressStart, offset)
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
            tag = f.ReadChars(4);
            tagGroup = new string(tag);
            Array.Reverse(tag);// 0x14
            tagGroupRev = new string(tag);
            local_handle = f.ReadInt32();
            ExeTagInstance();
        }

        public override byte[] GetBytes()
        {
            var temp = BitConverter.GetBytes(global_handle).Concat(BitConverter.GetBytes(ref_id_int)).Concat(BitConverter.GetBytes(ref_id_sub_int)).Concat(BitConverter.GetBytes(ref_id_center_int));

            temp = temp.Concat(new byte[4] { ((byte)tag[0]), ((byte)tag[1]), ((byte)tag[2]), ((byte)tag[3]) });
            temp = temp.Concat(BitConverter.GetBytes(local_handle));

            return temp.ToArray();
        }

        public override object AccessValue => new
        {
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
        public int InvalidRef_id_int { get => -1; }
        public int Ref_id_sub_int { get => ref_id_sub_int; set => ref_id_sub_int = value; }
        public int Ref_id_center_int { get => ref_id_center_int; set => ref_id_center_int = value; }
        public string TagGroupRev { get => tagGroupRev; set => tagGroupRev = value; }
        public string Path { get => tag_ref?.StrPath; }
        public string Ref_id { get => ref_id; set => ref_id = value; }
        public int Local_handle { get => local_handle; set => local_handle = value; }
    }
    public class Pointer : ValueTagInstace<Int64>
    {
        public Pointer(Template tagDef, long addressStart, long offset) : base(tagDef, addressStart, offset)
        {
        }

        public override void ReadIn(BinaryReader f, TagHeader? header = null)
        {
            base.ReadIn(f, header);
            value = f.ReadInt64();
            ExeTagInstance();
        }

        public override byte[] GetBytes()
        {
            return BitConverter.GetBytes(this.value);
        }
    }

    public class String : ValueTagInstace<string>
    {
        public String(Template tagDef, long addressStart, long offset) : base(tagDef, addressStart, offset)
        {
        }

        public override void ReadIn(BinaryReader f, TagHeader? header = null)
        {
            base.ReadIn(f, header);
            //value = new string(f.ReadChars((int)TagDef.S));
            value = f.ReadStringNullTerminated((int)TagDef.S);
            if (Mmr3HashLTU.ForceFillData)
            {
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
        public StringTag(Template tagDef, long addressStart, long offset) : base(tagDef, addressStart, offset)
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
        public Flags(Template tagDef, long addressStart, long offset) : base(tagDef, addressStart, offset)
        {
        }

        public override void ReadIn(BinaryReader f, TagHeader? header = null)
        {
            base.ReadIn(f, header);
            value = (f.ReadByte());
            ExeTagInstance();
        }
        public override byte[] GetBytes()
        {
            return BitConverter.GetBytes(this.value);
        }
    }

    public class Flag
    {
        bool _value = false;
        public string Label { get; set; }
        public bool Value
        {
            get { return _value; }
            set
            {
                _value = value;
                Parent?.FlagGroupC?.RegenerateOutPut();
            }

        }

        public FlagsModel Parent { get; set; }
    }

    public class FlagsModel : INotifyPropertyChanged
    {
        private string _value;

        public string Label { get; set; }
        public string Value
        {
            get { return _value; }
            set
            {
                _value = value;
                OnPropertyChanged("IntValue");
            }

        }
        public List<Flag>? Childrens
        {
            get;
            set;
        }

        public FlagGroup FlagGroupC { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

    }
    public class FlagGroup : ValueTagInstace<List<bool>>
    {

        List<string> options = new List<string>();
        List<bool> options_v = new List<bool>();
        List<FlagsModel> _salida = null;
        public override object AccessValue => new { options, options_v };

        public FlagGroup(Template tagDef, long addressStart, long offset) : base(tagDef, addressStart, offset)
        {
        }

        public override void ReadIn(BinaryReader f, TagHeader? header = null)
        {
            base.ReadIn(f, header);

            if (TagDef.GetType() == typeof(TagLayouts.FlagGroupTL))
            {
                TagLayouts.FlagGroupTL tempEnum = (TagDef as TagLayouts.FlagGroupTL);
                generateBits(addressStart + offset, tempEnum.A, tempEnum.MB, tempEnum.STR, f);
                foreach (int gvsdahb in tempEnum.STR.Keys)
                {
                    options.Add(tempEnum.STR[gvsdahb]);
                }
            }
            else if (TagDef.GetType() == typeof(TagLayoutsV2.F)) {
                TagLayoutsV2.F tempEnum = (TagDef as TagLayoutsV2.F);
                generateBits(addressStart + offset, tempEnum.S, 0, tempEnum.STR, f);
                foreach (int gvsdahb in tempEnum.STR.Keys)
                {
                    options.Add(tempEnum.STR[gvsdahb]);
                }
            }

            ExeTagInstance();
        }

        public override void WriteIn(Stream f, long offset = -1, TagHeader? header = null)
        {
            var toReturn = f.Position;
            f.Seek(offset == -1 ? InFileOffset : offset, SeekOrigin.Begin);
            f.Write(UtilBinaryReader.GetBytesFormStringBit(_salida[0].Value));
            f.Seek(toReturn, SeekOrigin.Begin);
        }
        public override byte[] GetBytes()
        {
            return UtilBinaryReader.GetBytesFormStringBit(_salida[0].Value);
        }

        public List<FlagsModel> Flags
        {
            get
            {
                if (_salida != null)
                    return _salida;
                _salida = new List<FlagsModel>();
                _salida.Add(new FlagsModel());
                List<Flag> r_options = new List<Flag>();
                for (int i = 0; i < options.Count; i++)
                {
                    r_options.Add(new Flag()
                    {
                        Label = options[i],
                        Value = options_v[i],
                        Parent = _salida[0]
                    });
                }

                string result = "";
                for (int i = 0; i < options_v.Count; i++)
                {
                    bool check = r_options.Count > i ? r_options[i].Value : options_v[i];
                    if (check)
                    {
                        result = result + "1";
                    }
                    else
                    {
                        result = result + "0";
                    }
                }

                _salida[0].Label = "Bin val";
                _salida[0].Value = result;
                _salida[0].Childrens = r_options;
                _salida[0].FlagGroupC = this;
                return _salida;
            }

        }

        public void RegenerateOutPut()
        {
            if (_salida == null)
                return;
            string result = "";
            for (int i = 0; i < options_v.Count; i++)
            {
                bool check = _salida[0].Childrens.Count > i ? _salida[0].Childrens[i].Value : options_v[i];
                if (check)
                {
                    result = result + "1";
                }
                else
                {
                    result = result + "0";
                }
            }
            _salida[0].Value = result;

        }

        public List<bool> Options_v { get => options_v; set => options_v = value; }

        public void generateBits(long startAddress, int amountOfBytes, int maxBit, Dictionary<int, string>? descriptions = null, BinaryReader? f = null)
        {
            /*
            this.startAddress = startAddress;
            this.amountOfBytes = amountOfBytes;
            this.maxBit = maxBit;
            */
            if (amountOfBytes > 1)
            {
            }
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
                    options_v.Add(UtilBinaryReader.GetBit(flags_value, bit));

                    bitsLeft--;
                }
            }
        }

    }
    public class Mmr3Hash : ValueTagInstace<Int32>
    {
        string str_value = "";
        public Mmr3Hash(Template tagDef, long addressStart, long offset) : base(tagDef, addressStart, offset)
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
            if (!Mmr3HashLTU.Mmr3lTU.TryGetValue(value, out str_value))
            {
                str_value = Mmr3HashLTU.getMmr3HashFromInt(value);
            }

            ExeTagInstance();
        }

        public override byte[] GetBytes()
        {
            return BitConverter.GetBytes(this.value);
        }
    }

    public class RgbPixel32 : ValueTagInstace<string>
    {
        string str_value = "";
        public RgbPixel32(Template tagDef, long addressStart, long offset) : base(tagDef, addressStart, offset)
        {
        }

        public string Str_value { get => str_value; set => str_value = value; }

        public override void ReadIn(BinaryReader f, TagHeader? header = null)
        {
            base.ReadIn(f, header);
            value = f.ReadInt32().ToString("X");

            ExeTagInstance();
        }
    }

    public class ArgbPixel32 : ValueTagInstace<string>
    {
        string str_value = "";
        public ArgbPixel32(Template tagDef, long addressStart, long offset) : base(tagDef, addressStart, offset)
        {
        }

        public string Str_value { get => str_value; set => str_value = value; }

        public override void ReadIn(BinaryReader f, TagHeader? header = null)
        {
            base.ReadIn(f, header);
            value = f.ReadInt32().ToString("X");

            ExeTagInstance();
        }
    }
    public class RGB : AtomicTagInstace
    {
        float r_value = -1;
        float g_value = -1;
        float b_value = -1;
        public RGB(Template tagDef, long addressStart, long offset) : base(tagDef, addressStart, offset)
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

        public override object AccessValue { get => new { R_value = r_value, G_value = g_value, B_value = b_value }; set { 
                var a = value;
                ; } }

        public float R_value { get => r_value; set => r_value = value; }
        public float B_value { get => b_value; set => b_value = value; }
        public float G_value { get => g_value; set => g_value = value; }

        public override byte[] GetBytes()
        {
            return BitConverter.GetBytes(r_value).Concat(BitConverter.GetBytes(g_value)).Concat(BitConverter.GetBytes(b_value)).ToArray();
        }
    }
    public class ARGB : AtomicTagInstace
    {
        float a_value = -1;
        float r_value = -1;
        float g_value = -1;
        float b_value = -1;
        public ARGB(Template tagDef, long addressStart, long offset) : base(tagDef, addressStart, offset)
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

        public override byte[] GetBytes()
        {
            return BitConverter.GetBytes(a_value).Concat(BitConverter.GetBytes(r_value)).Concat(BitConverter.GetBytes(g_value)).Concat(BitConverter.GetBytes(b_value)).ToArray();
        }

        public override object AccessValue { get => new { A_value = a_value, R_value = r_value, G_value = g_value, B_value = b_value }; set
            {
                var a = value;
                ;
            }
        }

        public float A_value { get => a_value; set => a_value = value; }
        public float R_value { get => r_value; set => r_value = value; }
        public float G_value { get => g_value; set => g_value = value; }
        public float B_value { get => b_value; set => b_value = value; }
    }
    public class BoundsFloat : AtomicTagInstace
    {
        float min = -1;
        float max = -1;
        public BoundsFloat(Template tagDef, long addressStart, long offset) : base(tagDef, addressStart, offset)
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

        public override byte[] GetBytes()
        {
            return BitConverter.GetBytes(min).Concat(BitConverter.GetBytes(max)).ToArray();
        }
        public override object AccessValue { get => new { Min = min, Max = max };set {
                var a = value;
                ;
            } }
    }
    public class Bounds2Byte : AtomicTagInstace
    {
        Int16 min = -1;
        Int16 max = -1;
        public Bounds2Byte(Template tagDef, long addressStart, long offset) : base(tagDef, addressStart, offset)
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

        public override byte[] GetBytes()
        {
            return BitConverter.GetBytes(min).Concat(BitConverter.GetBytes(max)).ToArray();
        }

        public override object AccessValue
        {
            get => new { Min = min, Max = max }; set
            {
                var a = value;
                ;
            }
        }
    }
    public class Point2DFloat : AtomicTagInstace
    {
        float x = -1;
        float y = -1;
        public Point2DFloat(Template tagDef, long addressStart, long offset) : base(tagDef, addressStart, offset)
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
        public override byte[] GetBytes()
        {
            return BitConverter.GetBytes(x).Concat(BitConverter.GetBytes(y)).ToArray();
        }
        public override object AccessValue
        {
            get => new { X = x, Y = y }; set
            {
                var a = value;
                ;
            }
        }

        public float X { get => x; set => x = value; }
        public float Y { get => y; set => y = value; }
    }
    public class Point2D2Byte : AtomicTagInstace
    {
        Int16 x = -1;
        Int16 y = -1;
        public Point2D2Byte(Template tagDef, long addressStart, long offset) : base(tagDef, addressStart, offset)
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

        public override byte[] GetBytes()
        {
            return BitConverter.GetBytes(x).Concat(BitConverter.GetBytes(y)).ToArray();
        }
        public override object AccessValue
        {
            get => new { X = x, Y = y }; set
            {
                var a = value;
                ;
            }
        }
    }
    public class Point3D : AtomicTagInstace
    {
        float x = -1;
        float y = -1;
        float z = -1;
        public Point3D(Template tagDef, long addressStart, long offset) : base(tagDef, addressStart, offset)
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
        public override byte[] GetBytes()
        {
            return BitConverter.GetBytes(x).Concat(BitConverter.GetBytes(y)).Concat(BitConverter.GetBytes(z)).ToArray();
        }
        public override object AccessValue
        {
            get => new { X = x, Y = y, Z = z }; set
            {
                var a = value;
                ;
            }
        }

        public float X { get => x; set => x = value; }
        public float Y { get => y; set => y = value; }
        public float Z { get => z; set => z = value; }
    }
    public class Plane2D : AtomicTagInstace
    {
        float x = -1;
        float y = -1;
        float point = -1;

        public Plane2D(Template tagDef, long addressStart, long offset) : base(tagDef, addressStart, offset)
        {
        }
        public override void ReadIn(BinaryReader f, TagHeader? header = null)
        {
            base.ReadIn(f, header);
            f.BaseStream.Seek(addressStart + offset, SeekOrigin.Begin);
            x = f.ReadSingle();
            y = f.ReadSingle();
            point = f.ReadSingle();
            ExeTagInstance();
        }
        public override byte[] GetBytes()
        {
            return BitConverter.GetBytes(x).Concat(BitConverter.GetBytes(y)).Concat(BitConverter.GetBytes(point)).ToArray();
        }
        public override object AccessValue
        {
            get => new { X = x, Y = y, Point = point }; set
            {
                var a = value;
                ;
            }
        }
    }
    public class Plane3D : AtomicTagInstace
    {
        float x = -1;
        float y = -1;
        float z = -1;
        float point = -1;

        public Plane3D(Template tagDef, long addressStart, long offset) : base(tagDef, addressStart, offset)
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
        public override byte[] GetBytes()
        {
            return BitConverter.GetBytes(x).Concat(BitConverter.GetBytes(y)).Concat(BitConverter.GetBytes(z)).Concat(BitConverter.GetBytes(point)).ToArray();
        }
        public override object AccessValue
        {
            get => new { X = x, Y = y, Z = z, Point = point }; set
            {
                var a = value;
                ;
            }
        }
    }
    public class Quaternion : AtomicTagInstace
    {
        float x = -1;
        float y = -1;
        float z = -1;
        float w = -1;
        public Quaternion(Template tagDef, long addressStart, long offset) : base(tagDef, addressStart, offset)
        {
        }


        public override object AccessValue
        {
            get => new { X = x, Y = y, Z = z, W = w }; set
            {
                var a = value;
                ;
            }
        }

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

        public override byte[] GetBytes()
        {
            return BitConverter.GetBytes(x).Concat(BitConverter.GetBytes(y)).Concat(BitConverter.GetBytes(z)).Concat(BitConverter.GetBytes(w)).ToArray();
        }

    }
    public class TagData : ValueTagInstace<int>
    {
        private ulong functAddress;
        private ulong functAddress_2;
        private int byteOffset;
        private int byteLengthCount;
        BinaryReader _f;
        public TagData(Template tagDef, long addressStart, long offset) : base(tagDef, addressStart, offset)
        {
        }

        public DataReference Data_reference { get; set; }

        public override object AccessValue
        {
            get => byteLengthCount; set
            {
                var a = value;
                ;
            }
        }

        public int ByteLengthCount { get => byteLengthCount; set => byteLengthCount = value; }
        public ulong FunctAddress { get => functAddress; set => functAddress = value; }
        public ulong FunctAddress_2 { get => functAddress_2; set => functAddress_2 = value; }

        public override void ReadIn(BinaryReader f, TagHeader? header = null)
        {
            base.ReadIn(f, header);
            //f.BaseStream.Seek(addressStart + offset, SeekOrigin.Begin);
            functAddress = f.ReadUInt64();
            functAddress_2 = f.ReadUInt64();
            byteOffset = f.ReadInt32();
            byteLengthCount = f.ReadInt32();
            var size_u = int.Parse(tagDef.E["int3"].ToString());
           
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
        protected CompoundTagInstance(Template tagDef, long addressStart, long offset) : base(tagDef, addressStart, offset)
        {
        }

        public abstract void AddChild(TagInstance item);

        public abstract List<TagInstance>? Childrens { get; }


    }
    public class ParentTagInstance : CompoundTagInstance, IDictionary<string, TagInstance>
    {
        protected Dictionary<string, TagInstance> keyValues=new Dictionary<string, TagInstance>();

        protected string item_name = "";
        protected string item_type = "";

        public override object AccessValue => getAccessValues();
        public override object AccessValueExtra => getAccessValuesExtra();

        public ICollection<string> Keys => ((IDictionary<string, TagInstance>)keyValues).Keys;

        public ICollection<TagInstance> Values => ((IDictionary<string, TagInstance>)keyValues).Values;

        public int Count => ((ICollection<KeyValuePair<string, TagInstance>>)keyValues).Count;

        public bool IsReadOnly => ((ICollection<KeyValuePair<string, TagInstance>>)keyValues).IsReadOnly;

        public override List<TagInstance>? Childrens => new List<TagInstance>(Values);

        public override TagInstance this[string path] { get => keyValues[path]; 
            set => keyValues[path] = value; }

        TagInstance IDictionary<string, TagInstance>.this[string key] { get => ((IDictionary<string, TagInstance>)keyValues)[key]; set => ((IDictionary<string, TagInstance>)keyValues)[key] = value; }

        public ParentTagInstance(Template tagDef, long addressStart, long offset) : base(tagDef, addressStart, offset)
        {
        }



        Dictionary<string, object> getAccessValues()
        {
            var result = new Dictionary<string, object>();
            foreach (var key in keyValues.Keys)
            {
                result[key] = keyValues[key].AccessValue;
            }
            return result;
        }
        Dictionary<string, object> getAccessValuesExtra()
        {
            var result = new Dictionary<string, object>();
            if (keyValues == null)
                return result;
            foreach (var key in keyValues.Keys)
            {
                result[key] = keyValues[key].AccessValueExtra;
            }
            return result;
        }



        public override void ReadIn(BinaryReader f, TagHeader? header = null)
        {
            base.ReadIn(f, header);
            //readTagBloks(f, header);
        }


        public override void AddChild(TagInstance tagInstance)
        {
            if (keyValues == null)
                keyValues = new Dictionary<string, TagInstance>();
            string key = tagInstance.TagDef.N;
            string item_key = tagInstance.TagDef.xmlPath.Item2;
            string parent_key = tagInstance.Parent == null ? "#document\\" : tagInstance.Parent.TagDef.xmlPath.Item2;
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
            if (!string.IsNullOrEmpty(path))
            {
                var a_split = path.Split('.');
                if (keyValues.ContainsKey(a_split[0]))
                {
                    if (a_split.Length == 1)
                    {
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

    public class ListItemTagInstance : ParentTagInstance
    {
        public int Index { get; set; }
        public ListItemTagInstance(Template tagDef, long addressStart, long offset) : base(tagDef, addressStart, offset)
        {
        }
    }
    public class RootTagInstance : ParentTagInstance
    {

        public RootTagInstance(Template tagDef, long addressStart, int offset) : base(tagDef, addressStart, offset)
        {
            FieldName = "Root";
        }

    }
    public class RenderGeometryTag : TagStructData
    {
        public RenderGeometryTag(Template tagDef, long addressStart, long offset) : base(tagDef, addressStart, offset)
        {
        }
    }
    public class TagStructData : ParentTagInstance
    {
        //bool generateEntry = false;
        //string comment = "";
        public TagStructData(Template tagDef, long addressStart, long offset) : base(tagDef, addressStart, offset)
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

        public ListTagInstance(Template tagDef, long addressStart, long offset) : base(tagDef, addressStart, offset)
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

        protected override long GetInFileOffset()
        {
            return ((Content_entry?.Data_parent?.OffsetPlus ?? 0) + (Content_entry?.Field_offset ?? inst_address));
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
                    if (index >= 0 && index < childs.Count)
                    {
                        string sub_path = string.Join('.', a_split.Skip(1));
                        if (a_split.Length == 1)
                            return childs[index];
                        return childs[index].GetObjByPath(sub_path);
                    }

                }

            }
            return null;
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

        public ExternalFileDescriptor(Template tagDef, long addressStart, long offset) : base(tagDef, addressStart, offset)
        {
        }

        public long NewAddress { get => newAddress; set => newAddress = value; }

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
        private bool _isExternal = false;
        private int _index = -1;

        public ResourceHandle(Template tagDef, long addressStart, long offset) : base(tagDef, addressStart, offset)
        {
        }

        public long NewAddress { get => newAddress; set => newAddress = value; }
        public bool IsExternal { get => _isExternal; set => _isExternal = value; }
        public int Index { get => _index; set => _index = value; }

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

        public Tagblock(Template tagDef, long addressStart, long offset) : base(tagDef, addressStart, offset)
        {
        }

        public long NewAddress { get => newAddress; set => newAddress = value; }
        public long StringAddress { get => stringAddress; set => stringAddress = value; }

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

        public ArrayFixLen(Template tagDef, long addressStart, long offset) : base(tagDef, addressStart, offset)
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
        public TagStructBlock(Template tagDef, long addressStart, long offset) : base(tagDef, addressStart, offset)
        {
            throw new Exception("REvisar el uso");
        }
    }
   }
