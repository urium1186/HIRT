using LibHIRT.Utils;
using System.Diagnostics;
using System.Xml;

namespace LibHIRT.TagReader
{
    // ###### for anyone interested, check out https://github.com/Lord-Zedd/H5Tags/tree/master/tags // thank you lord zedd
    // ###### its quite useful for mapping out descriptions and stuff
    public class TagLayouts
    {
        public class C
        {
            public TagElemntType? T { get; set; } // T = type
            public Dictionary<long, C>? B { get; set; } = null; // B = blocks? i forgot what B stands for
            public Dictionary<string, object>? P { get; set; } = null;
            public Dictionary<string, object>? E { get; set; } = null;
            /// <summary>
            /// Length of the tagblock
            /// </summary>
            public long S { get; set; } // S = size // length of tagblock

            public string N { get; set; } // N = name // our name for the block 


            /// <summary>
            /// Set during load, will be used when I add netcode 
            /// </summary>
            public long MemoryAddress { get; set; }

            /// <summary>
            /// The absolute offset from the base address of the tag
            /// eg C2 will resolve to assault_rifle_mp.weapon + C2 
            /// 
            /// This will be recursive so the actual _intValue might be 
            ///		assault_rifle_mp.weapon + C2 + (nested block) 12 + (nested block) 4
            ///		
            /// This will allow us to sync up changes across the server and client without
            /// the need to re-resolve memory addresses.
            /// </summary>
            public string AbsoluteTagOffset { get; set; } // as a string we can append offsets rather than mathmatically adding them

            public (string, string) xmlPath { get; set; }
            public string G { get; set; }
        }

        public class FlagGroupTL : C
        {
            public FlagGroupTL()
            {
                T = TagElemntType.FlagGroup;
            }

            /// <summary>
            /// Amount of bytes for flags
            /// </summary>
            public int A { get; set; }

            /// <summary>
            /// The max bit, if 0 then defaults to A * 8
            /// </summary>
            public int MB { get; set; }
            /// <summary>
            /// String description of the flags
            /// </summary>
            public Dictionary<int, string> STR { get; set; } = new Dictionary<int, string>();
        }
        public class EnumGroupTL : C
        {
            public EnumGroupTL()
            {
                T = TagElemntType.EnumGroup;
            }

            /// <summary>
            /// Amount of bytes for enum
            /// </summary>
            public int A { get; set; }

            /// <summary>
            /// String description of the flags
            /// </summary>
            public Dictionary<int, string> STR { get; set; } = new Dictionary<int, string>();
        }

        /*public static Dictionary<long, C> Tags(string grouptype)
		{
			run_parse r = new run_parse();
			return r.parse_the_mfing_xmls(grouptype);
		}*/

        public static class TagXmlParse
        {

            static long evalutated_index_PREVENT_DICTIONARYERROR = 99999;
            static Dictionary<string, Dictionary<long, C?>> readedTags = new Dictionary<string, Dictionary<long, C?>>();
            static string _tagsPath = "";

            public static void reset()
            {
                evalutated_index_PREVENT_DICTIONARYERROR = 99999;
                readedTags.Clear();
            }
            public static Dictionary<long, C?> parse_the_mfing_xmls(string file_to_find)
            {

                lock (readedTags)
                {
                    if (readedTags.ContainsKey(file_to_find))
                        return readedTags[file_to_find];

                    evalutated_index_PREVENT_DICTIONARYERROR = 99999;

                    Dictionary<long, C?> poopdict = new();
                    string predicted_file = GetXmlPath(ref file_to_find);

                    if (File.Exists(predicted_file))
                    {
                        try
                        {
                            XmlDocument xd = new XmlDocument();
                            xd.Load(predicted_file);
                            XmlNode xn = xd.SelectSingleNode("root");
                            long current_offset = 0;
                            /*XmlNodeList xnl = xn.ChildNodes;

                            foreach (XmlNode xntwo in xnl)
                            {
                                current_offset += the_switch_statement(xntwo, current_offset, ref poopdict);
                            }*/
                            the_switch_statement(xn, current_offset, ref poopdict);
                            readedTags[file_to_find] = poopdict;
                        }
                        catch (Exception ex)
                        {

                            throw ex;
                        }

                    }

                    return poopdict;
                }

            }

            public static string GetXmlPath(ref string file_to_find)
            {
                if (file_to_find.Contains("*"))
                {
                    file_to_find = file_to_find.Replace("*", "_");
                }
                string tempDirPath = Directory.GetCurrentDirectory() + "\\TagReader\\Tags\\";
                if (Directory.Exists(_tagsPath))
                    tempDirPath = _tagsPath + "\\";

                string predicted_file = tempDirPath + file_to_find + ".xml";
                return predicted_file;
            }

            static string getPathOfElement(XmlNode xn)
            {
                string path = xn.Name;
                var temp = xn;
                while (temp.ParentNode != null)
                {
                    temp = temp.ParentNode;
                    path = temp.Name + "\\" + path;
                }
                return path;
            }
            static string getPathOfElementNamed(XmlNode xn)
            {
                string path = "";
                if (xn.Attributes.GetNamedItem("v") != null)
                    path = xn.Attributes.GetNamedItem("v").InnerText;
                if (path == "")
                    path = xn.Name;
                var temp = xn;
                while (temp.ParentNode != null)
                {
                    temp = temp.ParentNode;

                    string t_s = temp.Name;
                    if (temp.Attributes != null && temp.Attributes.GetNamedItem("v") != null && temp.Attributes.GetNamedItem("v").InnerText != "")
                        t_s = temp.Attributes.GetNamedItem("v").InnerText;
                    path = t_s + "\\" + path;
                }
                return path;
            }
            static long the_switch_statement(XmlNode xn, long offset, ref Dictionary<long, C?> pairs)
            {
                var s_p = getPathOfElement(xn);
                string s_p_n = getPathOfElementNamed(xn);
                Dictionary<string, object> extra_afl = new Dictionary<string, object>();
                FillGeneralExtraData(xn, extra_afl);
                switch (xn.Name)
                {
                    case "root":
                        extra_afl.Clear();
                        if (xn.ChildNodes.Count > 0)
                        {
                            Dictionary<long, C> subthings = new Dictionary<long, C>();
                            XmlNodeList xnl2 = xn.ChildNodes;

                            FillGeneralExtraData(xn, extra_afl);

                            long current_offset2 = 0;
                            foreach (XmlNode xntwo2 in xnl2)
                            {
                                current_offset2 += the_switch_statement(xntwo2, current_offset2, ref subthings); // its gonna append that to the main, rather than our struct
                            }

                            pairs.Add(offset, new C { G = xn.Name, T = TagElemntType.RootTagInstance, N = "root", B = subthings, E = extra_afl, S = current_offset2, xmlPath = ("root", "root") });
                            return current_offset2;
                        }
                        else
                        {
                            pairs.Add(offset, new C { G = xn.Name, T = TagElemntType.Tagblock, N = xn.Attributes.GetNamedItem("v").InnerText, S = 20, xmlPath = (s_p, s_p_n) });
                        }
                        return 0;
                    case "_0": // string
                        pairs.Add(offset, new C { G = xn.Name, T = TagElemntType.String, N = xn.Attributes.GetNamedItem("v").InnerText, S = group_lengths_dict[xn.Name], xmlPath = (s_p, s_p_n) });
                        return group_lengths_dict[xn.Name];
                    case "_1": // long string
                        pairs.Add(offset, new C { G = xn.Name, T = TagElemntType.String, N = xn.Attributes.GetNamedItem("v").InnerText, S = group_lengths_dict[xn.Name], xmlPath = (s_p, s_p_n) });
                        return group_lengths_dict[xn.Name];
                    case "_2": // -- hashstring [ global | test | multi ] namespace
                        pairs.Add(offset, new C { G = xn.Name, T = TagElemntType.Mmr3Hash, N = xn.Attributes.GetNamedItem("v").InnerText, S = group_lengths_dict[xn.Name], xmlPath = (s_p, s_p_n) });
                        return group_lengths_dict[xn.Name];
                    case "_3":// unmapped - This case isn't found in any tag file
                        pairs.Add(offset, new C { G = xn.Name, T = TagElemntType.Comment, N = xn.Attributes.GetNamedItem("v").InnerText /*+ " (unmapped type(" + xn.Name + "), may cause errors)"*/, S = group_lengths_dict[xn.Name], xmlPath = (s_p, s_p_n) });
                        return group_lengths_dict[xn.Name];
                    case "_4": // char
                        pairs.Add(offset, new C { G = xn.Name, T = TagElemntType.Byte, N = xn.Attributes.GetNamedItem("v").InnerText, S = group_lengths_dict[xn.Name], xmlPath = (s_p, s_p_n) });
                        return group_lengths_dict[xn.Name];
                    case "_5": // short
                        pairs.Add(offset, new C { G = xn.Name, T = TagElemntType.TwoByte, N = xn.Attributes.GetNamedItem("v").InnerText, S = group_lengths_dict[xn.Name], xmlPath = (s_p, s_p_n) });
                        return group_lengths_dict[xn.Name];
                    case "_6": // long
                        pairs.Add(offset, new C { G = xn.Name, T = TagElemntType.FourByte, N = xn.Attributes.GetNamedItem("v").InnerText, S = group_lengths_dict[xn.Name], xmlPath = (s_p, s_p_n) });
                        return group_lengths_dict[xn.Name];
                    case "_7": // int64
                        pairs.Add(offset, new C { G = xn.Name, T = TagElemntType.Pointer, N = xn.Attributes.GetNamedItem("v").InnerText, S = group_lengths_dict[xn.Name], xmlPath = (s_p, s_p_n) });
                        return group_lengths_dict[xn.Name];
                    case "_8": // angle
                        pairs.Add(offset, new C { G = xn.Name, T = TagElemntType.Float, N = xn.Attributes.GetNamedItem("v").InnerText, S = group_lengths_dict[xn.Name], xmlPath = (s_p, s_p_n) });
                        return group_lengths_dict[xn.Name];
                    case "_9": // tag
                        pairs.Add(offset, new C { G = xn.Name, T = TagElemntType.StringTag, N = xn.Attributes.GetNamedItem("v").InnerText, S = group_lengths_dict[xn.Name], xmlPath = (s_p, s_p_n) });
                        return group_lengths_dict[xn.Name];
                    case "_A": // char enum
                        Dictionary<int, string> childdictionary1 = new();
                        for (int iu = 0; iu < xn.ChildNodes.Count; iu++)
                        {
                            childdictionary1.Add(iu, xn.ChildNodes[iu].Attributes.GetNamedItem("v").InnerText);
                        }
                        pairs.Add(offset, new EnumGroupTL { G = xn.Name, A = 1, N = xn.Attributes.GetNamedItem("v").InnerText, STR = childdictionary1, S = group_lengths_dict[xn.Name], xmlPath = (s_p, s_p_n) });

                        return group_lengths_dict[xn.Name];
                    case "_B": // short enum
                        Dictionary<int, string> childdictionary2 = new();
                        for (int iu = 0; iu < xn.ChildNodes.Count; iu++)
                        {
                            childdictionary2.Add(iu, xn.ChildNodes[iu].Attributes.GetNamedItem("v").InnerText);
                        }
                        pairs.Add(offset, new EnumGroupTL { G = xn.Name, A = 2, N = xn.Attributes.GetNamedItem("v").InnerText, STR = childdictionary2, S = group_lengths_dict[xn.Name], xmlPath = (s_p, s_p_n) });

                        return group_lengths_dict[xn.Name];
                    case "_C": // long enum
                        Dictionary<int, string> childdictionary3 = new();
                        for (int iu = 0; iu < xn.ChildNodes.Count; iu++)
                        {
                            childdictionary3.Add(iu, xn.ChildNodes[iu].Attributes.GetNamedItem("v").InnerText);
                        }
                        pairs.Add(offset, new EnumGroupTL { G = xn.Name, A = 4, N = xn.Attributes.GetNamedItem("v").InnerText, STR = childdictionary3, S = group_lengths_dict[xn.Name], xmlPath = (s_p, s_p_n) });

                        return group_lengths_dict[xn.Name];
                    case "_D": // long flags
                        Dictionary<int, string> childdictionary4 = new();
                        for (int iu = 0; iu < xn.ChildNodes.Count; iu++)
                        {
                            childdictionary4.Add(iu, xn.ChildNodes[iu].Attributes.GetNamedItem("v").InnerText);
                        }
                        pairs.Add(offset, new FlagGroupTL { G = xn.Name, A = 4, N = xn.Attributes.GetNamedItem("v").InnerText, STR = childdictionary4, S = group_lengths_dict[xn.Name], xmlPath = (s_p, s_p_n) });

                        return group_lengths_dict[xn.Name];
                    case "_E": // word flags
                        Dictionary<int, string> childdictionary5 = new();
                        for (int iu = 0; iu < xn.ChildNodes.Count; iu++)
                        {
                            childdictionary5.Add(iu, xn.ChildNodes[iu].Attributes.GetNamedItem("v").InnerText);
                        }
                        pairs.Add(offset, new FlagGroupTL { G = xn.Name, A = 2, N = xn.Attributes.GetNamedItem("v").InnerText, STR = childdictionary5, S = group_lengths_dict[xn.Name], xmlPath = (s_p, s_p_n) });

                        return group_lengths_dict[xn.Name];
                    case "_F": // byte flags
                        Dictionary<int, string> childdictionary6 = new();
                        for (int iu = 0; iu < xn.ChildNodes.Count; iu++)
                        {
                            childdictionary6.Add(iu, xn.ChildNodes[iu].Attributes.GetNamedItem("v").InnerText);
                        }
                        pairs.Add(offset, new FlagGroupTL { G = xn.Name, A = 1, N = xn.Attributes.GetNamedItem("v").InnerText, STR = childdictionary6, S = group_lengths_dict[xn.Name], xmlPath = (s_p, s_p_n) });

                        return group_lengths_dict[xn.Name];

                    case "_10": // short point 2d -- im not 100% on this one
                        pairs.Add(offset, new C { G = xn.Name, T = TagElemntType.Point2D2Byte, N = xn.Attributes.GetNamedItem("v").InnerText, xmlPath = (s_p, s_p_n) });
                        return group_lengths_dict[xn.Name];
                    case "_11":// unmapped -- short rectangle 2d
                        Debug.Assert(DebugConfig.NoCheckFails, "Revisar pq se  unmapped");//throw new Exception("Revisar pq se _11 unmapped");
                        pairs.Add(offset, new C { G = xn.Name, T = TagElemntType.Comment, N = xn.Attributes.GetNamedItem("v").InnerText /*+ " (unmapped type(" + xn.Name + "), may cause errors)"*/, S = group_lengths_dict[xn.Name], xmlPath = (s_p, s_p_n) });
                        return group_lengths_dict[xn.Name];
                    case "_12": // rgb pixel 32
                        //Debug.Assert(DebugConfig.NoCheckFails, "Revisar pq se  unmapped");//throw new Exception("Revisar pq se _12 unmapped Revisar pq se dice q es un color");
                        pairs.Add(offset, new C { G = xn.Name, T = TagElemntType.RgbPixel32, N = xn.Attributes.GetNamedItem("v").InnerText, S = group_lengths_dict[xn.Name], xmlPath = (s_p, s_p_n) });
                        return group_lengths_dict[xn.Name];
                    case "_13":// unmapped - argb pixel 32 - only found in ttag
                        //Debug.Assert(DebugConfig.NoCheckFails, "Revisar pq se  unmapped");//throw new Exception("Revisar pq se _11 unmapped");
                        pairs.Add(offset, new C { G = xn.Name, T = TagElemntType.ArgbPixel32, N = xn.Attributes.GetNamedItem("v").InnerText /*+ " (unmapped type(" + xn.Name + "), may cause errors)"*/, S = group_lengths_dict[xn.Name], xmlPath = (s_p, s_p_n) });
                        return group_lengths_dict[xn.Name];
                    case "_14": // real
                        pairs.Add(offset, new C { G = xn.Name, T = TagElemntType.Float, N = xn.Attributes.GetNamedItem("v").InnerText, S = group_lengths_dict[xn.Name], xmlPath = (s_p, s_p_n) });
                        return group_lengths_dict[xn.Name];
                    case "_15": // fraction
                        pairs.Add(offset, new C { G = xn.Name, T = TagElemntType.Float, N = xn.Attributes.GetNamedItem("v").InnerText, S = group_lengths_dict[xn.Name], xmlPath = (s_p, s_p_n) });
                        return group_lengths_dict[xn.Name];
                    case "_16": // real point 2d
                        pairs.Add(offset, new C { G = xn.Name, T = TagElemntType.Point2DFloat, N = xn.Attributes.GetNamedItem("v").InnerText, S = group_lengths_dict[xn.Name], xmlPath = (s_p, s_p_n) });
                        return group_lengths_dict[xn.Name];
                    case "_17":// real point 3d
                        pairs.Add(offset, new C { G = xn.Name, T = TagElemntType.Point3D, N = xn.Attributes.GetNamedItem("v").InnerText, S = group_lengths_dict[xn.Name], xmlPath = (s_p, s_p_n) });
                        return group_lengths_dict[xn.Name];
                    case "_18": // real vector 2d
                        pairs.Add(offset, new C { G = xn.Name, T = TagElemntType.Point2DFloat, N = xn.Attributes.GetNamedItem("v").InnerText, S = group_lengths_dict[xn.Name], xmlPath = (s_p, s_p_n) });
                        return group_lengths_dict[xn.Name];
                    case "_19": // real vector 3d
                        pairs.Add(offset, new C { G = xn.Name, T = TagElemntType.Point3D, N = xn.Attributes.GetNamedItem("v").InnerText, S = group_lengths_dict[xn.Name], xmlPath = (s_p, s_p_n) });
                        return group_lengths_dict[xn.Name];
                    case "_1A": // real quaternion
                        pairs.Add(offset, new C { G = xn.Name, T = TagElemntType.Quaternion, N = xn.Attributes.GetNamedItem("v").InnerText, S = group_lengths_dict[xn.Name], xmlPath = (s_p, s_p_n) });
                        return group_lengths_dict[xn.Name];
                    case "_1B": // real euler angles 2d
                        pairs.Add(offset, new C { G = xn.Name, T = TagElemntType.Point2DFloat, N = xn.Attributes.GetNamedItem("v").InnerText, S = group_lengths_dict[xn.Name], xmlPath = (s_p, s_p_n) });
                        return group_lengths_dict[xn.Name];
                    case "_1C": // real euler angles 3d
                        pairs.Add(offset, new C { G = xn.Name, T = TagElemntType.Point3D, N = xn.Attributes.GetNamedItem("v").InnerText, S = group_lengths_dict[xn.Name], xmlPath = (s_p, s_p_n) });
                        return group_lengths_dict[xn.Name];
                    case "_1D":// unmapped -- plane 2d
                        Debug.Assert(DebugConfig.NoCheckFails, "Revisar pq se  unmapped");//throw new Exception("Revisar pq se _1D unmapped");
                        pairs.Add(offset, new C { G = xn.Name, T = TagElemntType.Comment, N = xn.Attributes.GetNamedItem("v").InnerText /*+ " (unmapped type(" + xn.Name + "), may cause errors)"*/, S = group_lengths_dict[xn.Name], xmlPath = (s_p, s_p_n) });
                        return group_lengths_dict[xn.Name];
                    case "_1E": // plane 3d -- pretty sure this is currect, could be wrong though. I referenced calculus equations
                        pairs.Add(offset, new C { G = xn.Name, T = TagElemntType.Plane3D, N = xn.Attributes.GetNamedItem("v").InnerText, S = group_lengths_dict[xn.Name], xmlPath = (s_p, s_p_n) });
                        return group_lengths_dict[xn.Name];
                    case "_1F": // real rgb color
                        pairs.Add(offset, new C { G = xn.Name, T = TagElemntType.RGB, N = xn.Attributes.GetNamedItem("v").InnerText, S = group_lengths_dict[xn.Name], xmlPath = (s_p, s_p_n) });
                        return group_lengths_dict[xn.Name];
                    case "_20": // real argb color
                        pairs.Add(offset, new C { G = xn.Name, T = TagElemntType.ARGB, N = xn.Attributes.GetNamedItem("v").InnerText, S = group_lengths_dict[xn.Name], xmlPath = (s_p, s_p_n) });
                        return group_lengths_dict[xn.Name];
                    case "_21":// unmapped - only found in ttag
                        Debug.Assert(DebugConfig.NoCheckFails, "Revisar pq se  unmapped");//throw new Exception("Revisar pq se _21 unmapped");
                        pairs.Add(offset, new C { G = xn.Name, T = TagElemntType.Comment, N = xn.Attributes.GetNamedItem("v").InnerText /*+ " (unmapped type(" + xn.Name + "), may cause errors)"*/, S = group_lengths_dict[xn.Name], xmlPath = (s_p, s_p_n) });
                        return group_lengths_dict[xn.Name];
                    case "_22":// unmapped  - only found in ttag
                        Debug.Assert(DebugConfig.NoCheckFails, "Revisar pq se  unmapped");//throw new Exception("Revisar pq se _22 unmapped");
                        pairs.Add(offset, new C { G = xn.Name, T = TagElemntType.Comment, N = xn.Attributes.GetNamedItem("v").InnerText /*+ " (unmapped type(" + xn.Name + "), may cause errors)"*/, S = group_lengths_dict[xn.Name], xmlPath = (s_p, s_p_n) });
                        return group_lengths_dict[xn.Name];
                    case "_23": // short bounds -- revisar pq 2 entradas pudiendo ser una
                        pairs.Add(offset, new C { G = xn.Name, T = TagElemntType.TwoByte, N = xn.Attributes.GetNamedItem("v").InnerText + ".min", S = group_lengths_dict[xn.Name], xmlPath = (s_p, s_p_n) });
                        pairs.Add((offset + 2), new C { G = xn.Name, T = TagElemntType.TwoByte, N = xn.Attributes.GetNamedItem("v").InnerText + ".max", S = group_lengths_dict[xn.Name], xmlPath = (s_p, s_p_n) });
                        return group_lengths_dict[xn.Name];
                    case "_24": // angle bounds
                        pairs.Add(offset, new C { G = xn.Name, T = TagElemntType.BoundsFloat, N = xn.Attributes.GetNamedItem("v").InnerText, S = group_lengths_dict[xn.Name], xmlPath = (s_p, s_p_n) });
                        return group_lengths_dict[xn.Name];
                    case "_25": // real bounds
                        pairs.Add(offset, new C { G = xn.Name, T = TagElemntType.BoundsFloat, N = xn.Attributes.GetNamedItem("v").InnerText, S = group_lengths_dict[xn.Name], xmlPath = (s_p, s_p_n) });
                        return group_lengths_dict[xn.Name];
                    case "_26": // fraction bounds
                        pairs.Add(offset, new C { G = xn.Name, T = TagElemntType.BoundsFloat, N = xn.Attributes.GetNamedItem("v").InnerText, S = group_lengths_dict[xn.Name], xmlPath = (s_p, s_p_n) });
                        return group_lengths_dict[xn.Name];
                    case "_27":// unmapped - This case isn't found in any tag file
                        Debug.Assert(DebugConfig.NoCheckFails, "Revisar pq se  unmapped");//throw new Exception("Revisar pq se _27 unmapped");
                        pairs.Add(offset, new C { G = xn.Name, T = TagElemntType.Comment, N = xn.Attributes.GetNamedItem("v").InnerText /*+ " (unmapped type(" + xn.Name + "), may cause errors)"*/, S = group_lengths_dict[xn.Name], xmlPath = (s_p, s_p_n) });
                        return group_lengths_dict[xn.Name];
                    case "_28":// unmapped - This case isn't found in any tag file
                        Debug.Assert(DebugConfig.NoCheckFails, "Revisar pq se  unmapped");//throw new Exception("Revisar pq se _28 unmapped");
                        pairs.Add(offset, new C { G = xn.Name, T = TagElemntType.Comment, N = xn.Attributes.GetNamedItem("v").InnerText /*+ " (unmapped type(" + xn.Name + "), may cause errors)"*/, S = group_lengths_dict[xn.Name], xmlPath = (s_p, s_p_n) });
                        return group_lengths_dict[xn.Name];
                    case "_29":// unmapped - dword block flags - only found in ttag
                        Debug.Assert(DebugConfig.NoCheckFails, "Revisar pq se  unmapped");//throw new Exception("Revisar pq se _29 unmapped");
                        pairs.Add(offset, new C { G = xn.Name, T = TagElemntType.FourByte, N = xn.Attributes.GetNamedItem("v").InnerText /*+ " (unmapped type(" + xn.Name + "), may cause errors)"*/, S = group_lengths_dict[xn.Name], xmlPath = (s_p, s_p_n) });
                        return group_lengths_dict[xn.Name];
                    case "_2A":// unmapped - word block flags - only found in ttag
                        Debug.Assert(DebugConfig.NoCheckFails, "Revisar pq se  unmapped");//throw new Exception("Revisar pq se _2A unmapped");
                        pairs.Add(offset, new C { G = xn.Name, T = TagElemntType.Comment, N = xn.Attributes.GetNamedItem("v").InnerText /*+ " (unmapped type(" + xn.Name + "), may cause errors)"*/, S = group_lengths_dict[xn.Name], xmlPath = (s_p, s_p_n) });
                        return group_lengths_dict[xn.Name];
                    case "_2B":// unmapped - byte block flags - only found in ttag
                        Debug.Assert(DebugConfig.NoCheckFails, "Revisar pq se  unmapped");//throw new Exception("Revisar pq se _2B unmapped");
                        pairs.Add(offset, new C { G = xn.Name, T = TagElemntType.Byte, N = xn.Attributes.GetNamedItem("v").InnerText /*+ " (unmapped type(" + xn.Name + "), may cause errors)"*/, S = group_lengths_dict[xn.Name], xmlPath = (s_p, s_p_n) });
                        return group_lengths_dict[xn.Name];
                    case "_2C": // char block index
                        pairs.Add(offset, new C { G = xn.Name, T = TagElemntType.Byte, N = xn.Attributes.GetNamedItem("v").InnerText, S = group_lengths_dict[xn.Name], xmlPath = (s_p, s_p_n) });
                        return group_lengths_dict[xn.Name];
                    case "_2D":// unmapped - char block index custom -- only found in ttag
                        Debug.Assert(DebugConfig.NoCheckFails, "Revisar pq se  unmapped");//throw new Exception("Revisar pq se _2D unmapped");
                        pairs.Add(offset, new C { G = xn.Name, T = TagElemntType.Byte, N = xn.Attributes.GetNamedItem("v").InnerText /*+ " (unmapped type(" + xn.Name + "), may cause errors)"*/, S = group_lengths_dict[xn.Name], xmlPath = (s_p, s_p_n) });
                        return group_lengths_dict[xn.Name];
                    case "_2E": // short block index
                        pairs.Add(offset, new C { G = xn.Name, T = TagElemntType.TwoByte, N = xn.Attributes.GetNamedItem("v").InnerText, S = group_lengths_dict[xn.Name], xmlPath = (s_p, s_p_n) });
                        return group_lengths_dict[xn.Name];
                    case "_2F": // short block index custom
                        pairs.Add(offset, new C { G = xn.Name, T = TagElemntType.TwoByte, N = xn.Attributes.GetNamedItem("v").InnerText, S = group_lengths_dict[xn.Name], xmlPath = (s_p, s_p_n) });
                        return group_lengths_dict[xn.Name];
                    case "_30": // long block index
                        pairs.Add(offset, new C { G = xn.Name, T = TagElemntType.FourByte, N = xn.Attributes.GetNamedItem("v").InnerText, S = group_lengths_dict[xn.Name], xmlPath = (s_p, s_p_n) });
                        return group_lengths_dict[xn.Name];
                    case "_31":// unmapped -- long block index custom
                        Debug.Assert(DebugConfig.NoCheckFails, "Revisar pq se  unmapped");//throw new Exception("Revisar pq se _31 unmapped");
                        pairs.Add(offset, new C { G = xn.Name, T = TagElemntType.FourByte, N = xn.Attributes.GetNamedItem("v").InnerText /*+ " (unmapped type(" + xn.Name + "), may cause errors)"*/, S = group_lengths_dict[xn.Name], xmlPath = (s_p, s_p_n) });
                        return group_lengths_dict[xn.Name];
                    case "_32":// unmapped -- unused
                        Debug.Assert(DebugConfig.NoCheckFails, "Revisar pq se  unmapped");//throw new Exception("Revisar pq se _32 unmapped");
                        pairs.Add(offset, new C { G = xn.Name, T = TagElemntType.Comment, N = xn.Attributes.GetNamedItem("v").InnerText /*+ " (unmapped type(" + xn.Name + "), may cause errors)"*/, S = group_lengths_dict[xn.Name], xmlPath = (s_p, s_p_n) });
                        return group_lengths_dict[xn.Name];
                    case "_33":// unmapped -- unused
                        Debug.Assert(DebugConfig.NoCheckFails, "Revisar pq se  unmapped");//throw new Exception("Revisar pq se _33 unmapped");
                        pairs.Add(offset, new C { G = xn.Name, T = TagElemntType.Comment, N = xn.Attributes.GetNamedItem("v").InnerText /*+ " (unmapped type(" + xn.Name + "), may cause errors)"*/, S = group_lengths_dict[xn.Name], xmlPath = (s_p, s_p_n) });
                        return group_lengths_dict[xn.Name];
                    case "_34": // field pad
                        int length = int.Parse(xn.Attributes.GetNamedItem("s").InnerText);
                        if (length == 1)
                        {
                            pairs.Add(offset, new C { G = xn.Name, T = TagElemntType.Byte, N = xn.Attributes.GetNamedItem("v").InnerText, S = 1, xmlPath = (s_p, s_p_n) });
                        }
                        else if (length == 2)
                        {
                            pairs.Add(offset, new C { G = xn.Name, T = TagElemntType.TwoByte, N = xn.Attributes.GetNamedItem("v").InnerText, S = 2, xmlPath = (s_p, s_p_n) });
                        }
                        else if (length == 4)
                        {
                            pairs.Add(offset, new C { G = xn.Name, T = TagElemntType.FourByte, N = xn.Attributes.GetNamedItem("v").InnerText, S = 4, xmlPath = (s_p, s_p_n) });
                        }
                        else
                        {
                            pairs.Add(offset, new C { G = xn.Name, T = TagElemntType.GenericBlock, N = xn.Attributes.GetNamedItem("v").InnerText, S = length, xmlPath = (s_p, s_p_n) });
                        }
                        return length;
                    case "_35": // len depent type
                        Debug.Assert(DebugConfig.NoCheckFails);
                        //pairs.Add(offset, new C { G = xn.Name, T = TagElemntType.FourByte, N = xn.Attributes.GetNamedItem("v").InnerText + " Index", S = 4, xmlPath = (s_p, s_p_n) }); // Definitely could be wrong, just guessing here.
                        //pairs.Add(offset + 4, new C { G = xn.Name, T = TagElemntType.Mmr3Hash, N = xn.Attributes.GetNamedItem("v").InnerText + " Name", S = 4, xmlPath = (s_p, s_p_n) });
                        int l = int.Parse(xn.Attributes.GetNamedItem("s").InnerText);

                        pairs.Add(offset, new C { G = xn.Name, T = TagElemntType.Comment, N = xn.Attributes.GetNamedItem("v").InnerText /*+ " (unmapped type(" + xn.Name + "), may cause errors)"*/, S = l, xmlPath = (s_p, s_p_n) });
                        return l;
                    case "_36": // explanation
                        if (xn.Attributes.GetNamedItem("v").InnerText != "")
                        {
                            pairs.Add(offset + evalutated_index_PREVENT_DICTIONARYERROR, new C { G = xn.Name, T = TagElemntType.Explanation, N = xn.Attributes.GetNamedItem("v").InnerText, S = group_lengths_dict[xn.Name], xmlPath = (s_p, s_p_n) });
                            evalutated_index_PREVENT_DICTIONARYERROR++;
                        }
                        else
                        {

                        }
                        return 0;
                    case "_37": // TODO revisar regin typed grouper
                        if (xn.Attributes.GetNamedItem("v").InnerText != "")
                        {
                            pairs.Add(offset + evalutated_index_PREVENT_DICTIONARYERROR, new C { G = xn.Name, T = TagElemntType.CustomLikeGrouping, N = xn.Attributes.GetNamedItem("v").InnerText, S = group_lengths_dict[xn.Name], xmlPath = (s_p, s_p_n) });
                            evalutated_index_PREVENT_DICTIONARYERROR++;
                        }
                        else
                        {
                            // what
                        }
                        return 0;
                    case "_38": // struct
                        var temp_index = offset; //+evalutated_index_PREVENT_DICTIONARYERROR
                        
                        FillGeneralExtraData(xn, extra_afl);
                        extra_afl["count"] = 0;
                        
                        //evalutated_index_PREVENT_DICTIONARYERROR++;
                        long current_offset1 = 0;
                        XmlNodeList xnl1 = xn.ChildNodes;
                        Dictionary<long, C?> sub_dic = new Dictionary<long, C?>();
                        foreach (XmlNode xntwo2 in xnl1)
                        {
                            current_offset1 += the_switch_statement(xntwo2, current_offset1, ref sub_dic);
                        }
                        pairs[temp_index] = new C { G = xn.Name, T = TagElemntType.TagStructData, N = xn.Attributes.GetNamedItem("v").InnerText, P = null, B = sub_dic, E = extra_afl, S = current_offset1, xmlPath = (s_p, s_p_n) };
                       
                        return current_offset1;
                    case "_39": // array
                        // TODO revisar
                        extra_afl.Clear();
                        extra_afl["count"] = 0;

                        if (xn.HasChildNodes)
                        {
                            Dictionary<long, C?> subthings = new Dictionary<long, C?>();
                            long current_offset3 = 0;
                            
                            foreach (XmlNode xntwo2 in xn.ChildNodes)
                            {
                                current_offset3 += the_switch_statement(xntwo2, current_offset3, ref subthings);
                            }
                            extra_afl["count"] = int.Parse(xn.Attributes.GetNamedItem("count").InnerText);
                            if (xn.Attributes.GetNamedItem("hash") != null)
                                extra_afl["hash"] = xn.Attributes.GetNamedItem("hash").InnerText;

                            pairs[offset] = new C { G = xn.Name, T = TagElemntType.ArrayFixLen, N = xn.Attributes.GetNamedItem("v").InnerText, B = subthings, E = extra_afl, S = current_offset3 * (int)extra_afl["count"], xmlPath = (s_p, s_p_n) };
                            return current_offset3 * (int)extra_afl["count"];
                        }
                        else
                        {
                            pairs[offset] = new C { G = xn.Name, T = TagElemntType.ArrayFixLen, N = xn.Attributes.GetNamedItem("v").InnerText, E = extra_afl, S = group_lengths_dict[xn.Name], xmlPath = (s_p, s_p_n) };
                            return 0;
                        }
                    /*pairs.Add(offset + evalutated_index_PREVENT_DICTIONARYERROR, new C { G = xn.Name, T = TagElemntType.ArrayFixLen, N = xn.Attributes.GetNamedItem("v").InnerText, S = group_lengths_dict[xn.Name], xmlPath = (s_p, s_p_n) });
                    evalutated_index_PREVENT_DICTIONARYERROR++;
                    XmlNodeList xnl3 = xn.ChildNodes;
                    long current_offset3 = offset;
                    foreach (XmlNode xntwo2 in xnl3)
                    {
                        current_offset3 += the_switch_statement(xntwo2, current_offset3, ref pairs);
                    }
                    return current_offset3;*/
                    case "_3A":// unmapped - Not found in any tag
                        Debug.Assert(DebugConfig.NoCheckFails, "Revisar pq se  unmapped");//throw new Exception("Revisar pq se _3A unmapped");
                        pairs.Add(offset, new C { G = xn.Name, T = TagElemntType.Comment, N = xn.Attributes.GetNamedItem("v").InnerText /*+ " (unmapped type(" + xn.Name + "), may cause errors)"*/, S = group_lengths_dict[xn.Name], xmlPath = (s_p, s_p_n) });
                        return group_lengths_dict[xn.Name];
                    case "_3B":
                        return group_lengths_dict[xn.Name];
                    case "_3C": // byte
                        pairs.Add(offset, new C { G = xn.Name, T = TagElemntType.Byte, N = xn.Attributes.GetNamedItem("v").InnerText, S = 1, xmlPath = (s_p, s_p_n) });
                        return group_lengths_dict[xn.Name];
                    case "_3D": // word
                        pairs.Add(offset, new C { G = xn.Name, T = TagElemntType.TwoByte, N = xn.Attributes.GetNamedItem("v").InnerText, S = 2, xmlPath = (s_p, s_p_n) });
                        return group_lengths_dict[xn.Name];
                    case "_3E": // dword
                        pairs.Add(offset, new C { G = xn.Name, T = TagElemntType.FourByte, N = xn.Attributes.GetNamedItem("v").InnerText, S = 4, xmlPath = (s_p, s_p_n) });
                        return group_lengths_dict[xn.Name];
                    case "_3F": // qword
                        pairs.Add(offset, new C { G = xn.Name, T = TagElemntType.Pointer, N = xn.Attributes.GetNamedItem("v").InnerText, S = 8, xmlPath = (s_p, s_p_n) });
                        return group_lengths_dict[xn.Name];
                    case "_40": // block
                        extra_afl.Clear();
                        if (xn.ChildNodes.Count > 0)
                        {
                            Dictionary<long, C> subthings = new Dictionary<long, C>();
                            XmlNodeList xnl2 = xn.ChildNodes;
                            FillGeneralExtraData(xn, extra_afl);

                            long current_offset2 = 0;
                            foreach (XmlNode xntwo2 in xnl2)
                            {
                                current_offset2 += the_switch_statement(xntwo2, current_offset2, ref subthings); // its gonna append that to the main, rather than our struct
                            }

                            pairs.Add(offset, new C { G = xn.Name, T = TagElemntType.Tagblock, N = xn.Attributes.GetNamedItem("v").InnerText, B = subthings, E = extra_afl, S = current_offset2, xmlPath = (s_p, s_p_n) });

                        }
                        else
                        {
                            pairs.Add(offset, new C { G = xn.Name, T = TagElemntType.Tagblock, N = xn.Attributes.GetNamedItem("v").InnerText, S = 20, xmlPath = (s_p, s_p_n) });
                        }
                        return group_lengths_dict[xn.Name];
                    case "_41": // tag reference
                        pairs.Add(offset, new C { G = xn.Name, T = TagElemntType.TagRef, N = xn.Attributes.GetNamedItem("v").InnerText, S = 28, xmlPath = (s_p, s_p_n) });
                        return group_lengths_dict[xn.Name];
                    case "_42": // data

                        pairs.Add(offset, new C { G = xn.Name, T = TagElemntType.TagData, N = xn.Attributes.GetNamedItem("v").InnerText, S = 24, E = extra_afl, xmlPath = (s_p, s_p_n) });
                        return group_lengths_dict[xn.Name];
                    case "_43":// Mapping these to fix errors. The new length seems to fix some issues. Check pfnd > mobileNavMeshes to understand.
                        /*pairs.Add(offset, new C { G = xn.Name, T = TagElemntType.Pointer, N = xn.Attributes.GetNamedItem("v").InnerText });
						pairs.Add(offset + 8, new C { G = xn.Name, T = TagElemntType.mmr3Hash, N = xn.Attributes.GetNamedItem("v").InnerText});
						pairs.Add(offset + 12, new C { G = xn.Name, T = TagElemntType.4Byte, N = xn.Attributes.GetNamedItem("v").InnerText });*/
                        if (xn.ChildNodes.Count > 0)
                        {
                            Dictionary<long, C> subthings = new Dictionary<long, C>();
                            XmlNodeList xnl2 = xn.ChildNodes;
                            long current_offset2 = 0;
                            FillGeneralExtraData(xn, extra_afl);
                            foreach (XmlNode xntwo2 in xnl2)
                            {
                                current_offset2 += the_switch_statement(xntwo2, current_offset2, ref subthings); // its gonna append that to the main, rather than our struct
                            }

                            pairs.Add(offset, new C { G = xn.Name, T = TagElemntType.ResourceHandle, N = xn.Attributes.GetNamedItem("v").InnerText, B = subthings, E = extra_afl, S = current_offset2, xmlPath = (s_p, s_p_n) });

                        }
                        else
                        {
                            pairs.Add(offset, new C { G = xn.Name, T = TagElemntType.ResourceHandle, N = xn.Attributes.GetNamedItem("v").InnerText, S = 20, xmlPath = (s_p, s_p_n) });
                        }
                        return group_lengths_dict[xn.Name];
                    case "_44":// unmapped -- data path
                        //Debug.Assert(DebugConfig.NoCheckFails, "Revisar pq se  unmapped");//throw new Exception("Revisar pq se _44 unmapped");
                        pairs.Add(offset, new C { G = xn.Name, T = TagElemntType.String, N = xn.Attributes.GetNamedItem("v").InnerText /*+ " (unmapped type(" + xn.Name + "), may cause errors)"*/, S = group_lengths_dict[xn.Name], xmlPath = (s_p, s_p_n) });
                        return group_lengths_dict[xn.Name];
                    case "_45":// unmapped
                        Debug.Assert(DebugConfig.NoCheckFails, "Revisar pq se  unmapped");//throw new Exception("Revisar pq se _45 unmapped");
                        pairs.Add(offset, new C { G = xn.Name, T = TagElemntType.Comment, N = xn.Attributes.GetNamedItem("v").InnerText /*+ " (unmapped type(" + xn.Name + "), may cause errors)"*/, S = 4, xmlPath = (s_p, s_p_n) });
                        return group_lengths_dict[xn.Name];
                    case "_69":// unmapped693A unmapped");
                        pairs.Add(offset, new C { G = xn.Name, T = TagElemntType.Comment, N = xn.Attributes.GetNamedItem("v").InnerText + " (unmapped type(" + xn.Name + "), fuck this one _intValue specifically)", xmlPath = (s_p, s_p_n) });
                        return group_lengths_dict[xn.Name];

                }
                return group_lengths_dict[xn.Name];
            }
            private static void FillGeneralExtraData(XmlNode xn, Dictionary<string, object> extra_afl)
            {
                extra_afl.Clear();
                foreach (XmlAttribute item in xn.Attributes)
                {
                    if (item.Name == "v")
                        continue;
                    extra_afl[item.Name] = item.InnerText;
                }
            }
            private static void FillGeneralExtraDataSelective(XmlNode xn, Dictionary<string, object> extra_afl)
            {
                extra_afl.Clear();
                AddAtribute(xn, extra_afl, "hash");
                AddAtribute(xn, extra_afl, "T1");
                AddAtribute(xn, extra_afl, "T2");
                AddAtribute(xn, extra_afl, "hashTR0");
                AddAtribute(xn, extra_afl, "hashTR1");
                AddAtribute(xn, extra_afl, "size");
                AddAtribute(xn, extra_afl, "ui2");
                AddAtribute(xn, extra_afl, "ui3");
                AddAtribute(xn, extra_afl, "ui4");
                AddAtribute(xn, extra_afl, "comp");
                AddAtribute(xn, extra_afl, "ui6");
                AddAtribute(xn, extra_afl, "aottr");
                AddAtribute(xn, extra_afl, "au0");
                AddAtribute(xn, extra_afl, "ul1");
                AddAtribute(xn, extra_afl, "ul2");
                AddAtribute(xn, extra_afl, "ul3");

                static void AddAtribute(XmlNode xn, Dictionary<string, object> extra_afl, string name)
                {
                    if (xn.Attributes.GetNamedItem(name) != null)
                        extra_afl[name] = xn.Attributes.GetNamedItem(name).InnerText;
                }

            }

            public static Dictionary<string, long> group_lengths_dict = new()
            {
                { "_0", 32 }, // _field_string
				{ "_1", 256 }, // _field_long_string
				{ "_2", 4 }, // _field_string_id
				{ "_3", 4 }, // ## Not found in any tag type
				{ "_4", 1 }, // _field_char_integer
				{ "_5", 2 }, // _field_short_integer
				{ "_6", 4 }, // _field_long_integer
				{ "_7", 8 }, // _field_int64_integer
				{ "_8", 4 }, // _field_angle
				{ "_9", 4 }, // _field_tag
				{ "_A", 1 }, // _field_char_enum
				{ "_B", 2 }, // _field_short_enum
				{ "_C", 4 }, // _field_long_enum
				{ "_D", 4 }, // _field_long_flags
				{ "_E", 2 }, // _field_word_flags
				{ "_F", 1 }, // _field_byte_flags
				{ "_10", 4 }, // _field_point_2d -- 2 2bytes?
				{ "_11", 8 }, // _field_rectangle_2d -> 4 short
				{ "_12", 4 }, // _field_rgb_color -- hex color codes --- rgb pixel 32 - it's technically only 3 bytes but the final byte is FF
				{ "_13", 4 }, // _field_argb_color --- argb pixel 32
				{ "_14", 4 }, // _field_real
				{ "_15", 4 }, // _field_real_fraction
				{ "_16", 8 }, // _field_real_point_2d
				{ "_17", 12 }, // _field_real_point_3d
				{ "_18", 8 }, // _field_real_vector_2d -- 
				{ "_19", 12 }, // _field_real_vector_3d
				{ "_1A", 16 }, // _field_real_quaternion
				{ "_1B", 8 }, // _field_real_euler_angles_2d
				{ "_1C", 12 }, // _field_real_euler_angles_3d
				{ "_1D", 12 }, // _field_real_plane_2d
				{ "_1E", 16 }, // _field_real_plane_3d
				{ "_1F", 12 }, // _field_real_rgb_color
				{ "_20", 16 }, // _field_real_argb_color
				{ "_21", 4 }, // _field_real_hsv_colo
				{ "_22", 4 }, // _field_real_ahsv_color
				{ "_23", 4 }, // _field_short_bounds
				{ "_24", 8 }, // _field_angle_bounds
				{ "_25", 8 }, // _field_real_bounds
				{ "_26", 8 }, // _field_real_fraction_bounds
				{ "_27", 4 }, // ## Not found in any tag type
				{ "_28", 4 }, // ## Not found in any tag type
				{ "_29", 4 }, // _field_long_block_flags
				{ "_2A", 2 }, // _field_word_block_flags
				{ "_2B", 1 }, // _field_byte_block_flags
				{ "_2C", 1 }, // _field_char_block_index
				{ "_2D", 1 }, // _field_custom_char_block_index
				{ "_2E", 2 }, // _field_short_block_index
				{ "_2F", 2 }, // _field_custom_short_block_index
				{ "_30", 4 }, // _field_long_block_index
				{ "_31", 4 }, // _field_custom_long_block_index
				{ "_32", 4 }, // ## Not found in any tag type
				{ "_33", 4 }, // ## Not found in any tag type
				{ "_34", 4 }, // _field_pad ## variable length
				{ "_35", 4 }, // 'field_skip' ## iirc
				{ "_36", 0 }, // _field_explanation
				{ "_37", 0 }, // _field_custom
				{ "_38", 0 }, // _field_struct
				{ "_39", 32 }, // _field_array
				{ "_3A", 4 },
                { "_3B", 0 }, // ## end of struct or something
				{ "_3C", 1 }, // _field_byte_integer
				{ "_3D", 2 }, // _field_word_integer
				{ "_3E", 4 }, // _field_dword_integer
				{ "_3F", 8 }, // _field_qword_integer
				{ "_40", 20 }, // _field_block_v2
				{ "_41", 28 }, // _field_reference_v2
				{ "_42", 24 }, // _field_data_v2

				{ "_43", 16 }, // ok _field_resource_handle

				{ "_44", 256 },// revisar original 4 --- data path
				{ "_45", 16 },
            };

            public static string TagsPath
            {
                get => _tagsPath; set
                {
                    lock (_tagsPath)
                    {
                        _tagsPath = value;
                    }

                }
            }
        }
    }
}
