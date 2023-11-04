using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

using static LibHIRT.TagReader.TagLayoutsV2;

namespace LibHIRT.TagReader
{
    public static class TagXmlParseV2
    {
        static int evalutated_index_PREVENT_DICTIONARYERROR = 99999;
        static Dictionary<string, Dictionary<int, Template?>> readedTags = new Dictionary<string, Dictionary<int, Template?>>();
        static string _tagsPath = "";

        public static void reset()
        {
            evalutated_index_PREVENT_DICTIONARYERROR = 99999;
            readedTags.Clear();
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

        public static Dictionary<int, Template?> parse_the_mfing_xmls(string file_to_find)
        {

            lock (readedTags)
            {
                if (readedTags.ContainsKey(file_to_find))
                    return readedTags[file_to_find];

                evalutated_index_PREVENT_DICTIONARYERROR = 99999;

                Dictionary<int, Template?> poopdict = new();
                string predicted_file = GetXmlPath(ref file_to_find);

                if (File.Exists(predicted_file))
                {
                    try
                    {
                        XmlDocument xd = new XmlDocument();
                        xd.Load(predicted_file);
                        XmlNode xn = xd.SelectSingleNode("root");
                        int current_offset = 0;
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
        static int the_switch_statement(XmlNode xn, int offset, ref Dictionary<int, Template?> pairs)
        {
            var s_p = getPathOfElement(xn);
            string s_p_n = getPathOfElementNamed(xn);
            Dictionary<string, object> extra_afl = new Dictionary<string, object>();
            FillGeneralExtraData(xn, extra_afl);
            TagElemntTypeV2 t_type = TagElemntTypeV2.Block;
            if (xn.Name == "root")
                t_type = TagElemntTypeV2.RootTagInstance;
            else
                t_type = (TagElemntTypeV2)Convert.ToInt32(xn.Name.Replace("_",""), 16);
            int t_size = 0;
            
            if (xn.Attributes.GetNamedItem("s") != null)
                t_size = int.Parse(xn.Attributes.GetNamedItem("s").InnerText);
            int class_size = -1;
            if (xn.Attributes.GetNamedItem("size") != null)
                class_size = int.Parse(xn.Attributes.GetNamedItem("size").InnerText);

            switch (t_type)
            {
                case TagElemntTypeV2.Undefined:
                    return 0;
                case TagElemntTypeV2.RootTagInstance:
                    extra_afl.Clear();
                    if (xn.ChildNodes.Count > 0)
                    {
                        Dictionary<int, Template> subthings_r = new Dictionary<int, Template>();
                        XmlNodeList xnl2_r = xn.ChildNodes;

                        FillGeneralExtraData(xn, extra_afl);

                        int current_offset2_r = 0;
                        foreach (XmlNode xntwo2 in xnl2_r)
                        {
                            current_offset2_r += the_switch_statement(xntwo2, current_offset2_r, ref subthings_r); // its gonna append that to the main, rather than our struct
                        }

                        pairs.Add(offset, new P { G = xn.Name, T = t_type, N = "root", B = subthings_r, E = extra_afl, S = current_offset2_r, xmlPath = ("root", "root") });
                        Debug.Assert(current_offset2_r == class_size);
                        return current_offset2_r;
                    }
                    else
                    {
                        pairs.Add(offset, new P { G = xn.Name, T = TagElemntTypeV2.Block, N = xn.Attributes.GetNamedItem("v").InnerText, S = 20, xmlPath = (s_p, s_p_n) });
                    }
                    return 0;
                case TagElemntTypeV2.CharEnum:
                case TagElemntTypeV2.ShortEnum:
                case TagElemntTypeV2.LongEnum:
                    Dictionary<int, string> childdictionary1 = new();
                    for (int iu = 0; iu < xn.ChildNodes.Count; iu++)
                    {
                        childdictionary1.Add(iu, xn.ChildNodes[iu].Attributes.GetNamedItem("v").InnerText);
                    }
                    pairs.Add(offset, new E { G = xn.Name, N = xn.Attributes.GetNamedItem("v").InnerText, T = t_type, STR = childdictionary1, S = t_size, E = extra_afl, xmlPath = (s_p, s_p_n) });
                    return t_size;
                case TagElemntTypeV2.LongFlags:
                case TagElemntTypeV2.WordFlags:
                case TagElemntTypeV2.ByteFlags:
                    Dictionary<int, string> childdictionary4 = new();
                    for (int iu = 0; iu < xn.ChildNodes.Count; iu++)
                    {
                        childdictionary4.Add(iu, xn.ChildNodes[iu].Attributes.GetNamedItem("v").InnerText);
                    }
                    pairs.Add(offset, new F { G = xn.Name, N = xn.Attributes.GetNamedItem("v").InnerText, T = t_type,STR = childdictionary4, S = t_size, E = extra_afl, xmlPath = (s_p, s_p_n) });
                    return t_size;
                case TagElemntTypeV2.Struct:
                    var temp_index = offset;
                    int current_offset1 = 0;
                    XmlNodeList xnl1 = xn.ChildNodes;
                    Dictionary<int, Template?> sub_dic = new Dictionary<int, Template?>();
                    foreach (XmlNode xntwo2 in xnl1)
                    {
                        current_offset1 += the_switch_statement(xntwo2, current_offset1, ref sub_dic);
                    }
                    pairs[temp_index] = new P { G = xn.Name, T = t_type, N = xn.Attributes.GetNamedItem("v").InnerText, B = sub_dic, E = extra_afl, S = current_offset1, xmlPath = (s_p, s_p_n) };
                    Debug.Assert(current_offset1 == class_size);
                    return current_offset1;
                case TagElemntTypeV2.Array:
                    Dictionary<int, Template?> subthings = new Dictionary<int, Template?>();
                    int current_offset3 = 0;
                    int count = int.Parse(extra_afl["count"].ToString());
                    foreach (XmlNode xntwo2 in xn.ChildNodes)
                    {
                        current_offset3 += the_switch_statement(xntwo2, current_offset3, ref subthings);
                    }
                    pairs[offset] = new P { G = xn.Name, T = t_type, N = xn.Attributes.GetNamedItem("v").InnerText, B = subthings, E = extra_afl, S = current_offset3 * count, xmlPath = (s_p, s_p_n) };
                    Debug.Assert(current_offset3 == class_size);
                    return current_offset3 * count;
                case TagElemntTypeV2.Block:
                case TagElemntTypeV2.ResourceHandle:
                    if (xn.ChildNodes.Count > 0)
                    {
                        Dictionary<int, Template> subthings_b = new Dictionary<int, Template>();
                        XmlNodeList xnl2 = xn.ChildNodes;
                        FillGeneralExtraData(xn, extra_afl);

                        int current_offset2 = 0;
                        foreach (XmlNode xntwo2 in xnl2)
                        {
                            current_offset2 += the_switch_statement(xntwo2, current_offset2, ref subthings_b);
                        }

                        pairs.Add(offset, new P { G = xn.Name, T = t_type, N = xn.Attributes.GetNamedItem("v").InnerText, B = subthings_b, E = extra_afl, S = current_offset2, xmlPath = (s_p, s_p_n) });

                    }
                    else
                    {
                        pairs.Add(offset, new P { G = xn.Name, T = t_type, N = xn.Attributes.GetNamedItem("v").InnerText, S = 20, xmlPath = (s_p, s_p_n) });
                    }
                    return t_size;
                default:
                    int key = offset;
                    
                        if (t_size == 0)
                        {
                            key = offset + evalutated_index_PREVENT_DICTIONARYERROR;
                            evalutated_index_PREVENT_DICTIONARYERROR++;
                        }
                        else { 
                        }
                        
                    
                    pairs.Add(key, new C { G = xn.Name, T = t_type, N = xn.Attributes.GetNamedItem("v").InnerText, S = t_size, E = extra_afl, xmlPath = (s_p, s_p_n) });
                    return t_size;
            }
            return t_size;
        }

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
