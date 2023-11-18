using Assimp;
using HaloInfiniteResearchTools.Common.Extensions;
using LibHIRT.Data;
using LibHIRT.Data.Geometry;
using LibHIRT.Domain;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Numerics;
using System.Reflection.Metadata.Ecma335;

namespace HaloInfiniteResearchTools.Assimport
{
    public class MeshBuilder
    {
        private readonly ISceneContext _context;
        public Mesh Mesh { get; }

        private Bone[]? bones_ref;
        private uint bone_index;
        private bool use_dual_quat; // no idea what this means
        private VertType mesh_vert_type;
        //public Dictionary<short, Bone> Bones { get; }
        //public Dictionary<string, Bone> BoneNames { get; }
        public Dictionary<uint, uint> VertexLookup { get; }
        public short SkinCompoundId { get; }
        private readonly s_mesh _object;
        public MeshBuilder(ISceneContext context, s_mesh obj, S3DGeometrySubMesh submesh, Bone[]? bones)
        {
            _context = context;
            _object = obj;

            var meshName = _object.Name;

            bones_ref = bones;
            // im not really sure if the indexes go higher than 128, but we'll use a unit anyway
            use_dual_quat = obj.UseDualQuat;
            mesh_vert_type = obj.VertType;
            bone_index = (uint)obj.RigidNodeIndex;

            Mesh = new Mesh(meshName, PrimitiveType.Triangle);

            if (bones != null)
                Mesh.Bones.AddRange(bones); // apply the bones

            //Bones = new Dictionary<short, Bone>();
            //BoneNames = new Dictionary<string, Bone>();
            VertexLookup = new Dictionary<uint, uint>();
        }
        public Mesh Build()
        {
            AddVertices(_object.LODRenderData[0]);
            AddFaces(_object.LODRenderData[0]);
            AddInterleavedData(_object.LODRenderData[0]);
            return Mesh;
        }


        // create a piramide whit a entry h
        public static Mesh CreatePiramide(float h, string meshName)
        {
            var mesh = new Mesh(meshName, PrimitiveType.Triangle);
            mesh.Vertices.Add(new Vector3D(0, 0, 0));
            mesh.Vertices.Add(new Vector3D(0, 0, h));
            mesh.Vertices.Add(new Vector3D(h, 0, 0));
            mesh.Vertices.Add(new Vector3D(0, h, 0));
            mesh.Faces.Add(new Face(new int[3] { 0, 1, 2 }));
            mesh.Faces.Add(new Face(new int[3] { 0, 1, 3 }));
            mesh.Faces.Add(new Face(new int[3] { 0, 2, 3 }));
            mesh.Faces.Add(new Face(new int[3] { 1, 2, 3 }));
            return mesh;
        }

        public static Mesh CreateCubeMesh(float length, string meshName)
        {
            var mesh = new Mesh(meshName, PrimitiveType.Triangle);
            mesh.Vertices.Add(new Vector3D(-length, -length, -length));
            mesh.Vertices.Add(new Vector3D(-length, -length, length));
            mesh.Vertices.Add(new Vector3D(-length, length, -length));
            mesh.Vertices.Add(new Vector3D(-length, length, length));
            mesh.Vertices.Add(new Vector3D(length, -length, -length));
            mesh.Vertices.Add(new Vector3D(length, -length, length));
            mesh.Vertices.Add(new Vector3D(length, length, -length));
            mesh.Vertices.Add(new Vector3D(length, length, length));
            mesh.Faces.Add(new Face(new int[3] { 0, 1, 2 }));
            mesh.Faces.Add(new Face(new int[3] { 1, 3, 2 }));
            mesh.Faces.Add(new Face(new int[3] { 4, 6, 5 }));
            mesh.Faces.Add(new Face(new int[3] { 5, 6, 7 }));
            mesh.Faces.Add(new Face(new int[3] { 0, 2, 4 }));
            mesh.Faces.Add(new Face(new int[3] { 4, 2, 6 }));
            mesh.Faces.Add(new Face(new int[3] { 1, 5, 3 }));
            mesh.Faces.Add(new Face(new int[3] { 5, 7, 3 }));
            mesh.Faces.Add(new Face(new int[3] { 0, 4, 1 }));
            mesh.Faces.Add(new Face(new int[3] { 4, 5, 1 }));
            mesh.Faces.Add(new Face(new int[3] { 2, 3, 6 }));
            mesh.Faces.Add(new Face(new int[3] { 6, 3, 7 }));
            return mesh;
        }

        private void AddVertices(LODRenderData meshLOD)
        {
            uint offset = 0;
            if (meshLOD.Vertexs == null)
                return;

            foreach (var vertex in meshLOD.Vertexs)
            {

                Mesh.Vertices.Add(vertex.Position.ToAssimp3D(false));

                // assign to bone here
                if (bones_ref != null){
                    switch (mesh_vert_type)
                    {
                        case VertType.rigid:
                            bones_ref[bone_index].VertexWeights.Add(new((int)offset, 1.0f));
                            break;
                        case VertType.skinned:
                        case VertType.skinned_8_weights:
                        case VertType.dq_skinned:
                            byte[] blend_indicies = vertex.Node_indices.Node_index;
                            // the last two are probably always set to 0 or null or whatever

                            float[] blend_weights = vertex.Node_weights.Node_weight;
                            
                            int bones_count = 0;
                            for (int i = 7; i >= 0; i--)
                            {
                                int? target_bone_index = blend_indicies[i];
                                if (target_bone_index == byte.MaxValue)
                                    continue;

                                if (i < 7)
                                {
                                    if (blend_indicies[i + 1] != target_bone_index)
                                        bones_count++;
                                    continue;
                                }
                                // this should probably never happen, but just in case
                                bones_count++;
                            }

                            // preprocess to fill in missing values & normalize the weights
                            float normalization_value = 0.0f;
                            for (int i = 7; i >= 0; i--)
                            {
                                int? target_bone_index = blend_indicies[i];
                                if (target_bone_index == byte.MaxValue || target_bone_index >= 255) continue;
                                if (i < 7 && blend_indicies[i + 1] == target_bone_index) break;

                                if (blend_weights[i] == float.MaxValue)
                                    blend_weights[i] = 1.0f;
                                float? weight = blend_weights[i];

                                normalization_value += (float)weight;
                            }
                            normalization_value = 1 / normalization_value;

                            // and now we iterate through the indexes & assign to bones
                            float debug_total_measured_weight = 0.0f;
                            float debug_total_adjusted_weight = 0.0f;
                            for (int i = 7; i >= 0; i--)
                            {
                                int? target_bone_index = blend_indicies[i];
                                // if not assigned or potentially invalid index, then skip (we should actually break, as its unlikely the following items would have results)
                                if (target_bone_index == byte.MaxValue || target_bone_index >= 255)
                                    continue;
                                // it turns out unused indexes just copy the previous one (which doesn't cause any issues with the previous code) opposed to being set to 255
                                // se we're just checking to see if we already added that bone weight entry already
                                // to do this correctly, we have to iterate through the array backwards, as duplicates are stored at the front
                                if (i < 7 && blend_indicies[i + 1] == target_bone_index)
                                    break; // anything after this is probably also a duplicate

                                float? weight = blend_weights[i] * normalization_value;

                                debug_total_measured_weight += (float)weight;
                                if (bones_count == 1) // if only one parent bone or not using dual quat, then this bone has full ownership
                                    weight = 1.0f; // i think this is what that means

                                debug_total_adjusted_weight += (float)weight;
                                bones_ref[(int)target_bone_index].VertexWeights.Add(new((int)offset, (float)weight));
                            }
                            break;
                        case VertType.rigid_boned:
                            HashSet<byte> uni_bones = vertex.Node_indices.Node_index.ToHashSet();

                            foreach (var bone_i in uni_bones)
                            {
                                if (bone_i < bones_ref.Length) {
                                    if (bone_i == byte.MaxValue)
                                        continue;
                                    bones_ref[bone_i].VertexWeights.Add(new((int)offset, 1.0f));
                                }
                                    
                            }

                            break;
                        

                        
                        default:
                            break;
                    }

                    if (false && bone_index == 255){ // then this is a weighted vert that may have 1 or more parent bones

                        byte[] blend_indicies = vertex.Node_indices.Node_index;
                        // the last two are probably always set to 0 or null or whatever

                        float[] blend_weights = vertex.Node_weights.Node_weight;
                        
                            
                            if (mesh_vert_type == VertType.dq_skinned){
                            
                            //    blend_weights[3] = null; // ignore whatever that extra value is
                            //    blend_weights[4] = null;
                            //    blend_weights[5] = null;
                            
                            }
                        

                        // to remove duplicates, we iterate through the list backwards, as for some reason duplicates are at the start

                        // count unique number of bone indexes
                        int bones_count = 0;
                        for (int i = 7; i >= 0; i--){
                            int? target_bone_index = blend_indicies[i];
                            if (target_bone_index == null)
                                continue;

                            if (i < 7){
                                if (blend_indicies[i+1] != target_bone_index)
                                    bones_count++;
                                continue;
                            }
                            // this should probably never happen, but just in case
                            bones_count++;
                        }

                        // preprocess to fill in missing values & normalize the weights
                        float normalization_value = 0.0f;
                        for (int i = 7; i >= 0; i--){
                            int? target_bone_index = blend_indicies[i];
                            if (target_bone_index == null || target_bone_index >= 255) continue;
                            if (i < 7 && blend_indicies[i + 1] == target_bone_index) break;

                            if (blend_weights[i] == null)
                                blend_weights[i] = 1.0f;
                            float? weight = blend_weights[i];

                            normalization_value += (float)weight;
                        }
                        normalization_value = 1 / normalization_value;

                        // and now we iterate through the indexes & assign to bones
                        float debug_total_measured_weight = 0.0f;
                        float debug_total_adjusted_weight = 0.0f;
                        for (int i = 7; i >= 0; i--){
                            int? target_bone_index = blend_indicies[i];
                            // if not assigned or potentially invalid index, then skip (we should actually break, as its unlikely the following items would have results)
                            if (target_bone_index == null || target_bone_index >= 255)
                                continue;
                            // it turns out unused indexes just copy the previous one (which doesn't cause any issues with the previous code) opposed to being set to 255
                            // se we're just checking to see if we already added that bone weight entry already
                            // to do this correctly, we have to iterate through the array backwards, as duplicates are stored at the front
                            if (i < 7 && blend_indicies[i+1] == target_bone_index)
                                break; // anything after this is probably also a duplicate

                            float? weight = blend_weights[i] * normalization_value;

                            debug_total_measured_weight += (float)weight;
                            if (bones_count == 1) // if only one parent bone or not using dual quat, then this bone has full ownership
                                weight = 1.0f; // i think this is what that means

                            debug_total_adjusted_weight += (float)weight;
                            bones_ref[(int)target_bone_index].VertexWeights.Add(new((int)offset, (float)weight));
                        }

                        // ///////////////////// //
                        // error checking stuff //
                        // /////////////////// //

                        // VertType.skinned: potentially uses 2 extra blend weights that would be set to 1 by default, and then normalized with the other values (likely in groups of 3)
                        // VertType.dq_skinned: normal? apparently works the same as skinned but with an extra variable that we dont use
                        // VertType.rigid_boned: no blend weights, but has blend indices
                        if (mesh_vert_type != VertType.skinned && mesh_vert_type != VertType.dq_skinned && mesh_vert_type != VertType.rigid_boned)
                        {

                        }
                        // total weight should be 1.0f, but i feel like its possible that it doesn't always have to be?
                        if (debug_total_adjusted_weight != 1.0f)
                        {
                             
                        }

                        // debugging breakpoints
                        if (mesh_vert_type == VertType.skinned)
                        {

                        }
                        if (mesh_vert_type == VertType.dq_skinned)
                        {

                        }



                    }
                    // else its a rigid mesh and we assign the singlular bone 

                    //else bones_ref[bone_index].VertexWeights.Add(new((int)offset, 1.0f));

                }

                VertexLookup.Add(offset++, (uint)VertexLookup.Count);
            }
        }
        private S3DFace[] DeserializeFaces(List<uint> vert_index)
        {
            S3DFace[] salida = new S3DFace[vert_index.Count / 3];
            int rest = vert_index.Count % 3;
            int nFace = 0;
            for (int i = 0; i < vert_index.Count-rest; i += 3)
            {

                salida[nFace++] = S3DFace.Create(new uint[3] { vert_index[i], vert_index[i + 1], vert_index[i + 2] });
            }
            return salida;
        }
        private void AddFaces(LODRenderData meshLOD)
        {
            if (meshLOD.IndexBufferIndex.Count == 0)
            {
                for (int i = 0; i < meshLOD.Vertexs.Length; i++)
                {
                    meshLOD.IndexBufferIndex.Add((uint)i);
                }
            }
            S3DFace[] faces = DeserializeFaces(meshLOD.IndexBufferIndex);
            foreach (var face in faces)
            {
                var assimpFace = new Face();
                assimpFace.Indices.Add((int)VertexLookup[face[0]]);
                assimpFace.Indices.Add((int)VertexLookup[face[1]]);
                assimpFace.Indices.Add((int)VertexLookup[face[2]]);

                Mesh.Faces.Add(assimpFace);
            }
        }
        private void AddInterleavedData(LODRenderData meshLOD)
        {
            if (meshLOD.Vertexs == null)
                return;

            foreach (var vertex in meshLOD.Vertexs)
            {
                Vector4 uvVector = default;
                uvVector.X = vertex.Texcoord.Value.X;
                uvVector.Y = vertex.Texcoord.Value.Y;
                AddVertexUV(0, uvVector);
                if (vertex.Texcoord1 != null)
                {
                    Vector4 uvVector1 = default;
                    uvVector.X = vertex.Texcoord1.Value.X;
                    uvVector.Y = vertex.Texcoord1.Value.Y;
                    AddVertexUV(1, uvVector1);
                }
                if (vertex.Texcoord2 != null)
                {
                    Vector4 uvVector2 = default;
                    uvVector.X = vertex.Texcoord2.Value.X;
                    uvVector.Y = vertex.Texcoord2.Value.Y;
                    AddVertexUV(2, uvVector2);
                }

            }
        }

        private void AddVertexUV(byte uvChannel, Vector4 uvVector)
        {
            /*if (!_submesh.UvScaling.TryGetValue(uvChannel, out var scaleFactor))
                scaleFactor = 1;*/

            float scaleFactor = 1;
            var scaleVector = new Vector3D(scaleFactor);
            var scaledUvVector = uvVector.ToAssimp3D(false) * scaleVector;

            Mesh.TextureCoordinateChannels[uvChannel].Add(scaledUvVector);

            /* This is a bit confusing, but this property denotes the size of the UV element.
             * E.g. setting it to 2 means there is a U and a V.
             * I don't know how 4D UVs work, but if we ever add support for them, we'd need to
             * adjust this accordingly.
             */
            Mesh.UVComponentCount[uvChannel] = 2;
        }

    }
}
