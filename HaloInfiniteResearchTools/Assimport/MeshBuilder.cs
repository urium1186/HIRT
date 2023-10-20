using Assimp;
using HaloInfiniteResearchTools.Common.Extensions;
using LibHIRT.Data;
using LibHIRT.Data.Geometry;
using LibHIRT.Domain;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Numerics;

namespace HaloInfiniteResearchTools.Assimport
{
    public class MeshBuilder
    {
        private readonly ISceneContext _context;
        public Mesh Mesh { get; }

        private Bone[]? bones_ref;
        private uint bone_index;
        private bool use_dual_quat; // no idea what this means
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
                    if (bone_index == 255){ // then this is a weighted vert that may have 1 or more parent bones

                        int?[] blend_indicies = new int?[8];
                        // the last two are probably always set to 0 or null or whatever

                        float?[] blend_weights = new float?[6];

                        if (vertex.BlendIndices0 != null){
                            blend_indicies[0] = (int)vertex.BlendIndices0.Value.X;
                            blend_indicies[1] = (int)vertex.BlendIndices0.Value.Y;
                            blend_indicies[2] = (int)vertex.BlendIndices0.Value.Z;
                            //blend_indicies[3] = (int)vertex.BlendIndices0.Value.W; // turned off for now, so we only match up the first 3 weights until blend group [1] is implemented 
                        }
                        if (vertex.BlendWeights0 != null){
                            blend_weights[0] = vertex.BlendWeights0.Value.X;
                            blend_weights[1] = vertex.BlendWeights0.Value.Y;
                            blend_weights[2] = vertex.BlendWeights0.Value.Z;
                        }
                    
                        // NOTE: BLEND INDICIES/WEIGHTS [1] ARE NOT SETUP YET!!! URIUM HAS TO SET THIS UP
                        if (vertex.BlendIndices1 != null){
                            blend_indicies[4] = (int)vertex.BlendIndices1.Value.X;
                            blend_indicies[5] = (int)vertex.BlendIndices1.Value.Y;
                            blend_indicies[6] = (int)vertex.BlendIndices1.Value.Z;
                            blend_indicies[7] = (int)vertex.BlendIndices1.Value.W;
                        }
                        if (vertex.BlendWeights1 != null){
                            blend_weights[3] = vertex.BlendWeights1.Value.X;
                            blend_weights[4] = vertex.BlendWeights1.Value.Y;
                            blend_weights[5] = vertex.BlendWeights1.Value.Z;
                        }
                    
                        // and now we iterate through the indexes & assign to bones
                        for (int i = 0; i < 8; i++){
                            int? target_bone_index = blend_indicies[i];
                            // if not assigned or potentially invalid index, then skip (we should actually break, as its unlikely the following items would have results)
                            if (target_bone_index == null || target_bone_index >= 255 || blend_weights[i] == null)
                                continue;
                            float? weight = blend_weights[i];
                            if (!use_dual_quat) weight = 1.0f; // i think this is what that means

                            bones_ref[(int)target_bone_index].VertexWeights.Add(new((int)offset, (float)weight));
                        }



                    }
                    // else its a rigid mesh and we assign the singlular bone 

                    else bones_ref[bone_index].VertexWeights.Add(new((int)offset, 1.0f));

                }

                VertexLookup.Add(offset++, (uint)VertexLookup.Count);
            }
        }
        private S3DFace[] DeserializeFaces(List<uint> vert_index)
        {
            S3DFace[] salida = new S3DFace[vert_index.Count / 3];
            int nFace = 0;
            for (int i = 0; i < vert_index.Count; i += 3)
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
                uvVector.X = vertex.UV0.Value.X;
                uvVector.Y = vertex.UV0.Value.Y;
                AddVertexUV(0, uvVector);
                if (vertex.UV1 != null)
                {
                    Vector4 uvVector1 = default;
                    uvVector.X = vertex.UV1.Value.X;
                    uvVector.Y = vertex.UV1.Value.Y;
                    AddVertexUV(1, uvVector1);
                }
                if (vertex.UV2 != null)
                {
                    Vector4 uvVector2 = default;
                    uvVector.X = vertex.UV2.Value.X;
                    uvVector.Y = vertex.UV2.Value.Y;
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
