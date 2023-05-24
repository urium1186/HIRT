﻿using DeepCopy;
using HaloInfiniteResearchTools.Common;
using LibHIRT.TagReader;
using PropertyChanged;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace HaloInfiniteResearchTools.Models
{
    public class TagReaderPath : ObservableObject, IDeepCopy<TagReaderOptionsModel>
    {
        public string Path { get; set; }
        [OnChangedMethod(nameof(SetActivePath))]
        public bool Active { get; set; }

        private void SetActivePath()
        {
            if (Active)
                TagLayouts.TagXmlParse.TagsPath = Path;
        }

        public TagReaderPath()
        {
        }

        [DeepCopyConstructor]
        public TagReaderPath(TagReaderPath source)
        {
        }
    }
    public class TagReaderOptionsModel : ObservableObject, IDeepCopy<TagReaderOptionsModel>
    {

        #region Data Members

        public static TagReaderOptionsModel Default
        {
            get
            {
                return new TagReaderOptionsModel
                {
                    Paths = new ObservableCollection<TagReaderPath>()
                };
            }
        }

        #endregion

        #region Properties
        [OnChangedMethod(nameof(SetGlobalDefaults))]
        [JsonIgnore]
        public string XmlOutputPath { get; set; }

        private void SetGlobalDefaults()
        {
            if (string.IsNullOrEmpty(XmlOutputPath))
                return;
            if (Paths == null)
            {
                Paths = new ObservableCollection<TagReaderPath>();
                Paths.Add(new TagReaderPath
                {
                    Active = true,
                    Path = XmlOutputPath
                });
            }
            else
            {
                bool found = false;
                foreach (var item in Paths)
                {
                    if (item.Path == XmlOutputPath)
                    {
                        item.Active = true;
                        found = true;
                        break;
                    }
                    else
                    {
                        item.Active = false;
                    }
                }
                if (!found)
                {
                    Paths.Add(new TagReaderPath
                    {
                        Active = true,
                        Path = XmlOutputPath
                    });
                }
            }


        }


        [JsonIgnore]
        public string Filters { get; set; }

        [DefaultValue(null)]
        public ObservableCollection<TagReaderPath> Paths { get; set; }



        #endregion

        #region Constructor

        public TagReaderOptionsModel()
        {
        }

        [DeepCopyConstructor]
        public TagReaderOptionsModel(TagReaderOptionsModel source)
        {
        }

        #endregion

    }
}
