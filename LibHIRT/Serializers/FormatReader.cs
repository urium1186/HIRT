using System.Diagnostics;
using SharpDX.Mathematics;
using SharpDX.Animation;
using System.Collections;
using System.Numerics;

namespace LibHIRT.Serializers
{
    public static class FormatReader
    {
        public static (byte, byte, byte, byte) ReadByteARGBColor(byte[]? in_byte) {
            Debug.Assert(in_byte != null && in_byte.Length == 4);
            return (in_byte[0], in_byte[1], in_byte[2], in_byte[3]);
        }
        public static (float, float, float, float) ReadWordVector4DNormalized(byte[]? in_byte) {
            Debug.Assert(in_byte != null && in_byte.Length == 8);
            return ((float)(BitConverter.ToUInt16(in_byte,0)/ 65535.0f), (BitConverter.ToUInt16(in_byte, 2) / 65535.0f), (BitConverter.ToUInt16(in_byte, 4) / 65535.0f), (BitConverter.ToUInt16(in_byte, 6) / 65535.0f));
        }
        
        public static (float, float) ReadWordVector2DNormalized(byte[]? in_byte) {
            Debug.Assert(in_byte != null && in_byte.Length == 4);
            return ((BitConverter.ToUInt16(in_byte,0)/ 65535.0f), (BitConverter.ToUInt16(in_byte, 2) / 65535.0f));
        }
        
        
        public static float ReadReal(byte[]? in_byte) {
            Debug.Assert(in_byte != null && in_byte.Length == 4);
            return (BitConverter.ToSingle(in_byte,0));
        }

        public static (byte, byte, byte) ReadByteUnitVector3D(byte[]? in_byte)
        {
            Debug.Assert(in_byte != null && in_byte.Length == 3);
            return (in_byte[0], in_byte[1], in_byte[2]);
        }
        
        public static (byte, byte, byte, byte) readByteVector4D(byte[]? in_byte)
        {
            Debug.Assert(in_byte != null && in_byte.Length == 4);
            return (in_byte[0], in_byte[1], in_byte[2], in_byte[3]);
        }

        public static (float, float, float, float) ReadF_10_10_10_normalized(byte[]? in_byte)
        {
            Debug.Assert(in_byte != null && in_byte.Length == 4);
            var inter = InterpretVertexBuffer(BitConverter.ToInt32(in_byte));
            float fx;
            float fy;
            float fz;
            UnpackF_10_10_10_Normalized(BitConverter.ToUInt32(in_byte),out fx, out fy,out fz);
            float x;
            float y;
            float z;
            float w;

            NormalizeWeights(BitConverter.ToUInt32(in_byte),out x,out y,out z,out w);
            BitArray temp = new BitArray(in_byte);
            BitArray myB10 = new BitArray(new bool[32] { true, true, true, true, true, true, true, true, true, true,
                                                        false, false, false, false, false, false, false, false, false, false,
                                                        false, false, false, false, false, false, false, false, false, false,
                                                        false, false});
            BitArray myB20 = new BitArray(new bool[32] { false, false, false, false, false, false, false, false, false, false,
                                                        true, true, true, true, true, true, true, true, true, true,
                                                        false, false, false, false, false, false, false, false, false, false,
                                                        false, false});
            BitArray myB30 = new BitArray(new bool[32] { false, false, false, false, false, false, false, false, false, false,
                                                        false, false, false, false, false, false, false, false, false, false,
                                                        true, true, true, true, true, true, true, true, true, true,
                                                        false, false});
            myB10.And(temp);
            var sB10 = BitArrayToStringBit(myB10);
            myB20.And(temp);
            var sB20 = BitArrayToStringBit(myB20);
            myB20.RightShift(10);
            var sB20L = BitArrayToStringBit(myB20);
            myB30.And(temp);
            var sB30 = BitArrayToStringBit(myB30);
            myB30.RightShift(20);
            var sB30L = BitArrayToStringBit(myB30);
            float f1 = (float)(BitConverter.ToInt32(BitArrayToByteArray(myB10)) / 1023.0) ;
            float f2 = (float)(BitConverter.ToInt32(BitArrayToByteArray(myB20)) / 1023.0);
            float f3 = (float)(BitConverter.ToInt32(BitArrayToByteArray(myB30)) / 1023.0);

            float f12 = (float)(BitConverter.ToSingle(BitArrayToByteArray(myB10)));
            float f22 = (float)(BitConverter.ToSingle(BitArrayToByteArray(myB20)) );
            float f32 = (float)(BitConverter.ToSingle(BitArrayToByteArray(myB30)) );
            //SharpDX.Direct3D12.FeatureDataFormatInformation
            //BitConverter.
            Debug.Assert(f12<=10 && f22 <= 10 && f32 <= 10);
            Debug.Assert(f12>=0 && f22 >= 0 && f32 >= 0);
            return (f1,f2,f3,0);
        }

        public static (float, float, float, float) ReadF_10_10_10_2_signedNormalizedPackedAsUnorm(byte[]? in_byte)
        {
            Debug.Assert(in_byte != null && in_byte.Length == 4);
            //SharpDX.Direct3D12.FeatureDataFormatInformation
            //BitConverter.
            return ((float)(BitConverter.ToInt16(in_byte, 0) / 65535.0), (float)(BitConverter.ToInt16(in_byte, 2) / 65535.0), (float)(BitConverter.ToInt16(in_byte, 4) / 65535.0), (float)(BitConverter.ToInt16(in_byte, 6) / 65535.0));
        }

        public static byte[] BitArrayToByteArray(BitArray bits)
        {
            byte[] ret = new byte[(bits.Length - 1) / 8 + 1];
            bits.CopyTo(ret, 0);
            return ret;
        }

        public static string BitArrayToStringBit(BitArray bits)
        {
            string result = "";
            for (int i = 0; i < bits.Count; i++)
            {

                if (bits[i])
                {
                    result = result + "1";
                }
                else {
                    result = result + "0";
                }
            }
            return result;
        }

        static private Vector3 InterpretVertexBuffer(int packedValue)
        {
            int x = packedValue & 0x3ff;
            int y = (packedValue >> 10) & 0x3ff;
            int z = (packedValue >> 20) & 0x3ff;
            int w = (packedValue >> 30) & 0x3;

            x = (x << 6) >> 6;
            y = (y << 6) >> 6;
            z = (z << 6) >> 6;
            w = (w << 30) >> 30;

            float xf = x / 1023.0f;
            float yf = y / 1023.0f;
            float zf = z / 1023.0f;
            float wf = w / 1.0f;

            return new Vector3(xf, yf, zf);
        }

        static private void UnpackF_10_10_10_Normalized(uint packedValue, out float x, out float y, out float z)
        {
            // Divide cada componente en un número entero no negativo de 10 bits
            int packedX = (int)(packedValue & 0x3FF);
            int packedY = (int)((packedValue >> 10) & 0x3FF);
            int packedZ = (int)((packedValue >> 20) & 0x3FF);

            // Convierte cada componente a un número flotante normalizado
            x = (float)packedX / 1023.0f;
            y = (float)packedY / 1023.0f;
            z = (float)packedZ / 1023.0f;
        }

        static private void UnpackWeight4(uint packedValue, out float x, out float y, out float z, out float w)
        {
            // Divide cada componente en un número entero no negativo de 10 bits
            int packedX = (int)(packedValue & 0x3FF);
            int packedY = (int)((packedValue >> 10) & 0x3FF);
            int packedZ = (int)((packedValue >> 20) & 0x3FF);
            int packedW = (int)((packedValue >> 30) & 0x3);

            // Convierte cada componente a un número flotante normalizado
            x = (float)packedX / 1023.0f;
            y = (float)packedY / 1023.0f;
            z = (float)packedZ / 1023.0f;
            w = (float)packedW / 3.0f;
        }

        static private void NormalizeWeights(uint packedValue, out float x, out float y, out float z, out float w)
        {
            // Desempaqueta los pesos
            int packedX = (int)(packedValue & 0x3FF);
            int packedY = (int)((packedValue >> 10) & 0x3FF);
            int packedZ = (int)((packedValue >> 20) & 0x3FF);
            int packedW = (int)((packedValue >> 30) & 0x3);

            // Convierte los pesos a valores flotantes
            x = (float)packedX / 1023.0f;
            y = (float)packedY / 1023.0f;
            z = (float)packedZ / 1023.0f;
            w = (float)packedW / 3.0f;

            // Suma de todos los pesos
            float sum = x + y + z + w;

            // Verificación de la suma
            if (sum != 0)
            {
                // Normalización de los pesos
                x /= sum;
                y /= sum;
                z /= sum;
                w /= sum;
            }
            else
            {
                // Si la suma es cero, todos los pesos son 0
                x = y = z = w = 0;
            }
        }

    }
}
