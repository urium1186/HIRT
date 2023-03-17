﻿using System.Numerics;
using LibHIRT.Common;

namespace LibHIRT.Domain.RenderModel
{
    public struct RenderModelMarkerGroup {
        public string Name { get; set; }
        public RenderModelMarker[] Markers { get; set; }
    }

    public struct RenderModelMarker {
        public int Index { get; set; }
        public int RegionIndex { get; set; }
        public int PermutationIndex { get; set; }
        public int NodeIndex { get; set; }
        public Vector3 Translation { get; set; }
        public Quaternion Rotation { get; set; }
        public Vector3 Scale { get; set; }
        public Vector3 Direction { get; set; }

        public bool HasNodeRelativeDirection { get; set; }

    }
    public class ModelBone
    {

        public string Name { get; set; }
        public int Index { get; set; }
        public Matrix4x4 LocalTransform { get; set; }
        public Matrix4x4 GlobalTransform { get; set; }


        public bool LoadedLocalTransform { get; set; }
        public bool LoadedGlobalTransform { get; set; }
        public Matrix4x4 Offset { get; set; }

        public Vector3 Traslation { get; set; }
        
        public Quaternion Rotation { get; set; }
        public Vector3 Scale { get; set; }

        public float DistanceFromParent { get; set; }
        public ModelBone Parent { get; set; }
        public ModelBone FirstChild { get; set; }
        public ModelBone NextSibling { get; set; }

        public int ParentIndex { get; set; }
        public int FirstChildIndex { get; set; }
        public int NextSiblingIndex { get; set; }

        /*
         
         inverse forward" />
		<_19 v="inverse left" />
		<_19 v="inverse up" />
		<_17 v="inverse position" />
		<_14 v="inverse scale" />
		<_14 v="distance from parent" />
         
         */

        // Get the gloal transformation from his parent
        static public void calculateGlobalTransformation(ModelBone bone)
        {
            if (bone.Parent == null)
            {
                bone.GlobalTransform = bone.LocalTransform;
            }
            else
            {
                bone.GlobalTransform = bone.Parent.GlobalTransform * bone.LocalTransform;
            }
            bone.LoadedGlobalTransform = true;  
        }

        // Get Local transfomr from Traslation Rotation Scale 
        // Assimp.Matrix4x4 do not have TRS so we need to convert it
        static public void calculateLocalTransformation(ModelBone bone)
        {
            bone.LocalTransform = NumericExtensions.TRS(bone.Traslation, bone.Rotation, bone.Scale);
            bone.LoadedLocalTransform = true;
        }

        // get the Assimp.Matrix4x4 from SharpDX.Matrix 
       

        // Get Local transfomr from Traslation Rotation Scale SharpDX.Matrix 



    }


}
