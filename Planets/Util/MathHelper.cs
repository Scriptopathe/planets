using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX;
using SlimDX.D3DCompiler;
using SlimDX.Direct3D11;
using SlimDX.DXGI;
using SlimDX.Windows;
using Device = SlimDX.Direct3D11.Device;
using Resource = SlimDX.Direct3D11.Resource;
using Buffer = SlimDX.Direct3D11.Buffer;
namespace SimpleTriangle.Util
{
    /// <summary>
    /// Contient des fonctions d'aide mathématiques.
    /// </summary>
    public static class MathHelper
    {

        /// <summary>
        /// Multiplie 2 vecteurs composante par composante.
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static Vector3 PerComponentMultiply(Vector3 v1, Vector3 v2)
        {
            return new Vector3(v1.X * v2.X, v1.Y * v2.Y, v1.Z * v2.Z);
        }

        /// <summary>
        /// Retourne un vecteur2D comprenant les composantes X et Y du vecteur 3D passé en paramètre.
        /// </summary>
        /// <param name="vector3D"></param>
        /// <returns></returns>
        public static Vector2 ReduceXY(Vector3 vector3D)
        {
            return new Vector2(vector3D.X, vector3D.Y);
        }
        /// <summary>
        /// Retourne un vecteur3D comprenant les composantes X et Y et Z du vecteur 4D passé en paramètre.
        /// </summary>
        /// <param name="vector4D"></param>
        /// <returns></returns>
        public static Vector3 ReduceXYZ(Vector4 vector4D)
        {
            return new Vector3(vector4D.X, vector4D.Y, vector4D.Z);
        }
        /// <summary>
        /// Transforme une bounding box à l'aide de la matrice passée en paramètre.
        /// </summary>
        /// <param name="world"></param>
        /// <param name="bb"></param>
        /// <returns></returns>
        public static BoundingBox Transform(Matrix world, BoundingBox bb)
        {
            return BoundingBox.FromPoints(TransformAll(world, bb.GetCorners()));
        }

        /// <summary>
        /// Retourne un vecteur dont les composantes sont celles ayant la plus grande valeur parmi chaque composante 
        /// des vecteurs passés en paramètres.
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static Vector3 Max(Vector3 v1, Vector3 v2)
        {
            return new Vector3(Math.Max(v1.X, v2.X), Math.Max(v1.Y, v2.Y), Math.Max(v1.Z, v2.Z));
        }
        /// <summary>
        /// Retourne un vecteur dont les composantes sont celles ayant la plus faible valeur parmi chaque composante 
        /// des vecteurs passés en paramètres.
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static Vector3 Min(Vector3 v1, Vector3 v2)
        {
            return new Vector3(Math.Min(v1.X, v2.X), Math.Min(v1.Y, v2.Y), Math.Min(v1.Z, v2.Z));
        }
        /// <summary>
        /// Transforme tous les vecteurs d'un tableau.
        /// </summary>
        /// <param name="world"></param>
        /// <param name="vectors"></param>
        /// <returns></returns>
        public static Vector3[] TransformAll(Matrix world, Vector3[] vectors)
        {
            Vector3[] newVectors = new Vector3[vectors.Length];
            for(int i = 0; i < vectors.Length; i++)
            {
                Vector4 tr = Vector3.Transform(vectors[i], world);
                newVectors[i] = new Vector3(tr.X * tr.W, tr.Y * tr.W, tr.Z * tr.W);
            }
            return newVectors;
        }
    }
}
