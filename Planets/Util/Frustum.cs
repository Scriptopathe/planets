using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX.Direct3D11;
using SlimDX;
namespace SimpleTriangle.Util
{
    /// <summary>
    /// Représente le frustum de la caméra.
    /// </summary>
    public class Frustum
    {
        public enum ContainmentType
        {
            NoIntersection,
            Intersection,
            AllInside
        }
        private readonly Plane[] _frustum;
        /// <summary>
        /// Crée un frustrum à partir d'une matrice ViewProjection.
        /// </summary>
        /// <param name="vp"></param>
        public Frustum(Matrix vp)
        {
            _frustum = new[] {
                new Plane(vp.M14 + vp.M11, vp.M24 + vp.M21, vp.M34 + vp.M31, vp.M44 + vp.M41),
                new Plane(vp.M14 - vp.M11, vp.M24 - vp.M21, vp.M34 - vp.M31, vp.M44 - vp.M41),
                new Plane(vp.M14 - vp.M12, vp.M24 - vp.M22, vp.M34 - vp.M32, vp.M44 - vp.M42),
                new Plane(vp.M14 + vp.M12, vp.M24 + vp.M22, vp.M34 + vp.M32, vp.M44 + vp.M42),
                new Plane(vp.M13, vp.M23, vp.M33, vp.M43),
                new Plane(vp.M14 - vp.M13, vp.M24 - vp.M23, vp.M34 - vp.M33, vp.M44 - vp.M43)
            };
            foreach (var plane in _frustum)
            {
                plane.Normalize();
            }
        }
        
        /// <summary>
        /// Crée un nouveau frustrum à partir de la matrice view / projection.
        /// </summary>
        /// <param name="vp"></param>
        /// <returns></returns>
        public static Frustum FromViewProj(Matrix vp)
        {
            var ret = new Frustum(vp);
            return ret;
        }

        /// <summary>
        /// Retourne une valeur indiquant l'intersection entre ce frustum et
        /// la bounding box donnée.
        /// </summary>
        /// <param name="box"></param>
        /// <returns></returns>
        public ContainmentType Intersect(BoundingBox box)
        {
            var totalIn = 0;

            foreach (var plane in _frustum)
            {
                var intersection = Plane.Intersects(plane, box);
                if (intersection == PlaneIntersectionType.Back) return ContainmentType.NoIntersection;
                if (intersection == PlaneIntersectionType.Front)
                {
                    totalIn++;
                }
            }
            if (totalIn == 6)
            {
                return ContainmentType.AllInside;
            }
            return ContainmentType.Intersection;
        }
    }
}
