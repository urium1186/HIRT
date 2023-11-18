using Assimp;
using HaloInfiniteResearchTools.Common.Extensions;
using LibHIRT.Data.Geometry;
using LibHIRT.Domain;
using System.Collections.Generic;
using System.Numerics;

namespace HaloInfiniteResearchTools.Assimport
{
    public static class SMeshBuilder
    {

        public static Mesh Build(s_mesh _object, short lodIndex, string meshName, List<int> materialsIndexList)
        {
            var mesh = new Mesh(meshName, PrimitiveType.Triangle);
            var verLU = AddVertices(mesh, _object.LODRenderData[lodIndex]);
            AddFaces(mesh, _object.LODRenderData[lodIndex], verLU);
            AddInterleavedData(mesh, _object.LODRenderData[lodIndex]);
            if (materialsIndexList != null && materialsIndexList.Count > _object.LODRenderData[lodIndex].Parts[0].MaterialIndex)
                mesh.MaterialIndex = materialsIndexList[_object.LODRenderData[lodIndex].Parts[0].MaterialIndex];
            return mesh;
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

        private static Dictionary<uint, uint> AddVertices(Mesh onMesh, LODRenderData meshLOD)
        {
            Dictionary<uint, uint> vertexLookup = new Dictionary<uint, uint>();
            uint offset = 0;
            if (meshLOD.Vertexs == null)
                return vertexLookup;

            foreach (var vertex in meshLOD.Vertexs)
            {
                onMesh.Vertices.Add(vertex.Position.ToAssimp3D(false));

                vertexLookup.Add(offset++, (uint)vertexLookup.Count);
            }
            return vertexLookup;
        }
        private static S3DFace[] DeserializeFaces(List<uint> vert_index)
        {
            S3DFace[] salida = new S3DFace[vert_index.Count / 3];
            int nFace = 0;
            for (int i = 0; i < vert_index.Count; i += 3)
            {

                salida[nFace++] = S3DFace.Create(new uint[3] { vert_index[i], vert_index[i + 1], vert_index[i + 2] });
            }
            return salida;
        }
        private static void AddFaces(Mesh onMesh, LODRenderData meshLOD, Dictionary<uint, uint> vertexLookup)
        {
            if (meshLOD.IndexBufferIndex.Count == 0 && meshLOD.Vertexs != null)
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
                assimpFace.Indices.Add((int)vertexLookup[face[0]]);
                assimpFace.Indices.Add((int)vertexLookup[face[1]]);
                assimpFace.Indices.Add((int)vertexLookup[face[2]]);

                onMesh.Faces.Add(assimpFace);
            }
        }
        private static void AddInterleavedData(Mesh onMesh, LODRenderData meshLOD)
        {
            if (meshLOD.Vertexs == null)
                return;

            foreach (var vertex in meshLOD.Vertexs)
            {
                Vector4 uvVector = default;
                uvVector.X = vertex.Texcoord.Value.X;
                uvVector.Y = vertex.Texcoord.Value.Y;
                AddVertexUV(onMesh, 0, uvVector);
                if (vertex.Texcoord1 != null)
                {
                    Vector4 uvVector1 = default;
                    uvVector.X = vertex.Texcoord1.Value.X;
                    uvVector.Y = vertex.Texcoord1.Value.Y;
                    AddVertexUV(onMesh, 1, uvVector1);
                }
                if (vertex.Texcoord2 != null)
                {
                    Vector4 uvVector2 = default;
                    uvVector.X = vertex.Texcoord2.Value.X;
                    uvVector.Y = vertex.Texcoord2.Value.Y;
                    AddVertexUV(onMesh, 2, uvVector2);
                }

            }
        }

        private static void AddVertexUV(Mesh onMesh, byte uvChannel, Vector4 uvVector)
        {
            /*if (!_submesh.UvScaling.TryGetValue(uvChannel, out var scaleFactor))
                scaleFactor = 1;*/

            float scaleFactor = 1;
            var scaleVector = new Vector3D(scaleFactor);
            var scaledUvVector = uvVector.ToAssimp3D(false) * scaleVector;

            onMesh.TextureCoordinateChannels[uvChannel].Add(scaledUvVector);

            /* This is a bit confusing, but this property denotes the size of the UV element.
             * E.g. setting it to 2 means there is a U and a V.
             * I don't know how 4D UVs work, but if we ever add support for them, we'd need to
             * adjust this accordingly.
             */
            onMesh.UVComponentCount[uvChannel] = 2;
        }

    }
}
