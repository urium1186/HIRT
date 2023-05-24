namespace LibHIRT.Domain
{
    public enum BufferVertType
    {
        position = 0,
        texcoord = 1,
        texcoord1 = 2,
        texcoord2 = 3,
        lightmap_texcoord = 4,
        normal = 5,
        tangent = 6,
        node_indices = 7,
        node_weights = 8,
        binormal = 9,
        dual_quat_weight = 10,
        vertex_color = 11,
        vertex_alpha = 12,
        tangent_UV2 = 13,
        tangent_UV3 = 14,
        change_15 = 15,
        change_16 = 16,
        change_17 = 17,
        change_18 = 18,
    }

    [Flags]
    public enum PcVertexBuffersUsage : int
    {
        Position = 0,
        UV0 = 1,
        UV1 = 2,
        UV2 = 3,
        Color = 4,
        Normal = 5,
        Tangent = 6,
        BlendIndices0 = 7,
        BlendWeights0 = 8,
        BlendIndices1 = 9,
        BlendWeights1 = 10,
        PrevPosition = 11,
        InstanceData = 12,
        BlendshapePosition = 13,
        BlendshapeNormal = 14,
        BlendshapeIndex = 15,
        Edge = 16,
        EdgeIndex = 17,
        EdgeIndexInfo = 18
    }

    [Flags]
    public enum PcVertexBuffersFormat : int
    {
        real = 0,
        realVector2D = 1,
        realVector3D = 2,
        realVector4D = 3,
        byteVector4D = 4,
        byteARGBColor = 5,
        shortVector2D = 6,
        shortVector2DNormalized = 7,
        shortVector4DNormalized = 8,
        wordVector2DNormalized = 9,
        wordVector4DNormalized = 10,
        real16Vector2D = 11,
        real16Vector4D = 12,
        f_10_10_10_normalized = 13,
        f_10_10_10_2 = 14,
        f_10_10_10_2_signedNormalizedPackedAsUnorm = 15,
        dword = 16,
        dwordVector2D = 17,
        f_11_11_10_float = 18,
        byteUnitVector3D = 19,
        wordVector3DNormalizedWith4Word = 20
    }
    [Flags]
    public enum MeshResourcePackingPolicy : Int16
    {
        Single_Resource = 0,
        N_Meshes_Per_Resource = 1,
    }


    [Flags]
    public enum IndexBufferType : int
    {
        DEFAULT = 0,
        line_list = 1,
        line_strip = 2,
        triangle_list = 3,
        triangle_patch = 4,
        triangle_strip = 5,
        quad_list = 6,
    }
    [Flags]
    public enum VertType : int
    {
        world = 0,
        rigid = 1,
        skinned = 2,
        particle_model = 3,
        screen = 4,
        debug = 5,
        transparent = 6,
        particle = 7,
        removed08 = 8,
        removed09 = 9,
        chud_simple = 10,
        decorator = 11,
        position_only = 12,
        removed13 = 13,
        ripple = 14,
        removed15 = 15,
        tessellatedTerrain = 16,
        empty = 17,
        decal = 18,
        removed19 = 19,
        removed20 = 20,
        position_only_21 = 21,
        tracer = 22,
        rigid_boned = 23,
        removed24 = 24,
        CheapParticle = 25,
        dq_skinned = 26,
        skinned_8_weights = 27,
        tessellated_vector = 28,
        interaction = 29,
        number_of_standard_vertex_types = 30,
    }
    internal class TypesDefinition
    {
    }
}
