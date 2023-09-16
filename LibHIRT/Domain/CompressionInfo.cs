namespace LibHIRT.Domain
{
    public struct Matrix3x3
    {
        public float M11;
        public float M12;
        public float M13;

        public float M21;
        public float M22;
        public float M23;

        public float M31;
        public float M32;
        public float M33;

    }

    public struct Matrix2x3
    {
        public float M11;
        public float M12;
        public float M13;

        public float M21;
        public float M22;
        public float M23;

    }

    public struct CompressionInfo
    {
        public int CompressionFlags;
        public Matrix3x3 ModelScale;
        public Matrix2x3 Uv0Scale;
        public Matrix2x3 Uv1Scale;
        public Matrix2x3 Uv2Scale;

    }
}
