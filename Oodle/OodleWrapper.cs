using Oodle.NET;

namespace Oodle
{
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
    }
}