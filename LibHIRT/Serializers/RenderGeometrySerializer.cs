using LibHIRT.Domain;
using LibHIRT.Domain.DX;
using LibHIRT.Domain.Geometry;
using LibHIRT.Domain.RenderModel;
using LibHIRT.Files;
using LibHIRT.Files.FileTypes;
using LibHIRT.TagReader;
using LibHIRT.Utils;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace LibHIRT.Serializers
{
    public class RenderGeometrySerializer : SerializerBase<RenderGeometry>
    {
        static string _filePath = "";
        private static RenderGeometryTag _renderGeometry;
        static SSpaceFile _file;

        public static bool GetBlendInfo { get; private set; }

        public static RenderGeometry Deserialize(Stream stream, SSpaceFile file, RenderGeometryTag renderGeometry)
        {
            _file = file;
            _filePath = file.Path_string;
            GetBlendInfo = true;
            _renderGeometry = renderGeometry;
            var reader = new BinaryReader(new MemoryStream());
            return new RenderGeometrySerializer().Deserialize(reader);
        }

        protected override void OnDeserialize(BinaryReader reader, RenderGeometry obj)
        {
            ReadRenderGeometry(obj);
        }

        void ReadRenderGeometry(RenderGeometry obj)
        {
            if (_renderGeometry == null || obj == null)
                return;
            obj.CompressionInfo = createCompressionInfo();

            ListTagInstance temp = _renderGeometry["meshes"] as ListTagInstance;
            if (temp == null)
                return;
            TagInstance mesh_package = _renderGeometry["mesh package"];
            var tempMP = GetRenderGeometryMeshPackage(mesh_package);
            ReadBufferInAllChuncks(ref tempMP, _file.Parent == null);
            int pad = temp.Childs.Count.ToString().Length;
            string file_name = Path.GetFileNameWithoutExtension(_filePath);
            for (int i = 0; i < temp.Childs.Count; i++)
            {
                ObjMesh t_m = processMeshInst(temp.Childs[i], obj, tempMP, i);
                t_m.Name = file_name + "_mesh_" + i.ToString().PadLeft(pad, '0');
                obj.Meshes.Add(t_m);
            }
        }

        private ObjMesh processMeshInst(TagInstance mesh, RenderGeometry obj, RenderGeometryMeshPackage mesh_package, int m_i = -1)
        {

            ObjMesh obj_mesh = new ObjMesh();

            obj_mesh.CloneIndex = (Int16)mesh["clone index"].AccessValue;
            obj_mesh.RigidNodeIndex = (sbyte)mesh["rigid node index"].AccessValue;
            obj_mesh.VertType = (VertType)((EnumGroup)mesh["vertex type"]).SelectedIndex;
            
            obj_mesh.IndexBufferType = (IndexBufferType)((EnumGroup)mesh["index buffer type"]).SelectedIndex;
            obj_mesh.UseDualQuat = (sbyte)mesh["use dual quat"].AccessValue != 0;
            ListTagInstance lod_render_data = mesh["LOD render data"] as ListTagInstance;
            HashSet<short[]> indices = new HashSet<short[]>();
            if (mesh_package.MeshResourceGroups.Length > 0 && mesh_package.MeshResourceGroups[0].MeshResource[0].StreamingMeshes.Length > 0) {
                var str_mesh = mesh_package.MeshResourceGroups[0].MeshResource[0].StreamingMeshes[m_i];
                var max = -1;
                var min = int.MaxValue;
                short count_nz = 0;
                
                for (int w = 0; w < str_mesh.MeshLodChunks.Length; w++)
                {

                    var l_chun = str_mesh.MeshLodChunks[w];
                    if (l_chun.Chunks.Length == 0)
                        continue;
                    count_nz++;
                    var b = InterpretChunkParameterInfo(l_chun.Chunks);
                    indices.Add(b);
                }
            }
            

            if (lod_render_data != null && lod_render_data.Count > 0)
            {
                //Debug.Assert(indices.Contains(0) || indices.Count == 1);

                //for (int i = 0; i < lod_render_data.Count; i++)
                for (int i = 0; i < 1; i++)
                {
                    var lod = lod_render_data[i];
                    // if not (self.minLOD <= lod_i <= self.maxLOD):
                    var obj_lod = new MeshLOD(obj_mesh);
                    obj_lod.IndexBufferIndex.Clear();
                    short index_i = (short)lod["index buffer index"].AccessValue;
                    if (mesh_package.MeshResourceGroups.Length > 0 && mesh_package.MeshResourceGroups[0].MeshResource[0].PcIndexBuffers.Length > 0) {
                        var index_buff = mesh_package.MeshResourceGroups[0].MeshResource[0].PcIndexBuffers[index_i];
                        if (index_buff.d3dbuffer.D3dBuffer == null)
                        {
                            index_buff.d3dbuffer.D3dBuffer = ReadBufferInChuncks(indices, mesh_package.MeshResourceGroups[0].MeshResource[0], index_buff.offset, index_buff.d3dbuffer.ByteWidth);
                            if (index_buff.d3dbuffer.D3dBuffer == null)
                                continue;
                        }
                        if (index_buff.Stride == 4)
                        {

                        }
                        for (int o = 0; o < index_buff.count; o++)
                        {
                            uint index = uint.MaxValue;
                            if (index_buff.Stride == 2)
                                index = BitConverter.ToUInt16(index_buff.d3dbuffer.D3dBuffer, o * 2);
                            else if (index_buff.Stride == 4)
                            {
                                index = BitConverter.ToUInt32(index_buff.d3dbuffer.D3dBuffer, o * 4);
                            }
                            else
                                Debug.Assert(DebugConfig.NoCheckFails);
                            Debug.Assert(index != uint.MaxValue);
                            obj_lod.IndexBufferIndex.Add(index);
                        }



                    }
                    //Debug.Assert(index_buff.offset >= min && index_buff.offset + index_buff.d3dbuffer.ByteWidth <= max);
                    ListTagInstance vert_buffer_indices = lod["vertex buffer indices"] as ListTagInstance;
                    Dictionary<PcVertexBuffersUsage, RasterizerVertexBuffer> vert_buffers = new Dictionary<PcVertexBuffersUsage, RasterizerVertexBuffer>();
                    if (vert_buffer_indices != null)
                    {

                        for (int j = 0; j < vert_buffer_indices.Count; j++)
                        {
                            int temp_vert_index_block = (Int16)vert_buffer_indices[j]["vertex buffer index"].AccessValue;
                            if (temp_vert_index_block != -1)
                            {
                                var vertexBuffer = mesh_package.MeshResourceGroups[0].MeshResource[0].PcVertexBuffers[temp_vert_index_block];
                                vert_buffers[vertexBuffer.usage] = vertexBuffer;
                            }
                        }

                    }
                    if (vert_buffers.ContainsKey(PcVertexBuffersUsage.Position))
                    {
                        switch (obj_mesh.VertType)
                        {
                            case VertType.world:
                                break;
                            case VertType.rigid:
                                Debug.Assert(!vert_buffers.ContainsKey(PcVertexBuffersUsage.BlendIndices0));
                                Debug.Assert(!vert_buffers.ContainsKey(PcVertexBuffersUsage.BlendIndices1));
                                Debug.Assert(!vert_buffers.ContainsKey(PcVertexBuffersUsage.BlendWeights0));
                                Debug.Assert(!vert_buffers.ContainsKey(PcVertexBuffersUsage.BlendWeights1));
                                break;
                            case VertType.skinned:
                                Debug.Assert(vert_buffers[PcVertexBuffersUsage.BlendWeights0].format == PcVertexBuffersFormat.f_10_10_10_normalized);
                                Debug.Assert(vert_buffers.ContainsKey(PcVertexBuffersUsage.BlendIndices0));
                                Debug.Assert(!vert_buffers.ContainsKey(PcVertexBuffersUsage.BlendIndices1));
                                Debug.Assert(vert_buffers.ContainsKey(PcVertexBuffersUsage.BlendWeights0));
                                Debug.Assert(!vert_buffers.ContainsKey(PcVertexBuffersUsage.BlendWeights1));
                                break;
                            case VertType.particle_model:
                                break;
                            case VertType.screen:
                                break;
                            case VertType.debug:
                                break;
                            case VertType.transparent:
                                break;
                            case VertType.particle:
                                break;
                            case VertType.removed08:
                                break;
                            case VertType.removed09:
                                break;
                            case VertType.chud_simple:
                                break;
                            case VertType.decorator:
                                break;
                            case VertType.position_only:
                                break;
                            case VertType.removed13:
                                break;
                            case VertType.ripple:
                                break;
                            case VertType.removed15:
                                break;
                            case VertType.tessellatedTerrain:
                                break;
                            case VertType.empty:
                                break;
                            case VertType.decal:
                                break;
                            case VertType.removed19:
                                break;
                            case VertType.removed20:
                                break;
                            case VertType.position_only_21:
                                break;
                            case VertType.tracer:
                                break;
                            case VertType.rigid_boned:
                                Debug.Assert(vert_buffers.ContainsKey(PcVertexBuffersUsage.BlendIndices0));
                                Debug.Assert(!vert_buffers.ContainsKey(PcVertexBuffersUsage.BlendIndices1));
                                Debug.Assert(!vert_buffers.ContainsKey(PcVertexBuffersUsage.BlendWeights0));
                                Debug.Assert(!vert_buffers.ContainsKey(PcVertexBuffersUsage.BlendWeights1));
                                break;
                            case VertType.removed24:
                                break;
                            case VertType.CheapParticle:
                                break;
                            case VertType.dq_skinned:
                                Debug.Assert(vert_buffers.ContainsKey(PcVertexBuffersUsage.BlendIndices0));
                                Debug.Assert(!vert_buffers.ContainsKey(PcVertexBuffersUsage.BlendIndices1));
                                Debug.Assert(vert_buffers.ContainsKey(PcVertexBuffersUsage.BlendWeights0));
                                Debug.Assert(vert_buffers[PcVertexBuffersUsage.BlendWeights0].format == PcVertexBuffersFormat.f_10_10_10_normalized);
                                Debug.Assert(vert_buffers.ContainsKey(PcVertexBuffersUsage.BlendWeights1));
                                Debug.Assert(vert_buffers[PcVertexBuffersUsage.BlendWeights1].format == PcVertexBuffersFormat.real);
                                break;
                            case VertType.skinned_8_weights:
                                Debug.Assert(vert_buffers.ContainsKey(PcVertexBuffersUsage.BlendIndices0));
                                Debug.Assert(vert_buffers.ContainsKey(PcVertexBuffersUsage.BlendIndices1));
                                Debug.Assert(vert_buffers.ContainsKey(PcVertexBuffersUsage.BlendWeights0));
                                Debug.Assert(vert_buffers.ContainsKey(PcVertexBuffersUsage.BlendWeights1));
                                break;
                            case VertType.tessellated_vector:
                                break;
                            case VertType.interaction:
                                break;
                            case VertType.number_of_standard_vertex_types:
                                break;
                            default:
                                break;
                        }
                       

                        RasterizerVertexBuffer tempPosition = vert_buffers[PcVertexBuffersUsage.Position];
                        if (tempPosition.d3dbuffer.D3dBuffer == null)
                            tempPosition.d3dbuffer.D3dBuffer = ReadBufferInChuncks(indices, mesh_package.MeshResourceGroups[0].MeshResource[0], tempPosition.offset, tempPosition.d3dbuffer.ByteWidth);
                        MemoryStream msPosition = new MemoryStream(tempPosition.d3dbuffer.D3dBuffer);
                        RasterizerVertexBuffer tempUV0 = null;
                        RasterizerVertexBuffer tempUV1 = null;
                        RasterizerVertexBuffer tempUV2 = null;
                        RasterizerVertexBuffer blendIndices0 = null;
                        RasterizerVertexBuffer blendIndices1 = null;
                        RasterizerVertexBuffer blendWeights0 = null;
                        RasterizerVertexBuffer blendWeights1 = null;
                        MemoryStream msUV0 = null;
                        MemoryStream msUV1 = null;
                        MemoryStream msUV2 = null;
                        MemoryStream msBlendIndices0 = null;
                        MemoryStream msBlendIndices1 = null;
                        MemoryStream msBlendWeights0 = null;
                        MemoryStream msBlendWeights1 = null;
                        if (vert_buffers.ContainsKey(PcVertexBuffersUsage.UV0))
                        {
                            tempUV0 = vert_buffers[PcVertexBuffersUsage.UV0];
                            if (tempUV0.d3dbuffer.D3dBuffer == null)
                                tempUV0.d3dbuffer.D3dBuffer = ReadBufferInChuncks(indices, mesh_package.MeshResourceGroups[0].MeshResource[0], tempUV0.offset, tempUV0.d3dbuffer.ByteWidth);
                            msUV0 = new MemoryStream(tempUV0.d3dbuffer.D3dBuffer);
                            Debug.Assert(tempUV0.count == tempPosition.count);
                        }
                        if (vert_buffers.ContainsKey(PcVertexBuffersUsage.UV1))
                        {
                            tempUV1 = vert_buffers[PcVertexBuffersUsage.UV1];
                            if (tempUV1.d3dbuffer.D3dBuffer == null)
                                tempUV1.d3dbuffer.D3dBuffer = ReadBufferInChuncks(indices, mesh_package.MeshResourceGroups[0].MeshResource[0], tempUV1.offset, tempUV1.d3dbuffer.ByteWidth);
                            msUV1 = new MemoryStream(tempUV1.d3dbuffer.D3dBuffer);
                            Debug.Assert(tempUV1.count == tempPosition.count);
                        }
                        if (vert_buffers.ContainsKey(PcVertexBuffersUsage.UV2))
                        {
                            tempUV2 = vert_buffers[PcVertexBuffersUsage.UV2];
                            if (tempUV2.d3dbuffer.D3dBuffer == null)
                                tempUV2.d3dbuffer.D3dBuffer = ReadBufferInChuncks(indices, mesh_package.MeshResourceGroups[0].MeshResource[0], tempUV2.offset, tempUV2.d3dbuffer.ByteWidth);
                            msUV2 = new MemoryStream(tempUV2.d3dbuffer.D3dBuffer);
                            Debug.Assert(tempUV2.count == tempPosition.count);
                        }
                        if (vert_buffers.ContainsKey(PcVertexBuffersUsage.BlendIndices0))
                        {
                            blendIndices0 = vert_buffers[PcVertexBuffersUsage.BlendIndices0];
                            if (blendIndices0.d3dbuffer.D3dBuffer == null)
                                blendIndices0.d3dbuffer.D3dBuffer = ReadBufferInChuncks(indices, mesh_package.MeshResourceGroups[0].MeshResource[0], blendIndices0.offset, blendIndices0.d3dbuffer.ByteWidth);
                            msBlendIndices0 = new MemoryStream(blendIndices0.d3dbuffer.D3dBuffer);
                            //Debug.Assert(blendWeights0.count == tempPosition.count);
                        }
                        if (vert_buffers.ContainsKey(PcVertexBuffersUsage.BlendWeights0))
                        {
                            blendWeights0 = vert_buffers[PcVertexBuffersUsage.BlendWeights0];
                            if (blendWeights0.d3dbuffer.D3dBuffer == null)
                                blendWeights0.d3dbuffer.D3dBuffer = ReadBufferInChuncks(indices, mesh_package.MeshResourceGroups[0].MeshResource[0], blendWeights0.offset, blendWeights0.d3dbuffer.ByteWidth);
                            msBlendWeights0 = new MemoryStream(blendWeights0.d3dbuffer.D3dBuffer);
                            //Debug.Assert(blendWeights0.count == tempPosition.count);
                        }
                        if (vert_buffers.ContainsKey(PcVertexBuffersUsage.BlendIndices1))
                        {
                            blendIndices1 = vert_buffers[PcVertexBuffersUsage.BlendIndices1];
                            if (blendIndices1.d3dbuffer.D3dBuffer == null)
                                blendIndices1.d3dbuffer.D3dBuffer = ReadBufferInChuncks(indices, mesh_package.MeshResourceGroups[0].MeshResource[0], blendIndices1.offset, blendIndices1.d3dbuffer.ByteWidth);
                            msBlendIndices0 = new MemoryStream(blendIndices1.d3dbuffer.D3dBuffer);
                            //Debug.Assert(blendWeights0.count == tempPosition.count);
                        }
                        if (vert_buffers.ContainsKey(PcVertexBuffersUsage.BlendWeights1))
                        {
                            blendWeights1 = vert_buffers[PcVertexBuffersUsage.BlendWeights1];
                            if (blendWeights1.d3dbuffer.D3dBuffer == null)
                                blendWeights1.d3dbuffer.D3dBuffer = ReadBufferInChuncks(indices, mesh_package.MeshResourceGroups[0].MeshResource[0], blendWeights1.offset, blendWeights1.d3dbuffer.ByteWidth);
                            msBlendWeights1 = new MemoryStream(blendWeights1.d3dbuffer.D3dBuffer);
                            //Debug.Assert(blendWeights0.count == tempPosition.count);
                        }
                        obj_lod.Vertexs = new SSPVertex[tempPosition.count];
                        HashSet<string> vert = new HashSet<string>();
                        int countVOOB = 0;
                        for (int j = 0; j < tempPosition.count; j++)
                        {
                            SSPVertex temp = new SSPVertexStatic();
                            byte[] buffer = new byte[tempPosition.stride];
                            msPosition.Read(buffer, 0, tempPosition.stride);
                            var vals = FormatReader.ReadWordVector4DNormalized(buffer);
                            temp.Position = new System.Numerics.Vector4(
                                vals.Item1 * obj.CompressionInfo.ModelScale.M13 + obj.CompressionInfo.ModelScale.M11,
                                vals.Item2 * obj.CompressionInfo.ModelScale.M23 + obj.CompressionInfo.ModelScale.M21,
                                vals.Item3 * obj.CompressionInfo.ModelScale.M33 + obj.CompressionInfo.ModelScale.M31,
                                vals.Item4);
                            if (tempUV0 != null)
                            {
                                buffer = new byte[tempUV0.stride];
                                msUV0.Read(buffer, 0, tempUV0.stride);
                                var valsUV = FormatReader.ReadWordVector2DNormalized(buffer);
                                var uv0_scale = obj.CompressionInfo.Uv0Scale;
                                temp.UV0 = new System.Numerics.Vector2(
                                    valsUV.Item1 * uv0_scale.M13 + uv0_scale.M11,
                                    valsUV.Item2 * uv0_scale.M23 + uv0_scale.M21
                                    );
                            }
                            if (tempUV1 != null)
                            {
                                buffer = new byte[tempUV1.stride];
                                msUV1.Read(buffer, 0, tempUV1.stride);
                                var valsUV = FormatReader.ReadWordVector2DNormalized(buffer);
                                var uv1_scale = obj.CompressionInfo.Uv1Scale;
                                temp.UV1 = new System.Numerics.Vector2(
                                    valsUV.Item1 * uv1_scale.M13 + uv1_scale.M11,
                                    valsUV.Item2 * uv1_scale.M23 + uv1_scale.M21
                                    );
                            }
                            if (tempUV2 != null)
                            {
                                buffer = new byte[tempUV2.stride];
                                msUV2.Read(buffer, 0, tempUV2.stride);
                                var valsUV = FormatReader.ReadWordVector2DNormalized(buffer);
                                var uv2_scale = obj.CompressionInfo.Uv2Scale;
                                temp.UV2 = new System.Numerics.Vector2(
                                    valsUV.Item1 * uv2_scale.M13 + uv2_scale.M11,
                                    valsUV.Item2 * uv2_scale.M23 + uv2_scale.M21
                                    );
                            }
                            if (GetBlendInfo) {
                                if (blendIndices0 != null && blendIndices0.count == tempPosition.count)
                                {
                                    buffer = new byte[blendIndices0.stride];
                                    msBlendIndices0.Read(buffer, 0, blendIndices0.stride);
                                    (int, int, int, int) resultBI = ((byte, byte, byte, byte))FormatReader.Read(blendIndices0.format, buffer);
                                    temp.BlendIndices0 = new System.Numerics.Vector4(resultBI.Item1, resultBI.Item2, resultBI.Item3, resultBI.Item4);
                                }
                                if (blendWeights0 != null && blendWeights0.count == tempPosition.count)
                                {
                                    buffer = new byte[blendWeights0.stride];
                                    msBlendWeights0.Read(buffer, 0, blendWeights0.stride);
                                    Vector3 re = Vector3.One;
                                    if (blendWeights0.format == PcVertexBuffersFormat.f_10_10_10_normalized) {
                                        
                                        
                                        re = (Vector3)FormatReader.Read(blendWeights0.format, buffer);
                                        temp.BlendWeights0 = new System.Numerics.Vector4(re.X, re.Y, re.Z, 1);

                                        if (obj_mesh.VertType == VertType.dq_skinned )
                                        {
                                            Debug.Assert(blendWeights0.d3dbuffer.Usage == 8);
                                        }else  if ( obj_mesh.VertType == VertType.skinned)
                                        {
                                            Debug.Assert(blendWeights0.d3dbuffer.Usage == 8);
                                        }

                                        if (temp.BlendIndices0.Value.X == temp.BlendIndices0.Value.Y && temp.BlendIndices0.Value.X == temp.BlendIndices0.Value.Z && temp.BlendIndices0.Value.X == temp.BlendIndices0.Value.W)
                                        {
                                            countVOOB++;
                                            string vert_S = temp.Position.X.ToString() + temp.Position.Y.ToString() + temp.Position.Z.ToString();
                                            if (re.X + re.Y + re.Z != 0) {
                                                if (!vert.Contains(vert_S)) {
                                                    vert.Add(vert_S);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            
                            obj_lod.Vertexs[j] = temp;
                        }
                        if (obj_mesh.VertType == VertType.dq_skinned || obj_mesh.VertType == VertType.skinned)
                        {
                            // Debug.Assert(vert.Count==0);
                        }
                    }
                    /*
                    var arr = ReadBufferInChuncks(indices, mesh_package.MeshResourceGroups[0].MeshResource[0].StreamingChunks, vertexBuffer.offset, vertexBuffer.d3dbuffer.ByteWidth);
                    MemoryStream temp = new MemoryStream(arr);
                    //Debug.Assert(array.offset >= min && array.offset + array.d3dbuffer.ByteWidth<= max);
                    if (vertexBuffer.format == PcVertexBuffersFormat.f_10_10_10_normalized)
                    {

                    }
                    for (int l = 0; l < vertexBuffer.count; l++)
                    {
                        byte[] buffer = new byte[vertexBuffer.stride];
                        temp.Read(buffer, 0, vertexBuffer.stride);
                        var vals = FormatReader.ReadF_10_10_10_normalized(buffer);
                    }*/
                    obj_mesh.LODRenderData.Add(obj_lod);
                }

            }

            return obj_mesh;


        }
        void ReadBufferInAllChuncks(ref RenderGeometryMeshPackage mesh_package, bool inDisk = false)
        {
            if (inDisk)
            {
                ReadBufferInDiskChuncks(ref mesh_package);
            }
            else
            {
                ReadBufferInMemChuncks(ref mesh_package);
            }
        }
        void ReadBufferInDiskChuncks(ref RenderGeometryMeshPackage mesh_package)
        {
            var mesh_R = mesh_package.MeshResourceGroups[0].MeshResource[0];
            int chunk_i = 0;
            foreach (var item in mesh_R.StreamingBuffers)
            {
                if (item.BufferSize != 0 && item.TempBufferForPipeline == null)
                {
                    item.TempBufferForPipeline = new byte[item.BufferSize];
                    string pathFile = _file.InDiskPath;

                    while (mesh_R.StreamingChunks[chunk_i].BufferEnd <= item.BufferSize && mesh_R.StreamingChunks[chunk_i].BufferEnd != 0)
                    {
                        if (chunk_i == 1591)
                        {
                        }
                        string chunkPath = pathFile + string.Format("[{0}_mesh_resource.chunk{0}]", chunk_i);
                        var chunk = mesh_R.StreamingChunks[chunk_i];
                        if (File.Exists(chunkPath))
                        {
                            FileStream fileStream = new FileStream(chunkPath, FileMode.Open);
                            fileStream.Read(item.TempBufferForPipeline, chunk.BufferStart, chunk.BufferEnd - chunk.BufferStart);
                            fileStream.Close();
                        }
                        else
                            throw new Exception("No exist the file: " + chunkPath);
                        chunk_i++;
                    }

                }
            }


        }
        void ReadBufferInMemChuncks(ref RenderGeometryMeshPackage mesh_package)
        {
            if (mesh_package.MeshResourceGroups.Length == 0)
                return;
            var mesh_R = mesh_package.MeshResourceGroups[0].MeshResource[0];
            int chunk_i = 0;
            foreach (var item in mesh_R.StreamingBuffers)
            {
                if (item.BufferSize != 0 && item.TempBufferForPipeline == null)
                {
                    item.TempBufferForPipeline = new byte[item.BufferSize];
                    string pathFile = _file.Path_string;

                    while (chunk_i < mesh_R.StreamingChunks.Length && mesh_R.StreamingChunks[chunk_i].BufferEnd <= item.BufferSize && mesh_R.StreamingChunks[chunk_i].BufferEnd != 0)
                    {
                        string chunkPath = "";
                        if (_file.FileMemDescriptor.ResourceFiles.Count > chunk_i)
                        {
                            chunkPath = _file.FileMemDescriptor.ResourceFiles[chunk_i].Path_string;
                        }
                        else
                        {
                            chunkPath = pathFile + string.Format("[{0}:mesh resource.chunk{0}]", chunk_i);
                        }
                        var chunk = mesh_R.StreamingChunks[chunk_i];
                        FileDirModel file = HIFileContext.RootDir.GetChildByPath(chunkPath) as FileDirModel;
                        if (file != null && file.File != null)
                        {
                            var stream = file.File.GetStream();
                            if (stream != null)
                            {
                                //Debug.Assert(stream.Length == temp_z);
                                stream.Seek(0, SeekOrigin.Begin);
                                byte[] array = new byte[chunk.BufferEnd - chunk.BufferStart];
                                stream.Read(item.TempBufferForPipeline, chunk.BufferStart, chunk.BufferEnd - chunk.BufferStart);
                            }
                        }
                        else
                            throw new Exception("No exist the file: " + chunkPath);


                        chunk_i++;
                    }

                }
            }
        }

        byte[] ReadBufferInChuncks(HashSet<short[]> indices, s_render_geometry_api_resource api_resource, int offset, int byteSize)
        {
            bool found = false;
            int totalSize = 0;
            foreach (var item in api_resource.StreamingBuffers)
            {
                totalSize += item.BufferSize;
            }
            if (offset + byteSize > totalSize)
                return null;
            if (api_resource.StreamingBuffers.Length > 1)
            {
                Debug.Assert(api_resource.StreamingBuffers[1].BufferSize == 0);
            }
            MemoryStream fileStream = new MemoryStream(api_resource.StreamingBuffers[0].TempBufferForPipeline);
            byte[] buffer = new byte[byteSize];
            fileStream.Seek(offset, SeekOrigin.Begin);
            fileStream.Read(buffer, 0, byteSize);

            fileStream.Close();
            fileStream.Dispose();
            //return api_resource.StreamingBuffers[0].TempBufferForPipeline.Skip(offset).Take(byteSize).ToArray();
            return buffer;

            /*
            StreamingGeometryChunk[] listChunck = api_resource.StreamingChunks;
            foreach (var b in indices)
            {
                var chunck1 = listChunck[b.Item1];
                if (chunck1.BufferStart <= offset && chunck1.BufferEnd >= offset + byteSize)
                {
                    found = true;
                    string _path = _filePath + string.Format("[{0}:mesh resource.chunk{0}]", b.Item1);
                    //fileStream = new FileStream(new File(_path), FileAccess.Read);

                    FileDirModel file = HIFileContext.RootDir.GetChildByPath(_path) as FileDirModel;
                    if (file != null && file.File != null)
                    {
                        var stream = file.File.GetStream();
                        if (stream != null)
                        {
                            //Debug.Assert(stream.Length == temp_z);
                            stream.Seek(offset - chunck1.BufferStart, SeekOrigin.Begin);
                            byte[] array = new byte[byteSize];
                            stream.Read(array);
                            return array;
                        }
                    }
                    break;

                }
                else if (b.Item2 != -1)
                {
                    var chunck2 = listChunck[b.Item2];
                    if (chunck2.BufferStart <= offset && chunck2.BufferEnd >= offset + byteSize) {
                        string _path = _filePath + string.Format("[{0}:mesh resource.chunk{0}]", b.Item2);
                        //fileStream = new FileStream(new File(_path), FileAccess.Read);

                        FileDirModel file = HIFileContext.RootDir.GetChildByPath(_path) as FileDirModel;
                        if (file != null && file.File != null)
                        {
                            var stream = file.File.GetStream();
                            if (stream != null)
                            {
                                //Debug.Assert(stream.Length == temp_z);
                                stream.Seek(offset - chunck2.BufferStart, SeekOrigin.Begin);
                                byte[] array = new byte[byteSize];
                                stream.Read(array);
                                return array;
                            }
                        }
                        break;

                    }
                    else if (chunck1.BufferStart <= offset && chunck2.BufferEnd >= offset + byteSize)
                    {
                        found = true;
                        int bytesItem1 = chunck1.BufferEnd - offset;
                        int bytesItem2 = byteSize - bytesItem1;
                        byte[] array = new byte[byteSize];
                        string _path = _filePath + string.Format("[{0}:mesh resource.chunk{0}]", b.Item1);


                        FileDirModel file = HIFileContext.RootDir.GetChildByPath(_path) as FileDirModel; 

                        if (file != null && file.File != null)
                        {
                            var stream = file.File.GetStream();
                            if (stream != null)
                            {
                                //Debug.Assert(stream.Length == temp_z);
                                stream.Seek(offset - chunck1.BufferStart, SeekOrigin.Begin);

                                stream.Read(array,0, bytesItem1);
                                //return array;
                            }
                        }

                        string path2 = _filePath + string.Format("[{0}:mesh resource.chunk{0}]", b.Item2);


                        FileDirModel file2 = HIFileContext.RootDir.GetChildByPath(path2) as FileDirModel;

                        if (file2 != null && file2.File != null)
                        {
                            var stream = file2.File.GetStream();
                            if (stream != null)
                            {
                                //Debug.Assert(stream.Length == temp_z);
                                stream.Seek(0, SeekOrigin.Begin);

                                stream.Read(array, bytesItem1, bytesItem2);
                                return array;
                            }
                        }

                        break;
                    }

                }

            }
            Debug.Assert(found);    */
            return new byte[0];
        }


        private RenderGeometryMeshPackage GetRenderGeometryMeshPackage(TagInstance value)
        {
            RenderGeometryMeshPackage meshPackage = new RenderGeometryMeshPackage();
            meshPackage.Flags = (short)value["flags"].AccessValue;
            meshPackage.MeshResourcePackingPolicy = (MeshResourcePackingPolicy)(value["mesh resource packing policy"] as EnumGroup).SelectedIndex;
            meshPackage.TotalIndexBufferCount = (short)value["total index buffer count"].AccessValue;
            meshPackage.TotalVertexBufferCount = (short)value["total vertex buffer count"].AccessValue;
            ListTagInstance tempMRG = (ListTagInstance)value["mesh resource groups"];
            meshPackage.MeshResourceGroups = new RenderGeometryMeshPackageResourceGroup[tempMRG.Count];
            for (int i = 0; i < tempMRG.Count; i++)
            {
                meshPackage.MeshResourceGroups[i] = GetMeshPackageResourceGroup(tempMRG[i]);
            }

            ListTagInstance indexRLU = (ListTagInstance)value["index resource look up"];
            if (indexRLU != null)
            {
                meshPackage.IndexResourceLookUp = new RenderGeometryMeshPackage.ResourceLookup[indexRLU.Count];
                for (int i = 0; i < indexRLU.Count; i++)
                {
                    meshPackage.IndexResourceLookUp[i] = new RenderGeometryMeshPackage.ResourceLookup
                    {
                        GroupItemIndex = (short)indexRLU[i]["resource group index"].AccessValue,
                        ResourceGroupIndex = (short)indexRLU[i]["group item index"].AccessValue,
                    };
                }
            }


            ListTagInstance vertexRLU = (ListTagInstance)value["vertex resource look up"];
            if (vertexRLU != null)
            {
                meshPackage.VertexResourceLookUp = new RenderGeometryMeshPackage.ResourceLookup[vertexRLU.Count];
                for (int i = 0; i < vertexRLU.Count; i++)
                {
                    meshPackage.VertexResourceLookUp[i] = new RenderGeometryMeshPackage.ResourceLookup
                    {
                        GroupItemIndex = (short)vertexRLU[i]["resource group index"].AccessValue,
                        ResourceGroupIndex = (short)vertexRLU[i]["group item index"].AccessValue,
                    };
                }
            }

            return meshPackage;
        }

        private RenderGeometryMeshPackageResourceGroup GetMeshPackageResourceGroup(TagInstance value)
        {
            RenderGeometryMeshPackageResourceGroup temp = new RenderGeometryMeshPackageResourceGroup();
            ListTagInstance list = (ListTagInstance)value["mesh resource"];
            temp.MeshResource = new s_render_geometry_api_resource[list.Count];
            for (int i = 0; i < list.Count; i++)
            {
                temp.MeshResource[i] = new s_render_geometry_api_resource();

                ListTagInstance l_v_b = (ListTagInstance)list[i]["pc vertex buffers"];
                ListTagInstance l_i_b = (ListTagInstance)list[i]["pc index buffers"];
                ListTagInstance l_s_m = (ListTagInstance)list[i]["Streaming Meshes"];
                ListTagInstance l_s_c = (ListTagInstance)list[i]["Streaming Chunks"];
                ListTagInstance l_s_b = (ListTagInstance)list[i]["Streaming Buffers"];

                temp.MeshResource[i].m_sharedDXResources = (long)list[i]["m_sharedDXResources"].AccessValue;
                temp.MeshResource[i].m_sharedDXResourceRawView = (long)list[i]["m_sharedDXResourceRawView"].AccessValue;
                temp.MeshResource[i].RuntimeData = (long)list[i]["Runtime Data"].AccessValue;

                if (l_v_b != null)
                {
                    temp.MeshResource[i].PcVertexBuffers = new RasterizerVertexBuffer[l_v_b.Count];
                    for (int k = 0; k < l_v_b.Count; k++)
                    {
                        temp.MeshResource[i].PcVertexBuffers[k] = GetRasterizerVertexBuffer(l_v_b[k]);
                    }
                }
                if (l_i_b != null)
                {
                    temp.MeshResource[i].PcIndexBuffers = new RasterizerIndexBuffer[l_i_b.Count];
                    for (int k = 0; k < l_i_b.Count; k++)
                    {
                        temp.MeshResource[i].PcIndexBuffers[k] = GetRasterizerIndexBuffer(l_i_b[k]);
                    }
                }
                if (l_s_m != null)
                {
                    temp.MeshResource[i].StreamingMeshes = new StreamingGeometryMesh[l_s_m.Count];
                    for (int k = 0; k < l_s_m.Count; k++)
                    {
                        temp.MeshResource[i].StreamingMeshes[k] = GetStreamingGeometryMesh(l_s_m[k]);
                    }
                }
                if (l_s_c != null)
                {
                    temp.MeshResource[i].StreamingChunks = new StreamingGeometryChunk[l_s_c.Count];
                    for (int k = 0; k < l_s_c.Count; k++)
                    {
                        temp.MeshResource[i].StreamingChunks[k] = GetStreamingGeometryChunk(l_s_c[k]);
                        temp.MeshResource[i].StreamingChunks[k].BufferIndex = (short)k;
                    }
                }
                if (l_s_b != null)
                {
                    temp.MeshResource[i].StreamingBuffers = new StreamingGeometryBuffer[l_s_b.Count];
                    for (int k = 0; k < l_s_b.Count; k++)
                    {
                        temp.MeshResource[i].StreamingBuffers[k] = GetStreamingGeometryBuffer(l_s_b[k]);
                    }
                }

            }
            return temp;
        }

        RasterizerVertexBuffer GetRasterizerVertexBuffer(TagInstance value)
        {
            RasterizerVertexBuffer temp = new RasterizerVertexBuffer();
            temp.usage = (PcVertexBuffersUsage)(value["usage"] as EnumGroup).SelectedIndex;
            temp.format = (PcVertexBuffersFormat)(value["format"] as EnumGroup).SelectedIndex;
            temp.stride = (sbyte)value["stride"].AccessValue;
            temp.ownsD3DResource = (sbyte)value["ownsD3DResource"].AccessValue;
            temp.count = (int)value["count"].AccessValue;
            temp.offset = (int)value["offset"].AccessValue;
            temp.d3dbuffer = new D3DBufferData
            {
                ByteWidth = (int)value["d3dbuffer"]["byte width"].AccessValue,
                BindFlags = (SharpDX.Direct3D11.BindFlags)value["d3dbuffer"]["bind flags"].AccessValue,
                MiscFlags = (ResourceMiscFlags)value["d3dbuffer"]["misc flags"].AccessValue,
                StructureByteStride = (int)value["d3dbuffer"]["stride"].AccessValue,
                //D3dBuffer = value["d3dbuffer"]["d3d buffer"].AccessValue,
                Usage = (short)value["d3dbuffer"]["usage"].AccessValue,
                CPUAccessFlags = (short)value["d3dbuffer"]["cpu flags"].AccessValue,
            };
            int temp_c = (int)value["d3dbuffer"]["d3d buffer"].AccessValue;
            if (temp_c != 0)
            {
                TagData tempTagData = value["d3dbuffer"]["d3d buffer"] as TagData;
                Debug.Assert(temp.ownsD3DResource != 0);
                temp.d3dbuffer.D3dBuffer = tempTagData.ReadBuffer();
                // temp.d3dbuffer.D3dBuffer = new byte[temp_c];
                
            }


            temp.m_resource = (long)value["m_resource"].AccessValue;
            temp.m_resourceView = (long)value["m_resourceView"].AccessValue;

            return temp;
        }
        RasterizerIndexBuffer GetRasterizerIndexBuffer(TagInstance value)
        {
            RasterizerIndexBuffer temp = new();
            temp.DeclarationType = (sbyte)value["declaration type"].AccessValue;
            temp.Stride = (sbyte)value["stride"].AccessValue;
            temp.OwnsD3DResource = (sbyte)value["ownsD3DResource"].AccessValue;
            temp.count = (int)value["count"].AccessValue;
            temp.offset = (int)value["offset"].AccessValue;
            temp.d3dbuffer = new D3DBufferData
            {
                ByteWidth = (int)value["d3dbuffer"]["byte width"].AccessValue,
                BindFlags = (SharpDX.Direct3D11.BindFlags)value["d3dbuffer"]["bind flags"].AccessValue,
                MiscFlags = (ResourceMiscFlags)value["d3dbuffer"]["misc flags"].AccessValue,
                StructureByteStride = (int)value["d3dbuffer"]["stride"].AccessValue,
                //D3dBuffer = value["d3dbuffer"]["d3d buffer"].AccessValue,
                Usage = (short)value["d3dbuffer"]["usage"].AccessValue,
                CPUAccessFlags = (short)value["d3dbuffer"]["cpu flags"].AccessValue,
            };
            int temp_c = (int)value["d3dbuffer"]["d3d buffer"].AccessValue;
            if (temp_c != 0)
            {
                Debug.Assert(temp.OwnsD3DResource != 0);
                temp.d3dbuffer.D3dBuffer = new byte[temp_c];
            }


            temp.m_resource = (long)value["m_resource"].AccessValue;
            temp.m_resourceView = (long)value["m_resourceView"].AccessValue;

            return temp;
        }
        StreamingGeometryMesh GetStreamingGeometryMesh(TagInstance value)
        {
            StreamingGeometryMesh temp = new();
            temp.LodStateCacheSlot = (int)value["lod state cache slot"].AccessValue;
            temp.RequiredLod = (sbyte)value["required lod"].AccessValue;
            ListTagInstance temp_ch = (ListTagInstance)value["mesh lod chunks"];
            if (temp_ch != null)
            {
                temp.MeshLodChunks = new StreamingChunkList[temp_ch.Count];
                for (int i = 0; i < temp_ch.Count; i++)
                {
                    var v = temp_ch[i];
                    temp.MeshLodChunks[i] = new StreamingChunkList();
                    temp.MeshLodChunks[i].Chunks = (v["chunks"] as TagReader.TagData).ReadBuffer();
                    //InterpretChunkParameterInfo(temp.MeshLodChunks[i].Chunks);
                }
            }

            return temp;
        }

        private static Int16[] InterpretChunkParameterInfo(byte[]? Chunks)
        {
            Debug.Assert(Chunks != null && Chunks.Length % 2 == 0);
            Int16[] salida = new Int16[Chunks.Length / 2];
            for (int i = 0; i < salida.Length; i++)
            {
                salida[i] = BitConverter.ToInt16(Chunks, i * 2);
                /*if (i > 0 && (i+1)%2==0)
                    Debug.Assert(salida[i] - salida[i-1] == 1);*/
            }
            return salida;
        }

        StreamingGeometryChunk GetStreamingGeometryChunk(TagInstance value)
        {
            StreamingGeometryChunk temp = new();
            temp.BufferIndex = (short)value["buffer index"].AccessValue;
            temp.AllocationPriority = (short)value["allocation priority"].AccessValue;
            temp.BufferStart = (int)value["buffer start"].AccessValue;
            temp.BufferEnd = (int)value["buffer end"].AccessValue;
            return temp;
        }
        StreamingGeometryBuffer GetStreamingGeometryBuffer(TagInstance value)
        {
            StreamingGeometryBuffer temp = new();
            temp.BufferSize = (int)value["buffer size"].AccessValue;
            temp.BindFlags = (int)value["bind flags"].AccessValue;
            //
            temp.TempBufferForPipeline = null;//new byte[(int)value["Temp buffer for pipeline"].AccessValue];
            return temp;
        }

        CompressionInfo createCompressionInfo()
        {
            CompressionInfo temp = new CompressionInfo();
            var comp_info = _renderGeometry["compression info"]["[0]"];
            if (comp_info == null)
                return temp;

            var pb0 = comp_info["position bounds 0"] as Point3D;
            var pb1 = comp_info["position bounds 1"] as Point3D;
            temp.ModelScale.M11 = pb0.X;
            temp.ModelScale.M12 = pb0.Y;
            temp.ModelScale.M13 = pb0.Y - pb0.X;
            temp.ModelScale.M21 = pb0.Z;
            temp.ModelScale.M22 = pb1.X;
            temp.ModelScale.M23 = pb1.X - pb0.Z;
            temp.ModelScale.M31 = pb1.Y;
            temp.ModelScale.M32 = pb1.Z;
            temp.ModelScale.M33 = pb1.Z - pb1.Y;

            var uv00 = comp_info["texcoord bounds 0"] as Point2DFloat;
            var uv01 = comp_info["texcoord bounds 1"] as Point2DFloat;
            temp.Uv0Scale.M11 = uv00.X;
            temp.Uv0Scale.M12 = uv00.Y;
            temp.Uv0Scale.M13 = uv00.Y - uv00.X;
            temp.Uv0Scale.M21 = uv01.X;
            temp.Uv0Scale.M22 = uv01.Y;
            temp.Uv0Scale.M23 = uv01.Y - uv01.X;

            var uv10 = comp_info["texcoord bounds2 0"] as Point2DFloat;
            var uv11 = comp_info["texcoord bounds2 1"] as Point2DFloat;
            temp.Uv1Scale.M11 = uv10.X;
            temp.Uv1Scale.M12 = uv10.Y;
            temp.Uv1Scale.M13 = uv10.Y - uv10.X;
            temp.Uv1Scale.M21 = uv11.X;
            temp.Uv1Scale.M22 = uv11.Y;
            temp.Uv1Scale.M23 = uv11.Y - uv11.X;

            var uv20 = comp_info["texcoord bounds3 0"] as Point2DFloat;
            var uv21 = comp_info["texcoord bounds3 1"] as Point2DFloat;
            temp.Uv1Scale.M11 = uv20.X;
            temp.Uv1Scale.M12 = uv20.Y;
            temp.Uv1Scale.M13 = uv20.Y - uv20.X;
            temp.Uv1Scale.M21 = uv21.X;
            temp.Uv1Scale.M22 = uv21.Y;
            temp.Uv1Scale.M23 = uv21.Y - uv21.X;


            return temp;
        }

    }
}
