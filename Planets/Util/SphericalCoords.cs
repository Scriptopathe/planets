using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX;
namespace SimpleTriangle.Util
{
    /// <summary>
    /// Représente des coordonnées sphériques.
    /// </summary>
    public struct SphericalCoords
    {
        float m_phi;
        float m_theta;
        float m_radius;
        /// <summary>
        /// Rayon.
        /// </summary>
        public float Radius { get { return m_radius; } set { m_radius = value; } }
        /// <summary>
        /// Longitude (theta)
        /// </summary>
        public float Theta { get { return m_theta; } set { m_theta = value; } }
        /// <summary>
        /// Lattitude. (phi)
        /// </summary>
        public float Phi { get { return m_phi; } set { m_phi = value; } }

        /// <summary>
        /// Crée un point de coordonnées données.
        /// </summary>
        public SphericalCoords(float latt, float lg, float radius)
        {
            m_phi = latt;
            m_theta = lg;
            m_radius = radius;
        }


        /// <summary>
        /// Retourne la position cartésienne correspondant à la position en coordonnées sphériques actuelle.
        /// </summary>
        /// <returns></returns>
        public Vector3 ToCartesian()
        {
            float r = (float)Math.Sin(Theta) * Radius;
            float xSph = (float)Math.Cos(Phi) * r;
            float ySph = (float)Math.Sin(Phi) * r;
            float zSph = (float)Math.Cos(Theta) * Radius;
            return new Vector3(xSph, ySph, zSph);
        }

        /// <summary>
        /// Retourne la position cartésienne correspondant à la position en coordonnées sphériques passée en paramètre.
        /// </summary>
        /// <returns></returns>
        public static Vector3 ToCartesian(float theta, float phi, float radius)
        {
            float r = (float)Math.Sin(theta) * radius;
            float xSph = (float)Math.Cos(phi) * r;
            float ySph = (float)Math.Sin(phi) * r;
            float zSph = (float)Math.Cos(theta) * radius;
            return new Vector3(xSph, ySph, zSph);
        }

        /// <summary>
        /// Retourne la position sphérique associée à la position en coordonnées cartésiennes donnée.
        /// </summary>
        /// <returns></returns>
        public static SphericalCoords FromCartesian(Vector3 vec)
        {
            SphericalCoords coords = new SphericalCoords();
            coords.Radius = (float)Math.Sqrt(vec.X * vec.X + vec.Y * vec.Y + vec.Z * vec.Z);
            coords.Theta = To0_2PI_Range((float)Math.Acos(vec.Z / coords.Radius));
            coords.Phi = To0_2PI_Range((float)Math.Atan2(vec.Y, vec.X));
            return coords;

        }

        static float To0_2PI_Range(float angle)
        {
            if (angle < 0)
                angle += (float)Math.PI * 2;
            return angle;
        }
    }
}
