using LibHIRT.Domain;
using LibHIRT.Domain.RenderModel;
using LibHIRT.Files.FileTypes;
using LibHIRT.TagReader;

namespace LibHIRT.Serializers
{
    public class RenderModelSerializer : SerializerBase<RenderModelDefinition>
    {
        string _filePath = "";
        RenderModelFile _file;
        private RootTagInstance rootTagInst;

        private RenderModelSerializer(RenderModelFile file, string filePath)
        {
            _file = file;
            _filePath = filePath;
        }

        public static RenderModelDefinition Deserialize(RenderModelFile file)
        {
            TagParseControlFiltter parseControlFiltter = new TagParseControlFiltter
            {
                ClassFilter = new System.Collections.Generic.List<ClassFilter>()
            };
            parseControlFiltter.ClassFilter.Add(
            new ClassFilter // render geometry
            {
                Hash = "E423D497BA42B08FA925E0B06C3C363A",
                Full = true
            });
            parseControlFiltter.ClassFilter.Add(
            new ClassFilter // materials
            {
                Hash = "735913B1C54DA5343116909973E5B5B0",
                Full = true
            });
            parseControlFiltter.ClassFilter.Add(
            new ClassFilter // regions
            {
                Hash = "11BC235FD7426BB7466C8CA31358EE1D",
                Full = true
            });
            parseControlFiltter.ClassFilter.Add(
            new ClassFilter // nodes
            {
                Hash = "B75344B72E40E3D6D087158AEA22BFEF",
                Full = true
            });
            parseControlFiltter.ClassFilter.Add(
                new ClassFilter // marker groups
                {
                    Hash = "FA9406E60D4DECA98C23E68915BE6CF6",
                    Full = true
                });

            var layout = file.Deserialized(parseControlFiltter).TagParse.RootTagInst;
            return new RenderModelSerializer(file, file.Path_string).Deserialize(layout);
        }
        protected override void OnDeserialize(BinaryReader reader, RenderModelDefinition obj)
        {
        }

        protected override void OnDeserialize(TagInstance tagInstance, RenderModelDefinition obj)
        {
            rootTagInst = (RootTagInstance)tagInstance;  
            obj.TagInstance = rootTagInst;
            ReadRenderModelDefinition(obj);
        }

        void ReadRenderModelDefinition(RenderModelDefinition obj)
        {
            /*var g_h_id = (rootTagInst["parent model"] as TagRef).Ref_id_int;
            var g_h_mid = (rootTagInst["parent model"] as TagRef).Ref_id_center_int;
            var g_h_sub = (rootTagInst["parent model"] as TagRef).Ref_id_sub_int;
            var fil_id=(_file.Parent as ModuleFile).GetFileByGlobalId((int)g_h_id);
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
            ListTagInstance temp = rootTagInst["nodes"] as ListTagInstance;
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
            ListTagInstance markerGroups = rootTagInst["marker groups"] as ListTagInstance;
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
            ListTagInstance temp = rootTagInst["regions"] as ListTagInstance;
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
            if (obj == null || rootTagInst == null)
                return;
            obj = RenderGeometrySerializer.Deserialize(null, _file, rootTagInst["render geometry"] as RenderGeometryTag);
        }

    }
}
