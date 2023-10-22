using LibHIRT.Domain;
using LibHIRT.Files.Base;
using LibHIRT.Files.FileTypes;
using LibHIRT.ModuleUnpacker;
using LibHIRT.Serializers;
using LibHIRT.TagReader;
using LibHIRT.TagReader.Headers;
using static LibHIRT.TagReader.TagLayouts;

namespace LibHIRT.Files
{
    public abstract class SSpaceFile : ISSpaceFile, IHIRTFile
    {

        #region Data Members

        private ISSpaceFile _parent;
        private ISSpaceFile _ref_parent;
        private IList<ISSpaceFile> _children;
        private Dictionary<int, ISSpaceFile> _resource;
        private List<ISSpaceFile> _resource_list;
        private Dictionary<int, ISSpaceFile> _ref_children;

        private string _name;
        private string _tagGroup;

        private string _extension;
        private long _hash;


        private HiModuleFileEntry fileMemDescriptor;

        private HIRTStream _baseStream;
        public long _dataStartOffset;
        public long _dataEndOffset;
        private BinaryReader _reader;

        private bool _isInitialized;
        private bool _isDisposed;

        private string _inDiskPath = "";

        #endregion

        #region Properties

        public ISSpaceFile Parent => _parent;
        public IEnumerable<ISSpaceFile> Children => _children;

        public string Name => _name;
        public string Extension => _extension;
        public long Hash => _hash;
        public long SizeInBytes
        {
            get => _dataEndOffset - _dataStartOffset;
        }

        public abstract string FileTypeDisplay { get; }

        protected HIRTStream BaseStream => _baseStream;
        protected long DataStartOffset => _dataStartOffset;
        protected long DataEndOffset => _dataEndOffset;
        protected BinaryReader Reader => _reader;

        public HiModuleFileEntry FileMemDescriptor { get => fileMemDescriptor; set => fileMemDescriptor = value; }

        public string Path_string => fileMemDescriptor?.Path_string;

        public string TagGroup
        {
            get { return _tagGroup == null ? fileMemDescriptor?.TagGroupRev : _tagGroup; }
            set { _tagGroup = value; }
        }

        public string InDiskPath { get => _inDiskPath; set => _inDiskPath = value; }

        public ISSpaceFile RefParent { get => _ref_parent; set => _ref_parent = value; }

        public Dictionary<int, ISSpaceFile> RefChildren
        {
            get
            {
                if (_ref_children == null)
                    _ref_children = new Dictionary<int, ISSpaceFile>();
                return _ref_children;
            }
        }

        private DinamycType? _deserialized;

        public bool IsDeserialized { get => _deserialized != null; }
        public List<ISSpaceFile> Resource_list { get => _resource_list; set => _resource_list = value; }

        public DinamycType? Deserialized(EventHandler<ITagInstance> _onDeserialized = null)
        {
            if (_deserialized == null)
                _deserialized = GenericSerializer.Deserialize(GetStream(), this, _onDeserialized);
            GetStream().Seek(0, SeekOrigin.Begin);
            return _deserialized;

        }

        #endregion

        #region Constructor

        protected SSpaceFile(string name, HIRTStream baseStream,
          long dataStartOffset, long dataEndOffset,
          ISSpaceFile parent = null)
        {
            _hash = name.GetHashCode();
            //_name = _hash.ToString() + SanitizeName(name);
            _name = SanitizeName(name);
            if (parent != null && parent is ModuleFile)
                _name = (((ModuleFile)parent).TryGetGlobalId().ToString()) + "_" + _name;
            _extension = Path.GetExtension(_name);

            _baseStream = baseStream;
            _dataStartOffset = dataStartOffset;
            _dataEndOffset = dataEndOffset;
            if (_baseStream != null)
                _reader = new BinaryReader(_baseStream, System.Text.Encoding.UTF8, true);

            _parent = parent;
            _children = new List<ISSpaceFile>();
            _resource_list = new List<ISSpaceFile>();
            _resource = new Dictionary<int, ISSpaceFile>();
        }

       public ISSpaceFile GetResourceAt(int index)
        {
            if (_resource.ContainsKey(index))
                return _resource[index];
            else {
                var temp = Parent as ModuleFile;
                var r_file = (SSpaceFile)temp.getResourceOfFileAt(this, index);
                if (r_file != null) {
                    _resource[index] = r_file;
                    return r_file;
                }
            }

            return null;
        }

        ~SSpaceFile()
        {
            Dispose(false);
        }

        #endregion

        #region Public Methods

        public void Initialize()
        {
            if (_isInitialized)
                return;

            OnInitialize();

            _isInitialized = true;
        }

        public virtual HIRTStream GetStream()
          => GetFromFileDescriptor();
        //=> new HIRTStreamSegment(_baseStream, _dataStartOffset, SizeInBytes);

        #endregion

        #region Protected Methods

        protected virtual void OnInitialize()
        {
        }

        protected void AddChild(ISSpaceFile file)
        {
            _children.Add(file);
        }

        protected virtual string SanitizeName(string path)
        {
            if (path.Contains(":"))
                path = path.Substring(path.IndexOf(':') + 1);
            if (path.Contains(">"))
                path = path.Substring(path.IndexOf('>') + 1);

            return Path.GetFileName(path);
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
          => Dispose(true);

        private void Dispose(bool isDisposing)
        {
            if (_isDisposed)
                return;

            OnDisposing(isDisposing);

            if (isDisposing)
            {
                foreach (var child in _children)
                    child?.Dispose();
            }

            _isDisposed = true;
        }

        protected virtual void OnDisposing(bool isDisposing)
        {
        }

        #endregion

        #region IEquatable Methods

        public bool Equals(ISSpaceFile other)
        {
            return Name.Equals(other.Name);
        }

        public Stream GetMemoryStream_()
        {
            if (_baseStream != null)
            {

            }
            if (fileMemDescriptor != null)
            {
                var paret = (Parent as ModuleFile);
                if (paret != null)
                {
                    return paret.GetMemoryStreamFromFile(fileMemDescriptor);
                }
            }
            return new MemoryStream();
        }

        protected HIRTStream GetFromFileDescriptor()
        {
            if (_baseStream != null)
            {
                return _baseStream;
            }
            if (fileMemDescriptor != null)
            {
                var paret = (Parent as ModuleFile);
                if (paret != null)
                {
                    _baseStream = HIRTExtractedFileStream.FromStream(paret.GetMemoryStreamFromFile(fileMemDescriptor));
                    _reader = new BinaryReader(_baseStream, System.Text.Encoding.UTF8, true);
                }
            }
            return _baseStream;
        }

        public string GetTagXmlTempaltePath()
        {

            if (fileMemDescriptor != null)
            {
                string temp = fileMemDescriptor.TagGroupRev;
                ;
                return TagXmlParse.GetXmlPath(ref temp);
            }
            return "";
        }

        public int TryGetGlobalId()
        {
            if (this is ModuleFile)
                return ((ModuleFile)this).ModuleHeader.ModuleIntId;
            if (fileMemDescriptor != null)
            {
                return fileMemDescriptor.GlobalTagId1;
            }
            if (_baseStream != null)
            {
                TagFile temp = new TagFile();
                return temp.tryReadGlobalId(_baseStream);
            }
            return -1;
        }
        #endregion

    }
}
