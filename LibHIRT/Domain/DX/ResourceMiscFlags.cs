namespace LibHIRT.Domain.DX
{
    [Flags]
    public enum ResourceMiscFlags
    {
        None = 0,
        GenerateMips = 1,
        Shared = 2,
        TextureCube = 3,
        DrawIndirect = 4,
        BufferAllowRawViews = 5,
        BufferStructured = 6,
        ResourceClamp = 7,
        SharedKeyedMutex = 8,
        GdiCompatible = 9,
    }
}
