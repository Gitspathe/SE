using System.Numerics;
using Microsoft.Xna.Framework;

namespace SE.Core.Extensions
{

    public static class MonoGameMatrixExtensions
    {

        public static Matrix ToMonoGameMatrix(this Matrix4x4 tM)
        {
            return new Matrix(tM.M11, tM.M12, tM.M13, tM.M14, tM.M21, tM.M22, tM.M23, tM.M24, tM.M31, tM.M32, tM.M33, tM.M34, tM.M41, tM.M42, tM.M43, tM.M44);
        }

        public static Matrix? ToMonoGameMatrix(this Matrix4x4? mat)
        {
            if (!mat.HasValue) 
                return null;

            Matrix4x4 tM = mat.Value;
            return new Matrix(tM.M11, tM.M12, tM.M13, tM.M14, tM.M21, tM.M22, tM.M23, tM.M24, tM.M31, tM.M32, tM.M33, tM.M34, tM.M41, tM.M42, tM.M43, tM.M44);
        }

    }

}