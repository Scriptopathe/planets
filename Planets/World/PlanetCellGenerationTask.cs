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
using System.Threading;
using NoiseParameters = SimpleTriangle.Noise.NoiseMapGenerator.NoiseParameters;
namespace SimpleTriangle.World
{
    /// <summary>
    /// Représente la tâche chargée de générer une cellule de la planète.
    /// </summary>
    public class PlanetCellGenerationTask
    {
        #region Structs
        public class Rsc
        {
            public Buffer VertexBuffer;
            public BoundingBox Box;
            public Generation.ChunkAltitude Altitude;
            public void Dispose()
            {
                VertexBuffer.Dispose();
            }
        }
        #endregion

        #region Static

        #endregion

        #region Variables
        volatile bool m_isRessourceReady;
        volatile Rsc m_ressource;
        volatile bool m_isComputing;
        volatile bool m_isAborted;

        Vector3 m_planetPosition;
        float m_planetRadius;
        int m_gridSize;
        Thread m_currentThread;
        #endregion

        #region Properties

        /// <summary>
        /// Obtient ou définit une valeur indiquant si la ressource est prête à être utilisée.
        /// </summary>
        public bool IsRessourceReady
        {
            get { return m_isRessourceReady; }
            private set { m_isRessourceReady = value; }
        }

        /// <summary>
        /// Obtient la valeur de la ressource calculée.
        /// </summary>
        public Rsc Ressource
        {
            get
            {
                if (IsRessourceReady)
                    return m_ressource;
                else
                    // TODO : résoudre le bug qui se produit quand une cellule mère tente de supprimer une cellule fille encore entrain de créer ses buffers
                    throw new InvalidOperationException(); 
            }
            private set
            {
                m_ressource = value;
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Crée une nouvelle instance de PlanetCellGenerationTask.
        /// </summary>
        public PlanetCellGenerationTask()
        {
        }
        /// <summary>
        /// Lance le calcul de génération d'une grille de planète.
        /// </summary>
        /// <param name="planetPosition"></param>
        /// <param name="planetRadius"></param>
        /// <param name="gridSize"></param>
        /// <param name="noiseLow"></param>
        /// <param name="noiseHigh"></param>
        /// <param name="repartitionNoise"></param>
        /// <param name="world"></param>
        /// <param name="initialGridPos"></param>
        /// <param name="gridLevelScale"></param>
        public void RunCalculation(Vector3 planetPosition, float planetRadius, int gridSize, Noise.NoiseMapGenerator.NoiseParameters noiseLow, Noise.NoiseMapGenerator.NoiseParameters noiseHigh, 
            Noise.NoiseMapGenerator.NoiseParameters repartitionNoise, Matrix world, Vector2 initialGridPos, float gridLevelScale)
        {
            if (m_isComputing)
                throw new NotImplementedException();
            
            m_planetPosition = planetPosition;
            m_planetRadius = planetRadius;
            m_gridSize = gridSize;

            IsRessourceReady = false;
            System.Threading.Thread.CurrentThread.Priority = ThreadPriority.Highest;
            Thread thread = new Thread(() =>
            {
                m_isComputing = true;
                System.Threading.Thread.CurrentThread.Priority = ThreadPriority.Normal;
                var low = noiseLow.CreateNoise();
                var high = noiseHigh.CreateNoise();
                var rep = repartitionNoise.CreateNoise();
                BoundingBox aabb;
                Generation.ChunkAltitude altitude;
                Buffer vertexBuffer;
                Generation.ModelGenerator.GeneratePlanet(planetPosition,
                    planetRadius,
                    gridSize, 
                    initialGridPos,
                    gridLevelScale,
                    low, high, rep, // bruits
                    out vertexBuffer,
                    out aabb,
                    out altitude);

                // Si on abort la tâche.
                Ressource = new Rsc()
                {
                    VertexBuffer = vertexBuffer,
                    Box = aabb,
                    Altitude = altitude
                };
                IsRessourceReady = true;
                m_isComputing = false;
                m_currentThread = null;

                if(m_isAborted)
                {
                    Ressource.Dispose();
                    IsRessourceReady = false;
                }
            });
            m_currentThread = thread;
            Scene.Instance.ThreadPool.AddThread(thread);
        }

        /// <summary>
        /// Supprime les ressources allouées à cette tâche.
        /// </summary>
        public void Dispose()
        {
            if(IsRessourceReady)
            {
                Ressource.Dispose();
                IsRessourceReady = false;
            }
        }

        /// <summary>
        /// Supprime cette tâche.
        /// </summary>
        public void Abort()
        {
            if (!m_isComputing)
                Scene.Instance.ThreadPool.RemoveThread(m_currentThread);
            m_isAborted = true;
        }

        #endregion
    }
}
