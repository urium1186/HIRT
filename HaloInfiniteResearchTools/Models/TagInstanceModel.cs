using LibHIRT.TagReader;
using System.Collections.Generic;

namespace HaloInfiniteResearchTools.Models
{
    public class TagInstanceModel
    {
        protected List<TagInstanceModel> _childrens = new List<TagInstanceModel>();

        protected TagInstance? _value;

        public TagInstanceModel(TagInstance value)
        {
            _value = value;
            if (value is ParentTagInstance)
            {
                var temp = (ParentTagInstance)value;
                foreach (var item in temp.Keys)
                {
                    var child = getTagModelBy(temp[item]);
                    child.Header = item;
                    _childrens.Add(child);
                }
            }
            else if (value is ListTagInstance)
            {
                var temp = (ListTagInstance)value;
                for (int i = 0; i < temp.Count; i++)
                {
                    var child = getTagModelBy(temp[i]);
                    child.Header = "[" + i.ToString() + "]";
                    _childrens.Add(child);
                }
            }
            else
            {
                Header = _value.AccessValue.ToString();
            }
        }

        public TagInstanceModel getTagModelBy(TagInstance item)
        {
            if (item is TagRef)
                return new TagRefInstaceModel(item);
            else
                return new TagInstanceModel(item);
        }
        public TagInstanceModel()
        {
        }

        string _header = "Root";

        public List<TagInstanceModel> Childrens { get => _childrens; set => _childrens = value; }
        public TagInstance Value { get => _value; set => _value = value; }

        public string StrValue
        {
            get
            {
                if (Value is AtomicTagInstace)
                    return Value.AccessValue.ToString();
                else if (Value is ListTagInstance)
                    return "Count - " + (Value as ListTagInstance).Count.ToString();
                //else if (Value is ParentTagInstance)
                //    return "Number of childs - "+(Value as ParentTagInstance).Count.ToString();
                return "";
            }
        }
        public string Header { get => _header; set => _header = value; }
    }

    public class TagRefInstaceModel : TagInstanceModel
    {
        public TagRefInstaceModel(TagInstance value) : base(value)
        {
        }

        public string TagGroup { get { return "Is a tag ref"; } }
    }
}
