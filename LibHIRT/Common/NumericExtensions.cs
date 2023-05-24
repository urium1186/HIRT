using System.Diagnostics;
using System.Numerics;

namespace LibHIRT.Common
{
    /*
     using System.Windows.Media.Media3D;

Vector3D vector = new Vector3D(1, 2, -3); // Vector de ejemplo
Vector3D up = new Vector3D(0, 1, 0); // Vector que representa la dirección "arriba"
Vector3D down = new Vector3D(0, -1, 0); // Vector que representa la dirección "abajo"
Vector3D right = new Vector3D(1, 0, 0); // Vector que representa la dirección "derecha"
Vector3D left = new Vector3D(-1, 0, 0); // Vector que representa la dirección "izquierda"
Vector3D forward = new Vector3D(0, 0, -1); // Vector que representa la dirección "adelante"
Vector3D backward = new Vector3D(0, 0, 1); // Vector que representa la dirección "atrás"

double dotProduct = Vector3D.DotProduct(vector, up);
if (dotProduct > 0)
{
    // El vector está en dirección "arriba"
}
else if (dotProduct < 0)
{
    // El vector está en dirección "abajo"
}
// Repite el proceso con los otros vectores de dirección para determinar las demás direcciones

     */
    public static class NumericExtensions
    {

        /// <summary>
        ///   Converts an SNorm-compressed 8-bit integer to a float.
        /// </summary>
        /// <param name="value">
        ///   The _intValue to convert.
        /// </param>
        /// <returns>
        ///   The uncompressed _intValue.
        /// </returns>
        [DebuggerHidden]
        public static float SNormToFloat(this sbyte value)
          => value / (float)sbyte.MaxValue; // TODO: Saber might handle this differently.

        /// <summary>
        ///   Converts an SNorm-compressed 16-bit integer to a float.
        /// </summary>
        /// <param name="value">
        ///   The _intValue to convert.
        /// </param>
        /// <returns>
        ///   The uncompressed _intValue.
        /// </returns>
        [DebuggerHidden]
        public static float SNormToFloat(this short value)
          => value / (float)short.MaxValue; // TODO: Saber might handle this differently.

        /// <summary>
        ///   Converts a UNorm-compressed 8-bit integer to a float.
        /// </summary>
        /// <param name="value">
        ///   The _intValue to convert.
        /// </param>
        /// <returns>
        ///   The uncompressed _intValue.
        /// </returns>
        [DebuggerHidden]
        public static float UNormToFloat(this byte value)
          => value / (float)byte.MaxValue; // TODO: Saber might handle this differently.

        public static Matrix4x4 TRS(this Vector3 translation, Quaternion rotation, Vector3 scale)
        {
            return Matrix4x4.CreateFromQuaternion(rotation) *
                   //Matrix4x4.CreateScale(scale) *
                   Matrix4x4.CreateTranslation(translation);
        }

        public static double GetTransformDistance(this Matrix4x4 matrix1, Matrix4x4 matrix2)
        {
            double distance = 0.0;
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    double diff = matrix1.getElementAt(i, j) - matrix2.getElementAt(i, j);
                    distance += diff * diff;
                }
            }
            return Math.Sqrt(distance);
        }

        public static float getElementAt(this Matrix4x4 matrix, int i, int j)
        {
            if (i == 0 && j == 0)
                return matrix.M11;
            else if (i == 0 && j == 1)
                return matrix.M12;
            else if (i == 0 && j == 2)
                return matrix.M13;
            else if (i == 0 && j == 3)
                return matrix.M14;
            else if (i == 1 && j == 0)
                return matrix.M21;
            else if (i == 1 && j == 1)
                return matrix.M22;
            else if (i == 1 && j == 2)
                return matrix.M23;
            else if (i == 1 && j == 3)
                return matrix.M24;
            else if (i == 2 && j == 0)
                return matrix.M31;
            else if (i == 2 && j == 1)
                return matrix.M32;
            else if (i == 2 && j == 2)
                return matrix.M33;
            else if (i == 2 && j == 3)
                return matrix.M34;
            else if (i == 3 && j == 0)
                return matrix.M41;
            else if (i == 3 && j == 1)
                return matrix.M42;
            else if (i == 3 && j == 2)
                return matrix.M43;

            return matrix.M44;

        }
        // get is a vector direction is point up or down or left or right or forward or backward
        public static bool isVectorDirection(this Vector3 vector, Vector3 direction)
        {
            double dotProduct = Vector3.Dot(vector, direction);
            if (dotProduct > 0)
            {
                return true;
            }
            else if (dotProduct < 0)
            {
                return false;
            }
            return false;
        }
        // get is a vector direction is point up or down or left or right or forward or backward result in enum
        public static VectorDirection getVectorDirection(this Vector3 vector)
        {
            Vector3 up = new Vector3(0, 1, 0); // Vector that represents the direction "up"
            Vector3 down = new Vector3(0, -1, 0); // Vector that represents the direction "down"
            Vector3 right = new Vector3(1, 0, 0); // Vector that represents the direction "right"
            Vector3 left = new Vector3(-1, 0, 0); // Vector that represents the direction "left"
            Vector3 forward = new Vector3(0, 0, -1); // Vector that represents the direction "forward"
            Vector3 backward = new Vector3(0, 0, 1); // Vector that represents the direction "backward"
            double dotProduct = Vector3.Dot(vector, up);
            if (dotProduct > 0)
            {
                return VectorDirection.Up;
            }
            else if (dotProduct < 0)
            {
                dotProduct = Vector3.Dot(vector, down);
                if (dotProduct > 0)
                {
                    return VectorDirection.Down;
                }
            }
            dotProduct = Vector3.Dot(vector, right);
            if (dotProduct > 0)
            {
                return VectorDirection.Right;
            }
            else if (dotProduct < 0)
            {
                dotProduct = Vector3.Dot(vector, left);
                if (dotProduct > 0)
                {
                    return VectorDirection.Left;
                }
            }
            dotProduct = Vector3.Dot(vector, forward);
            if (dotProduct > 0)
            {
                return VectorDirection.Forward;
            }
            else if (dotProduct < 0)
            {
                dotProduct = Vector3.Dot(vector, backward);
                if (dotProduct > 0)
                {
                    return VectorDirection.Backward;
                }
            }
            return VectorDirection.None;
        }
        // create the VectorDireciton enum
        public enum VectorDirection
        {
            Up,
            Down,
            Right,
            Left,
            Forward,
            Backward,
            None
        }
        //  get is a vector direction is point up or down or left or right or forward or backward vs parent node is transfor matrix
        public static bool isVectorDirection(this Vector3 vector, Vector3 direction, Matrix4x4 parentTransform)
        {
            Vector3 parentVector = Vector3.Transform(vector, parentTransform);
            double dotProduct = Vector3.Dot(parentVector, direction);
            if (dotProduct > 0)
            {
                return true;
            }
            else if (dotProduct < 0)
            {
                return false;
            }
            return false;
        }

        // get the result in a enum
        public static VectorDirection getVectorDirection(this Vector3 vector, Matrix4x4 parentTransform)
        {
            Vector3 parentVector = Vector3.Transform(vector, parentTransform);
            Vector3 up = new Vector3(0, 1, 0); // Vector that represents the direction "up"
            Vector3 down = new Vector3(0, -1, 0); // Vector that represents the direction "down"
            Vector3 right = new Vector3(1, 0, 0); // Vector that represents the direction "right"
            Vector3 left = new Vector3(-1, 0, 0); // Vector that represents the direction "left"
            Vector3 forward = new Vector3(0, 0, -1); // Vector that represents the direction "forward"
            Vector3 backward = new Vector3(0, 0, 1); // Vector that represents the direction "backward"
            double dotProduct = Vector3.Dot(parentVector, up);
            if (dotProduct > 0)
            {
                return VectorDirection.Up;
            }
            else if (dotProduct < 0)
            {
                dotProduct = Vector3.Dot(parentVector, down);
                if (dotProduct > 0)
                {
                    return VectorDirection.Down;
                }
            }
            dotProduct = Vector3.Dot(parentVector, right);
            if (dotProduct > 0)
            {
                return VectorDirection.Right;
            }
            else if (dotProduct < 0)
            {
                dotProduct = Vector3.Dot(parentVector, left);
                if (dotProduct > 0)
                {
                    return VectorDirection.Left;
                }
            }
            dotProduct = Vector3.Dot(parentVector, forward);
            if (dotProduct > 0)
            {
                return VectorDirection.Forward;
            }
            else if (dotProduct < 0)
            {
                dotProduct = Vector3.Dot(parentVector, backward);
                if (dotProduct > 0)
                {
                    return VectorDirection.Backward;
                }
            }
            return VectorDirection.None;
        }

    }

}
