namespace LibHIRT.TagReader.Common
{
    public interface FieldModel
    {
        public string Name { get; }
        public string Description { get; }
        public string ParentOffset { get; }
        public string FileOffset { get; }
        public string Type { get; }
        public string Size { get;  }
        public List<FieldModel> Childrens { get;  }


    }
}
