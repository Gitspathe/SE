using System.Numerics;
using Microsoft.Xna.Framework;

namespace SE.Core.Extensions
{

    public static class MatrixExtensions
    {

        public static Matrix4x4 ToNumericsMatrix(this Matrix tM)
        {
            return new Matrix4x4(tM.M11, tM.M12, tM.M13, tM.M14, tM.M21, tM.M22, tM.M23, tM.M24, tM.M31, tM.M32, tM.M33, tM.M34, tM.M41, tM.M42, tM.M43, tM.M44);
        }

        public static Matrix4x4? ToNumericsMatrix(this Matrix? mat)
        {
            if (!mat.HasValue) 
                return null;

            Matrix tM = mat.Value;
            return new Matrix4x4(tM.M11, tM.M12, tM.M13, tM.M14, tM.M21, tM.M22, tM.M23, tM.M24, tM.M31, tM.M32, tM.M33, tM.M34, tM.M41, tM.M42, tM.M43, tM.M44);
        }

    }

}
