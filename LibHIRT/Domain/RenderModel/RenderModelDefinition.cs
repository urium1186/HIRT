using LibHIRT.TagReader;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LibHIRT.Domain.RenderModel
{
    public struct render_model_permutation
    {
        public int name_id;
        public string name;
        public short mesh_index;
        public short mesh_count;
        public int clone_name_id;
        public string clone_name;
    }
    public struct render_model_region {
        public int name_id;
        public string name;

        public render_model_permutation[] permutations;
    }
    public class RenderModelDefinition
    {
        
        render_model_region[] _regions;
        RenderGeometry _render_geometry;
        RenderModelMarkerGroup[] _marker_groups;
        ModelBone[] _nodes;

        public TagInstance TagInstance { get; set; }
        public render_model_region[] Regions { get => _regions; set => _regions = value; }
        public RenderGeometry Render_geometry { get => _render_geometry; set => _render_geometry = value; }
        public ModelBone[] Nodes { get => _nodes; set => _nodes = value; }
        public RenderModelMarkerGroup[] Marker_groups { get => _marker_groups; set => _marker_groups = value; }
    }
}
