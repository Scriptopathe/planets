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

namespace SimpleTriangle.World
{
    /// <summary>
    /// Représente une cellule du QuadTree formant les parcelles de terrains de la planète.
    /// </summary>
    public class TerrainRessource : QuadTreeCellRessource
    {
        #region Constants
        /// <summary>
        /// Résolution de la grille.
        /// </summary>
        public const int GridResolution = 64;
        public const FillMode RasterizerFillMode = FillMode.Solid;
        #endregion

        #region Variables
        /// <summary>
        /// Tâche de génération de la planète.
        /// </summary>
        PlanetCellGenerationTask m_genTask;
        
        #region Variables graphiques
        Graphics.Material m_material;
        #endregion

        #endregion

        #region Properties

        #endregion

        #region Methods
        /// <summary>
        /// Crée une nouvelle instance de PlanetCell.
        /// </summary>
        public TerrainRessource(QuadTreeCell parent) : base(parent)
        {
            m_genTask = new PlanetCellGenerationTask();
            InitializeEffect();
        }

        /// <summary>
        /// Initialise les paramètres du shader.
        /// </summary>
        void InitializeEffect()
        {
            m_material = new Graphics.Material(
                new Color4(1.0f, 0.48f, 0.48f, 0.48f), // ambient
                new Color4(1.0f, 0.88f, 0.88f, 0.88f), // diffuse
                new Color4(1.0f, 0.0f, 0.0f, 0.0f),
                //new Color4(2.0f, 0.2f, 0.2f, 0.2f), // specular
                new Color4(0, 0, 0, 0));
            
        }

        #region Expansion Mechanisms
 
        #endregion


        #region Buffer creation
        /// <summary>
        /// Crée les index et vertex buffers.
        /// </summary>
        public override void StartRessourceGeneration()
        {
            if (m_genTask.IsRessourceReady)
                return;
            // Heightmap
            // Initialisation des paramètres du bruit
            Noise.NoiseMapGenerator.NoiseParameters m_repartitionNoise;
            Noise.NoiseMapGenerator.NoiseParameters m_noiseHigh;
            Noise.NoiseMapGenerator.NoiseParameters m_noiseLow;

            m_repartitionNoise = new Noise.NoiseMapGenerator.NoiseParameters();
            m_noiseHigh = new Noise.NoiseMapGenerator.NoiseParameters();
            m_noiseLow = new Noise.NoiseMapGenerator.NoiseParameters();

            m_noiseHigh.NoiseType = Noise.NoiseMapGenerator.NoiseParameters.RIDGED_ID;
            m_noiseHigh.OctaveCount = 8;
            m_noiseHigh.Lacunarity = 1.8f;
            m_noiseHigh.Frequency = 6;
            m_noiseHigh.Persistence = 0.99f;
            m_noiseHigh.Seed = 1073741824;

            m_repartitionNoise.NoiseType = Noise.NoiseMapGenerator.NoiseParameters.RIDGED_ID;
            m_repartitionNoise.OctaveCount = 4;
            m_repartitionNoise.Persistence = 0.91f;
            m_repartitionNoise.Frequency = 2;
            m_repartitionNoise.Lacunarity = 3.6f;
            m_repartitionNoise.Seed = 1254546457;
            m_repartitionNoise.NoiseStart = Parent.GridPosition;
            m_repartitionNoise.NoiseEnd = Parent.GridPosition + new Vector2(1, 1) * Parent.Scale;

            m_noiseLow.NoiseType = Noise.NoiseMapGenerator.NoiseParameters.PERLIN_ID;
            m_noiseLow.Frequency = 4;
            m_noiseLow.Lacunarity = 1.4f;
            m_noiseLow.Persistence = 0.237f;
            m_noiseLow.OctaveCount = 2;
            m_noiseLow.Seed = 1073741824;

            m_genTask.RunCalculation(Parent.PlanetPosition, Parent.PlanetRadius, GridResolution, m_noiseLow, m_noiseHigh, m_repartitionNoise, Parent.World, Parent.GridPosition, Parent.Scale);
        }

        #endregion

        #region Update / Draw
        /// <summary>
        /// Annule la génération de la ressource.
        /// </summary>
        public override void AbortGeneration()
        {
            m_genTask.Abort();
        }
        /// <summary>
        /// Mise à jour de la cellule.
        /// </summary>
        public override void Update()
        {

        }
        /// <summary>
        /// Indique si la génération du terrain est terminée.
        /// </summary>
        /// <returns></returns>
        public override bool IsRessourceReady
        {
            get { return m_genTask.IsRessourceReady; }
        }
        /// <summary>
        /// Crée la ressource fille correspondant à la cellule mère passée en argument.
        /// </summary>
        public override QuadTreeCellRessource Subdivide(QuadTreeCell parentCells)
        {
            return new TerrainRessource(parentCells);
        }
        /// <summary>
        /// Dessine cette cellule.
        /// </summary>
        public override void Draw()
        {
            if (!m_genTask.IsRessourceReady)
                throw new Exception("Unable to draw this ressource when it is not ready !");

            // Dessine la cellule.
            var device = Scene.GetGraphicsDevice().ImmediateContext;
            device.InputAssembler.SetIndexBuffer(Generation.ModelGenerator.GetIndexBuffer(GridResolution), Format.R32_UInt, 0);
            device.InputAssembler.PrimitiveTopology = (PrimitiveTopology.TriangleList);
            device.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(m_genTask.Ressource.VertexBuffer, Graphics.VertexPositionTextureNormal.Vertex.Stride, 0));

            RasterizerStateDescription rsd = new RasterizerStateDescription()
            {
                CullMode = CullMode.Back,
                DepthBias = 0,
                DepthBiasClamp = 0.0f,
                FillMode = RasterizerFillMode,
                IsAntialiasedLineEnabled = true,
                IsDepthClipEnabled = true,
                IsFrontCounterclockwise = false,
                IsMultisampleEnabled = false,
                IsScissorEnabled = false,
                SlopeScaledDepthBias = 0.0f
            };
            RasterizerState rs = RasterizerState.FromDescription(Scene.GetGraphicsDevice(), rsd);
            device.Rasterizer.State = rs;

            // Variables
            Graphics.BasicEffect effect = Scene.GetGraphicsEngine().BasicEffect;
            effect.Apply(Matrix.Identity, Scene.Instance.Camera.View, Scene.Instance.Camera.Projection, m_material);
            device.DrawIndexed(Generation.ModelGenerator.GetIndexBuffer(GridResolution).Description.SizeInBytes / sizeof(int), 0, 0);
            
        }
        #endregion
        
        
        #region Dispose
        /// <summary>
        /// Supprime les ressources allouées par cette cellule.
        /// </summary>
        public override void Dispose()
        {
            DisposeBuffers();
        }
        /// <summary>
        /// Supprime les buffers de cette cellule.
        /// </summary>
        public void DisposeBuffers()
        {
            if (IsRessourceReady)
                m_genTask.Dispose();
        }
        #endregion

        #region Utils
        public override BoundingBox GetAABB()
        {
            return m_genTask.Ressource.Box;
        }

        
        #endregion

        #endregion
    }
}
