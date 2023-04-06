using Assimp;
using HelixToolkit.SharpDX.Core.Animations;
using HelixToolkit.SharpDX.Core.Model.Scene;
using LibHIRT.Domain.RenderModel;
using LibHIRT.TagReader;
using SharpDX;
using System;

namespace HaloInfiniteResearchTools.Common.Extensions
{

    public static class AssimpExtensions
    {
        
        public static Assimp.Matrix4x4 calculateGlobalTransformation(this Node node)
        {
            var initT = node.Transform;

            Node parent = node.Parent;

            while (parent != null)
            {
                // ... and all local transformations of the parent bones (recursively)
                initT = parent.Transform * initT;
                parent = parent.Parent;
            }
            return initT;
        } 
        public static Matrix calculateGlobalTransformation(this SceneNode node)
        {
            var initT = node.ModelMatrix;

            SceneNode parent = node.Parent;

            while (parent != null)
            {
                // ... and all local transformations of the parent bones (recursively)
                initT = parent.ModelMatrix * initT;
                parent = parent.Parent;
            }
            return initT;
        }
        public static Assimp.Matrix4x4 ToAssimp(this System.Numerics.Matrix4x4 m, bool transpose = true)
        {
            if (transpose)
                m = System.Numerics.Matrix4x4.Transpose(m);

            return new Assimp.Matrix4x4(
              m.M11, m.M12, m.M13, m.M14,
              m.M21, m.M22, m.M23, m.M24,
              m.M31, m.M32, m.M33, m.M34,
              m.M41, m.M42, m.M43, m.M44
              );
        }

        public static SharpDX.Matrix ToSharpDX(this System.Numerics.Matrix4x4 m, bool transpose = true)
        {
            if (transpose)
                m = System.Numerics.Matrix4x4.Transpose(m);

            return new SharpDX.Matrix(
              m.M11, m.M12, m.M13, m.M14,
              m.M21, m.M22, m.M23, m.M24,
              m.M31, m.M32, m.M33, m.M34,
              m.M41, m.M42, m.M43, m.M44
              );
        }

        public static System.Numerics.Matrix4x4 ToNumerics(this Assimp.Matrix4x4 m, bool transpose = true)
        {
            if (transpose)
                m.Transpose();

            return new System.Numerics.Matrix4x4(
              m.A1, m.A2, m.A3, m.A4,
              m.B1, m.B2, m.B3, m.B4,
              m.C1, m.C2, m.C3, m.C4,
              m.D1, m.D2, m.D3, m.D4);
        }

        public static Assimp.Vector3D ToAssimp(this System.Numerics.Vector3 v, bool traspose = true)
        {
            if (traspose)
                return new Assimp.Vector3D(-v.X, v.Z, v.Y);
            else
                return new Assimp.Vector3D(v.X, v.Y, v.Z);
        }

        public static Assimp.Vector3D ToAssimp3D(this System.Numerics.Vector4 v, bool traspose = true)
        {
            if (traspose)
                return new Assimp.Vector3D(-v.X, v.Z, v.Y);
            else
                return new Assimp.Vector3D(v.X, v.Y, v.Z);
        }

        public static System.Numerics.Vector3 ToNumerics(this Assimp.Vector3D v, bool traspose = true)
        {
            if (traspose)
                return new System.Numerics.Vector3(-v.X, v.Z, v.Y);
            else
                return new System.Numerics.Vector3(v.X, v.Y, v.Z);
        }

        // transpose a Numerix Vector4 from a Numeric Vector3
        public static System.Numerics.Vector4 ToNumerics4(this System.Numerics.Vector3 v, bool traspose = true)
        {
            if (traspose)
                return new System.Numerics.Vector4(-v.X, v.Z, v.Y, 1);
            else
                return new System.Numerics.Vector4(v.X, v.Y, v.Z, 1);
        }

        public static Assimp.Quaternion ToAssimp(this System.Numerics.Quaternion q, bool traspose = true)
        {
            if (traspose)
                return new Assimp.Quaternion(-q.X, q.Z, q.Y, q.W);
            else
                return new Assimp.Quaternion(q.X, q.Y, q.Z, q.W);
        }

        public static System.Numerics.Quaternion ToNumerics(this Assimp.Quaternion q, bool traspose = true)
        {
            if (traspose)
                return new System.Numerics.Quaternion(-q.X, q.Z, q.Y, q.W);
            else
                return new System.Numerics.Quaternion(q.X, q.Y, q.Z, q.W);
        }


        public static System.Numerics.Vector3 ToEulerAngles(this System.Numerics.Quaternion q)
        {
            // roll (x-axis rotation)
            double sinr_cosp = +2.0 * (q.W * q.X + q.Y * q.Z);
            double cosr_cosp = +1.0 - 2.0 * (q.X * q.X + q.Y * q.Y);
            double roll = Math.Atan2(sinr_cosp, cosr_cosp);
            // pitch (y-axis rotation)
            double sinp = +2.0 * (q.W * q.Y - q.Z * q.X);
            double pitch;
            if (Math.Abs(sinp) >= 1)
                pitch = Math.CopySign(Math.PI / 2, sinp); // use 90 degrees if out of range
            else
                pitch = Math.Asin(sinp);
            // yaw (z-axis rotation)
            double siny_cosp = +2.0 * (q.W * q.Z + q.X * q.Y);
            double cosy_cosp = +1.0 - 2.0 * (q.Y * q.Y + q.Z * q.Z);
            double yaw = Math.Atan2(siny_cosp, cosy_cosp);
            return new System.Numerics.Vector3((float)roll, (float)pitch, (float)yaw);
        }

        public static System.Numerics.Quaternion ToQuaternion(this System.Numerics.Vector3 v)
        {
            // Abbreviations for the various angular functions
            double cy = Math.Cos(v.Z * 0.5);
            double sy = Math.Sin(v.Z * 0.5);
            double cp = Math.Cos(v.Y * 0.5);
            double sp = Math.Sin(v.Y * 0.5);
            double cr = Math.Cos(v.X * 0.5);
            double sr = Math.Sin(v.X * 0.5);
            System.Numerics.Quaternion q = new System.Numerics.Quaternion();
            q.W = (float)(cy * cp * cr + sy * sp * sr);
            q.X = (float)(cy * cp * sr - sy * sp * cr);
            q.Y = (float)(sy * cp * sr + cy * sp * cr);
            q.Z = (float)(sy * cp * cr - cy * sp * sr);
            return q;
        }   

       

         

    }

}
