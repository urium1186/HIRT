using Oodle.NET;

namespace Oodle
{
    public enum OodleFormat : uint
    {
        LZH,
        LZHLW,
        LZNIB,
        None,
        LZB16,
        LZBLW,
        LZA,
        LZNA,
        Kraken,
        Mermaid,
        BitKnit,
        Selkie,
        Akkorokamui
    }

    public enum OodleCompressionLevel : ulong
    {
        None,
        SuperFast,
        VeryFast,
        Fast,
        Normal,
        Optimal1,
        Optimal2,
        Optimal3,
        Optimal4,
        Optimal5
    }

    public unsafe static class OodleWrapper
    {
        static private OodleCompressor? oodle = null;
        unsafe public static byte[] Decompress(byte[] compressedBuffer, int length, int decompressedSize)
        {
            if (oodle == null)
                oodle = new OodleCompressor(@".\libs\oo2core_8_win64.dll");

            if (oodle == null)
                throw new Exception("Oodle do not load!!!!");

            var decompressedBuffer = new byte[decompressedSize];
            long v = oodle.DecompressBuffer(compressedBuffer, compressedBuffer.Length, decompressedBuffer, decompressedSize, OodleLZ_FuzzSafe.No, OodleLZ_CheckCRC.No, OodleLZ_Verbosity.None, 0L, 0L, 0L, 0L, 0L, 0L, OodleLZ_Decode_ThreadPhase.Unthreaded);
            return decompressedBuffer;
        }

        public static byte[] Compress(byte[] buffer, int size, uint out_size = 0, OodleLZ_Compressor format = OodleLZ_Compressor.Kraken, OodleLZ_CompressionLevel level = OodleLZ_CompressionLevel.Optimal5)
        {
            if (oodle == null)
                oodle = new OodleCompressor(@".\libs\oo2core_8_win64.dll");

            if (oodle == null)
                throw new Exception("Oodle do not load!!!!");

            byte[] skBuffer = new byte[0];
            uint skBufferSize = (uint)skBuffer.Length;
            uint compressedBufferSize = out_size;
            if (out_size==0)
                compressedBufferSize  = GetCompressionBound((uint)size);
            byte[] compressedBuffer = new byte[compressedBufferSize];

            long compressedCount = oodle.CompressBuffer(format, buffer, size, compressedBuffer, level, 0L, 0L, 0L, 0L, skBufferSize);

            byte[] outputBuffer = new byte[compressedCount];
            Buffer.BlockCopy(compressedBuffer, 0, outputBuffer, 0, (int)compressedCount);

            return outputBuffer;
        }

        private static uint GetCompressionBound(uint bufferSize)
        {
            return bufferSize + 274 * ((bufferSize + 0x3FFFF) / 0x40000);
        }
    }
}