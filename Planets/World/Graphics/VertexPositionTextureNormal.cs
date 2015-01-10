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
namespace SimpleTriangle.World.Graphics
{
    public static class VertexPositionTextureNormal
    {
        /// <summary>
        /// Type de vertex prenant en charge des informations de Postion, Normale et Texture.
        /// </summary>
        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 1)]
        public struct Vertex
        {
            /// <summary>
            /// Position 3D du vertex.
            /// </summary>
            public Vector4 Position;    // 0
            /// <summary>
            /// Normale du vertex.
            /// </summary>
            public Vector4 Normal;      // 16
            /// <summary>
            /// Position de texture du vertex.
            /// </summary>
            public Vector2 Texture;     // 32
            /// <summary>
            /// Vecteur normalisé entre le centre de la planète et le vertex.
            /// (représente la normale selon laquelle on déplace les vertex pour
            /// former le terrain)
            /// </summary>
            public Vector4 SphereNormal; // 40
            /// <summary>
            /// Position du vertex sur la sphère, sans le déplacement de terrain.
            /// </summary>
            public Vector4 SpherePosition; // 56
            public float TextureId;     // 72
            public float Altitude;      // 76
            
            public const int Stride = 80;
            
        }
        /// <summary>
        /// Wrapper vers un vertex pouvant contenir des données additionnelles utiles pour le calcul mais qui ne doivent pas apparaître dans 
        /// la structure finale à exporter.
        /// </summary>
        public class VertexWrapper
        {
            public Vertex Vertex;

            public Vector3 SphereNormal;
            public VertexWrapper()
            {
                Vertex = new Vertex();
            }
        }
        /// <summary>
        /// Elements du layout.
        /// </summary>
        public static InputElement[] LayoutElements = new InputElement[]
        {
            new InputElement("POSITION", 0, Format.R32G32B32A32_Float, 
                0, 0, InputClassification.PerVertexData, 0),
            new InputElement("NORMAL", 0, Format.R32G32B32A32_Float, 
                16, 0, InputClassification.PerVertexData, 0),
            new InputElement("TEXTURE", 0, Format.R32G32_Float, 
                32, 0, InputClassification.PerVertexData, 0),
            new InputElement("PNORMAL", 0, Format.R32G32B32A32_Float, 
                40, 0, InputClassification.PerVertexData, 0),
            new InputElement("PPOSITION", 0, Format.R32G32B32A32_Float, 
                56, 0, InputClassification.PerVertexData, 0),
            new InputElement("TEXTURE_ID", 0, Format.R32_Float, 
                72, 0, InputClassification.PerVertexData, 0),
            new InputElement("ALTITUDE", 0, Format.R32_Float, 
                76, 0, InputClassification.PerVertexData, 0)
        };
    }

}
