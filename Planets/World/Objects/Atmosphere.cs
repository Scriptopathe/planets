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

namespace SimpleTriangle.World.Objects
{
    /// <summary>
    /// Représente une cellule du QuadTree formant une parcelle d'atmosphere.
    /// </summary>
    public class Atmosphere
    {
        #region Constants
        /// <summary>
        /// Résolution de la grille.
        /// </summary>
        public const int GridResolution = 256;
        #endregion

        #region Variables        
        #region Variables graphiques
        Graphics.Material m_material;
        SlimDX.Direct3D11.Buffer m_vBuffer;
        #endregion

        #endregion

        #region Properties

        #endregion

        #region Methods
        /// <summary>
        /// Crée une nouvelle instance de PlanetCell.
        /// </summary>
        public Atmosphere()
        {
            InitializeEffect();
            StartRessourceGeneration();
        }

        /// <summary>
        /// Initialise les paramètres du shader.
        /// </summary>
        void InitializeEffect()
        {
            m_material = new Graphics.Material(
                new Color4(1.0f, 0.48f, 0.48f, 0.48f), // ambient
                new Color4(1.0f, 0.88f, 0.88f, 0.88f), // diffuse
                new Color4(4.0f, 0.8f, 0.8f, 0.8f),
                new Color4(0, 0, 0, 0));
            
        }

        #region Buffer creation
        /// <summary>
        /// Crée les index et vertex buffers.
        /// </summary>
        public void StartRessourceGeneration()
        {
            BoundingBox aabb;
            Generation.ChunkAltitude alt;
            Generation.ModelGenerator.GeneratePlanet(Vector3.Zero, Planet.AtmosphereRadius, GridResolution, Vector2.Zero, 1.0f,
                new Noise.WhiteNoise(), new Noise.WhiteNoise(), new Noise.WhiteNoise(), out m_vBuffer, out aabb, out alt);
        }

        #endregion

        #region Update / Draw
        /// <summary>
        /// Mise à jour de la cellule.
        /// </summary>
        public void Update()
        {

        }

        /// <summary>
        /// Dessine cette cellule.
        /// </summary>
        public void Draw()
        {

            // Dessine la cellule.
            var device = Scene.GetGraphicsDevice().ImmediateContext;
            device.InputAssembler.SetIndexBuffer(Generation.ModelGenerator.GetIndexBuffer(GridResolution), Format.R32_UInt, 0);
            device.InputAssembler.PrimitiveTopology = (PrimitiveTopology.TriangleList);
            device.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(m_vBuffer, Graphics.VertexPositionTextureNormal.Vertex.Stride, 0));
            RasterizerStateDescription rsd = new RasterizerStateDescription()
            {
                CullMode = CullMode.Front,
                DepthBias = 0,
                DepthBiasClamp = 0.0f,
                FillMode = FillMode.Solid,
                IsAntialiasedLineEnabled = true,
                IsDepthClipEnabled = true,
                IsFrontCounterclockwise = false,
                IsMultisampleEnabled = false,
                IsScissorEnabled = false,
                SlopeScaledDepthBias = 0.0f
            };

            // Alpha blending
            var transDesc = new BlendStateDescription
            {
                AlphaToCoverageEnable = false,
                IndependentBlendEnable = false
            };
            transDesc.RenderTargets[0].BlendEnable = true;
            transDesc.RenderTargets[0].SourceBlend = BlendOption.SourceAlpha;
            transDesc.RenderTargets[0].DestinationBlend = BlendOption.InverseSourceAlpha;
            transDesc.RenderTargets[0].BlendOperation = BlendOperation.Add;
            transDesc.RenderTargets[0].SourceBlendAlpha = BlendOption.One;
            transDesc.RenderTargets[0].DestinationBlendAlpha = BlendOption.Zero;
            transDesc.RenderTargets[0].BlendOperationAlpha = BlendOperation.Add;
            transDesc.RenderTargets[0].RenderTargetWriteMask = ColorWriteMaskFlags.All;
            var bs = BlendState.FromDescription(device.Device, transDesc);
            device.OutputMerger.BlendState = bs;

            RasterizerState rs = RasterizerState.FromDescription(Scene.GetGraphicsDevice(), rsd);
            device.Rasterizer.State = rs;

            // Variables
            Graphics.AtmosphereEffect effect = Scene.GetGraphicsEngine().AtmosphereEffect;
            effect.Apply(Matrix.Identity, Scene.Instance.Camera.View, Scene.Instance.Camera.Projection, m_material);
            device.DrawIndexed(Generation.ModelGenerator.GetIndexBuffer(GridResolution).Description.SizeInBytes / sizeof(int), 0, 0);

            device.OutputMerger.BlendState = null;
            
        }
        #endregion

        
        #region Dispose
        /// <summary>
        /// Supprime les ressources allouées par cette cellule.
        /// </summary>
        public void Dispose()
        {
            DisposeBuffers();
        }
        /// <summary>
        /// Supprime les buffers de cette cellule.
        /// </summary>
        public void DisposeBuffers()
        {
            m_vBuffer.Dispose();
        }
        #endregion

        #endregion
    }
}
