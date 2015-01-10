using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX;
using System.Runtime.InteropServices;
namespace SimpleTriangle.World.Graphics
{
    /// <summary>
    /// Représente un matériau.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct MaterialStruct
    {
        public Color4 Ambient;
        public Color4 Diffuse;
        public Color4 Specular;
        public Color4 Reflect;
    }

    public class Material
    {
        MaterialStruct m_struct;
        DataStream m_structToBytes;

        /// <summary>
        /// Crée un nouveau matériau avec les paramètres passés en argument.
        /// </summary>
        public Material(Color4 ambiant, Color4 diffuse, Color4 speculat, Color4 reflect)
        {
            m_struct = new MaterialStruct()
            {
                Ambient = ambiant,
                Diffuse = diffuse,
                Specular = speculat,
                Reflect = reflect
            };
           m_structToBytes = new DataStream(Util.MarshalHelper.GetArray(m_struct), false, false);
        }

        /// <summary>
        /// Retourne la représentation binaire de ce matériau.
        /// </summary>
        public DataStream Bytes
        {
            get { return m_structToBytes; }
        }

        /// <summary>
        /// Obtient les données constituant le matériau.
        /// </summary>
        public MaterialStruct Data
        {
            get { return m_struct; }
        }
    }

    /// <summary>
    /// Représente une lumière directionnelle.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct DirectionalLight
    {
        public Color4 Ambient;
        public Color4 Diffuse;
        public Color4 Specular;
        public Vector3 Direction;
        public float Pad;
    }
    /// <summary>
    /// Représente une source de lumière ponctuelle.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct PointLight
    {
        public Color4 Ambient;
        public Color4 Diffuse;
        public Color4 Specular;
        public Vector3 Position;
        public float Range;
        public Vector3 Attenuation;
        public float Pad;

        /*public PointLight Copy(PointLight l)
        {
            
        }*/
    }

    /// <summary>
    /// Représente une source de lumière en forme de spot.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct SpotLight
    {
        public Color4 Ambient;
        public Color4 Diffuse;
        public Color4 Specular;
        public Vector3 Position;
        public float Range;
        public Vector3 Direction;
        public float Spot;
        public Vector3 Attenuation;
        public float Pad;
    }
}
