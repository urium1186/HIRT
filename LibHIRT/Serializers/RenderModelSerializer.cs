using LibHIRT.Data;
using LibHIRT.Domain;
using LibHIRT.Domain.DX;
using LibHIRT.Domain.Geometry;
using LibHIRT.Domain.RenderModel;
using LibHIRT.Files;
using LibHIRT.Files.FileTypes;
using LibHIRT.ModuleUnpacker;
using LibHIRT.TagReader;
using SharpDX.MediaFoundation;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Xml.Linq;

namespace LibHIRT.Serializers
{
    public class RenderModelSerializer : SerializerBase<RenderModelDefinition>
    {
        static string _filePath = "";
        static RenderModelFile _file;

        public static RenderModelDefinition Deserialize(Stream stream, RenderModelFile file)
        {
            _file = file;
            _filePath = file.Path_string;
            var reader = new BinaryReader(stream);
            return new RenderModelSerializer().Deserialize(reader);
        }
        protected override void OnDeserialize(BinaryReader reader, RenderModelDefinition obj)
        {
            if (obj == null)
            {
                return;
            }
            tagParse = new TagParseControl("", "mode", null, reader.BaseStream);
            tagParse.readFile();
            try
            {
                obj.TagInstance = tagParse.RootTagInst;
                ReadRenderModelDefinition(obj);
            }
            catch (Exception e)
            {

            }
        }
        void ReadRenderModelDefinition(RenderModelDefinition obj)
        {
            var g_h_id = (TagParse.RootTagInst["parent model"] as TagRef).Ref_id_int;
            var g_h_mid = (TagParse.RootTagInst["parent model"] as TagRef).Ref_id_center_int;
            var g_h_sub = (TagParse.RootTagInst["parent model"] as TagRef).Ref_id_sub_int;
            /*var fil_id=(_file.Parent as ModuleFile).GetFileByGlobalId((int)g_h_id);
            var fil_mid=(_file.Parent as ModuleFile).GetFileByGlobalId((int)g_h_mid);
            var fil_sub=(_file.Parent as ModuleFile).GetFileByGlobalId((int)g_h_sub);
            */
            ReadRegions(obj);
            ReadBoneNodes(obj);
            ReadMarkerGroups(obj);
            RenderGeometry temp = new RenderGeometry();
            
            ReadRenderGeometry(ref temp);
            obj.Render_geometry = temp;
        }

        private void ReadBoneNodes(RenderModelDefinition _obj)
        {
            ListTagInstance temp = TagParse.RootTagInst["nodes"] as ListTagInstance;
            if (temp == null)
                return;
            _obj.Nodes = new ModelBone[temp.Count];
            for (int i = 0; i < temp.Count; i++)
            {
                var obj = temp[i];
                if (_obj.Nodes[i] == null)
                {
                    _obj.Nodes[i] = new ModelBone();
                }
                ReadBoneNode(_obj.Nodes[i], temp[i]);
                _obj.Nodes[i].Index = i;
                if (_obj.Nodes[i].ParentIndex != -1)
                    _obj.Nodes[i].Parent = _obj.Nodes[_obj.Nodes[i].ParentIndex];

                if (_obj.Nodes[i].FirstChildIndex != -1)
                {
                    if (_obj.Nodes[_obj.Nodes[i].FirstChildIndex] == null)
                    {
                        _obj.Nodes[_obj.Nodes[i].FirstChildIndex] = new ModelBone();
                    }
                    _obj.Nodes[i].FirstChild = _obj.Nodes[_obj.Nodes[i].FirstChildIndex];
                }
                if (_obj.Nodes[i].NextSiblingIndex != -1)
                {
                    if (_obj.Nodes[_obj.Nodes[i].NextSiblingIndex] == null)
                    {
                        _obj.Nodes[_obj.Nodes[i].NextSiblingIndex] = new ModelBone();
                    }
                    _obj.Nodes[i].NextSibling = _obj.Nodes[_obj.Nodes[i].NextSiblingIndex];
                }
            }
        }

        private void ReadMarkerGroups(RenderModelDefinition _obj)
        {
            ListTagInstance markerGroups = TagParse.RootTagInst["marker groups"] as ListTagInstance;
            if (markerGroups == null)
            { return; }
            _obj.Marker_groups = new RenderModelMarkerGroup[markerGroups.Count];
            for (int i = 0; i < markerGroups.Count; i++)
            {
                _obj.Marker_groups[i] = new RenderModelMarkerGroup();
                _obj.Marker_groups[i].Name = (markerGroups[i]["name"] as Mmr3Hash).Str_value;
                ListTagInstance markers = markerGroups[i]["markers"] as ListTagInstance;
                if (markers == null)
                {
                    _obj.Marker_groups[i].Markers = new RenderModelMarker[0];
                    continue;
                }
                _obj.Marker_groups[i].Markers = new RenderModelMarker[markers.Count];
                for (int j = 0; j < markers.Count; j++)
                {
                    var marker = markers[j];
                    var trs = marker["translation"] as Point3D;
                    var rts = marker["rotation"] as TagReader.Quaternion;
                    float scl = (marker["scale"] as Float).Value;
                    var dir = marker["direction"] as Point3D;
                    _obj.Marker_groups[i].Markers[j] = new RenderModelMarker
                    {
                        Index = j,
                        RegionIndex = (sbyte)marker["region index"].AccessValue,
                        PermutationIndex = (Int32)marker["permutation index"].AccessValue,
                        NodeIndex = (Int16)marker["node index"].AccessValue,
                        HasNodeRelativeDirection = (marker["flags"] as FlagGroup).Options_v[0],
                        Translation = new System.Numerics.Vector3
                        {
                            X = trs.X,
                            Y = trs.Y,
                            Z = trs.Z,
                        },
                        Rotation = new System.Numerics.Quaternion
                        {
                            X = rts.X,
                            Y = rts.Y,
                            Z = rts.Z,
                            W = rts.W
                        },
                        Scale = new System.Numerics.Vector3
                        {
                            X = scl,
                            Y = scl,
                            Z = scl
                        },
                        Direction = new System.Numerics.Vector3
                        {
                            X = dir.X,
                            Y = dir.Y,
                            Z = dir.Z,
                        },
                    };
                }

            }
        }

        private void ReadBoneNode(ModelBone bone, TagInstance obj)
        {
            bone.Name = (obj["name"] as Mmr3Hash).Str_value;
            bone.ParentIndex = (Int16)obj["parent node"].AccessValue;
            bone.FirstChildIndex = (Int16)obj["first child node"].AccessValue;
            bone.NextSiblingIndex = (Int16)obj["next sibling node"].AccessValue;
            var trs = obj["default translation"] as Point3D;
            var trsFP = obj["distance from parent"] as Float;
            var rts = obj["default rotation"] as TagReader.Quaternion;

            bone.Traslation = new System.Numerics.Vector3
            {
                X = trs.X,
                Y = trs.Y,
                Z = trs.Z,
            };

            bone.Rotation = new System.Numerics.Quaternion
            {
                X = rts.X,
                Y = rts.Y,
                Z = rts.Z,
                W = rts.W
            };

            bone.Scale = new System.Numerics.Vector3
            {
                X = 1,
                Y = 1,
                Z = 1,
            };

            bone.DistanceFromParent = trsFP.Value;
        }
        void ReadRegions(RenderModelDefinition obj)
        {
            ListTagInstance temp = TagParse.RootTagInst["regions"] as ListTagInstance;
            if (temp == null)
                return;
            obj.Regions = new render_model_region[temp.Count];
            for (int i = 0; i < temp.Count; i++)
            {
                obj.Regions[i].name_id = (int)temp[i]["name"].AccessValue;
                ListTagInstance perms = temp[i]["permutations"] as ListTagInstance;
                if (perms != null)
                {
                    obj.Regions[i].permutations = new render_model_permutation[perms.Count];
                    for (int j = 0; j < perms.Count; j++)
                    {
                        obj.Regions[i].permutations[j].name_id = (int)perms[j]["name"].AccessValue;
                        obj.Regions[i].permutations[j].mesh_index = (short)perms[j]["mesh index"].AccessValue;
                        obj.Regions[i].permutations[j].mesh_count = (short)perms[j]["mesh count"].AccessValue;
                        obj.Regions[i].permutations[j].clone_name_id = (int)perms[j]["clone name"].AccessValue;
                    }
                }
            }
        }
        void ReadRenderGeometry(ref RenderGeometry obj)
        {
            if (obj == null || this.TagParse == null)
                return;
            obj = RenderGeometrySerializer.Deserialize(null, _file, TagParse.RootTagInst["render geometry"] as RenderGeometryTag);
        }

    }
}
