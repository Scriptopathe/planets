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
    public class QuadTreeCell
    {
        #region Constants
        /// <summary>
        /// Profondeur max d'une cellule dans le quad tree.
        /// </summary>
        public const int MaxDepth = 11; // 9
        public const float RETRACT_FACTOR = 1.3f;
        public const float DIVIDE_FACTOR = 1.1f;
        public const bool EXPAND_ALL = false;
        #endregion

        #region Variables
        /// <summary>
        /// Indique si la région actuelle est subdivisée en morceaux de taille inférieure ou non.
        /// </summary>
        bool m_isSubdivided;
        /// <summary>
        /// Cellules intérieures.
        /// Les données contenues dans ces cellules sont valides si m_isSubdivided vaut true.
        /// Sinon, elles doivent être supprimées.
        /// </summary>
        QuadTreeCell[] m_subCells;
        /// <summary>
        /// Obtient les ressources attachées à cette cellule.
        /// </summary>
        List<QuadTreeCellRessource> m_ressources;
        /// <summary>
        /// Facteur d'échelle de cette cellule.
        /// Ce facteur est divisé par deux à chaque étage du quadtree.
        /// </summary>
        float m_scale;

        /// <summary>
        /// Graine de la planète.
        /// </summary>
        int m_seed;

        /// <summary>
        /// Facteur d'échelle globale du quadtree.
        /// Il est passé à l'identique de cellule en cellule.
        /// </summary>
        Vector3 m_globalScale;

        /// <summary>
        /// Position du coin supérieur gauche de la cellule sur la grille unitaire.
        /// </summary>
        Vector2 m_gridPosition;

        /// <summary>
        /// Position du centre de la planète.
        /// </summary>
        Vector3 m_planetPosition;

        /// <summary>
        /// Profondeur de la cellule dans le quadtree.
        /// </summary>
        int m_depth;
        /// <summary>
        /// Indique si la cellule est en cours d'expansion (le thread de création des cellules filles est toujours en cours).
        /// </summary>
        bool m_isExpanding;
        /// <summary>
        /// Indique si la cellule est toujours en cours de rétractation.
        /// </summary>
        bool m_isRetracting;
        /// <summary>
        /// Rayon de la planète
        /// </summary>
        float m_planetRadius;
        #region Variables graphiques
        BoundingBox m_boundingBox;
        Matrix m_world;

        #endregion

        #endregion

        #region Properties
        /// <summary>
        /// Obtient une valeur indiquant si cette cellule est une feuille.
        /// </summary>
        public bool IsLeaf
        {
            get
            {
                return !m_isSubdivided;
            }
        }
        /// <summary>
        /// Obtient les cellules filles de la la cellule actuelle.
        /// </summary>
        public QuadTreeCell[] Children
        {
            get { return m_subCells; }
            private set { m_subCells = value; }
        }
        /// <summary>
        /// Matrice de transformation de la cellule.
        /// </summary>
        public Matrix World
        {
            get { return m_world; }
            set { m_world = value; }
        }
        /// <summary>
        /// Facteur d'échelle de cette cellule.
        /// Ce facteur est divisé par deux à chaque étage du quadtree.
        /// </summary>
        public float Scale
        {
            get { return m_scale; }
            set { m_scale = value; }
        }

        /// <summary>
        /// Position du centre de la planète dans laquelle est contenue cette cellule.
        /// </summary>
        public Vector3 PlanetPosition
        {
            get { return m_planetPosition; }
            set { m_planetPosition = value; }
        }

        /// <summary>
        /// Rayon de la planète.
        /// </summary>
        public float PlanetRadius
        {
            get { return m_planetRadius; }
            set { m_planetRadius = value; }
        }
        /// <summary>
        /// Graine de la planète.
        /// </summary>
        public int Seed
        {
            get { return m_seed; }
            set { m_seed = value; }
        }
        /// <summary>
        /// Profondeur de la cellule dans le quadtree.
        /// </summary>
        public int Depth
        {
            get { return m_depth; }
            set { m_depth = value; }
        }
        /// <summary>
        /// Position du coin supérieur gauche de la cellule sur la grille unitaire.
        /// </summary>
        public Vector2 GridPosition
        {
            get { return m_gridPosition; }
            set { m_gridPosition = value; }
        }

        /// <summary>
        /// Obtient la matrice de transformation subie par cette cellule.
        /// </summary>
        public Matrix Transform
        {
            get { return m_world; }
        }
        /// <summary>
        /// Retourne la box représentant les bords de cette cellule.
        /// </summary>
        public BoundingBox Bounds
        {
            get { return m_boundingBox; }
        }
        /// <summary>
        /// Indique si la cellule est prête à être affichée.
        /// </summary>
        public bool IsReady
        {
            get { return AllResssourcesReady(); }
        }
        /// <summary>
        /// Facteur d'échelle globale du quadtree.
        /// Il est passé à l'identique de cellule en cellule.
        /// </summary>
        public Vector3 GlobalScale
        {
            get { return m_globalScale; }
            set { m_globalScale = value; }
        }
        /// <summary>
        /// Représente les ressources contenues dans la cellule.
        /// </summary>
        public List<QuadTreeCellRessource> Ressources
        {
            get { return m_ressources; }
            set { m_ressources = value; }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Crée une nouvelle instance de PlanetCell.
        /// </summary>
        public QuadTreeCell(Vector3 planetPosition, Vector2 gridPosition, float scale, Vector3 globalScale, int mSeed, int depth, float radius)
        {
            m_subCells = new QuadTreeCell[4];
            m_gridPosition = gridPosition;
            m_globalScale = globalScale;
            m_planetPosition = planetPosition;
            m_scale = scale;
            m_seed = mSeed;
            m_world = ComputeTransform();
            m_depth = depth;
            m_boundingBox = new BoundingBox();
            m_planetRadius = radius;
            m_ressources = new List<QuadTreeCellRessource>();
        }

        /// <summary>
        /// Initialise les paramètres du shader.
        /// </summary>
        void InitializeEffect()
        {

            
        }
        #region Expansion Mechanisms
        /// <summary>
        /// Subdivise cette cellule en quatre autres cellules de taille inférieure et de même résolution.
        /// Supprime aussi les données d'affichage de cette cellule (vertex / index buffers).
        /// </summary>
        public void Expand()
        {
            CreateSubCells();
        }

        /// <summary>
        /// Initialise les cellules de l'étage inférieur du quadtree.
        /// </summary>
        void CreateSubCells()
        {
            float newScale = m_scale / 2;
            for(int i = 0; i < 4; i++)
            {
                int x = i % 2;
                int y = i / 2;
                Vector2 cellPos = new Vector2(m_gridPosition.X + x * newScale, m_gridPosition.Y + y * newScale);
                QuadTreeCell newCell = new QuadTreeCell(m_planetPosition, cellPos, newScale, m_globalScale, m_seed, m_depth + 1, m_planetRadius);
                foreach(QuadTreeCellRessource rsc in m_ressources)
                {
                    newCell.Ressources.Add(rsc.Subdivide(newCell));
                }
                newCell.InitializeRessources();
                m_subCells[i] = newCell;
            }
        }

        /// <summary>
        /// Supprime les cellules contenues dans cette cellule, et recrée les données d'affichage de celle-ci.
        /// </summary>
        public void Retract()
        {
            InitializeRessources();
            m_isRetracting = true;
        }

        /// <summary>
        /// Annule la génération des ressources dans cette cellule.
        /// </summary>
        public void AbortRessourceGeneration()
        {
            foreach(QuadTreeCellRessource rsc in m_ressources)
            {
                rsc.AbortGeneration();
            }
        }
        #endregion


        #region Buffer creation
        /// <summary>
        /// Crée les index et vertex buffers.
        /// </summary>
        public void InitializeRessources()
        {
            foreach(QuadTreeCellRessource rsc in m_ressources)
            {
                rsc.StartRessourceGeneration();
            }
        }
        /// <summary>
        /// Supprime les ressources contenues dans cette cellule.
        /// </summary>
        public void DisposeRessources()
        {
            foreach(QuadTreeCellRessource rsc in m_ressources)
            {
                rsc.Dispose();
            }
        }
        #endregion

        #region Update / Draw
        /// <summary>
        /// Calcule la distance du joueur à la cellule.
        /// TODO : à rendre mieux.
        /// </summary>
        /// <returns></returns>
        float ComputePlayerDistance()
        {
            
            BoundingBox bounds = Bounds;
            Vector2 position = new Vector2(Scene.Instance.Camera.Position.X, Scene.Instance.Camera.Position.Y);
            Vector2 center = new Vector2(bounds.Minimum.X + bounds.Maximum.X, bounds.Minimum.Y + bounds.Maximum.Y) / 2;
            float distance = Vector2.Distance(position, center);
            return distance;
        }
        /// <summary>
        /// Indique si cette cellule doit se rétracter.
        /// </summary>
        /// <returns></returns>
        bool ShouldRetract()
        {
            if (EXPAND_ALL || Depth < 3)
                return false;

            Generation.ChunkGeography chunk = GetGeography();
            Util.SphericalCoords playerPlanetCoords = Util.SphericalCoords.FromCartesian(Scene.Instance.Camera.Position - PlanetPosition);
            Vector2 player = new Vector2(playerPlanetCoords.Theta, playerPlanetCoords.Phi);

            float factor = 1.3f + Math.Max(0, 5 - m_depth) * 0.100f; ;
            bool thetaOK = player.X < chunk.ThetaMax * factor && player.X > chunk.ThetaMin / factor;
            bool phiOK = player.Y < chunk.PhiMax * factor && player.Y > chunk.PhiMin / factor;
            bool radiusOK = Math.Abs(playerPlanetCoords.Radius - PlanetRadius) < 1+(MaxDepth - m_depth)*2.0f;
            return !thetaOK || !phiOK || !radiusOK;
            /*float distance = ComputePlayerDistance();
            float diagLength = Vector2.Distance(Util.MathHelper.ReduceXY(Bounds.Minimum), Util.MathHelper.ReduceXY(Bounds.Maximum));*/
           // return distance > diagLength * RETRACT_FACTOR && !m_isRetracting && !m_isExpanding;
        }
        /// <summary>
        /// Indique si cette cellule doit se diviser.
        /// </summary>
        /// <returns></returns>
        bool ShouldDivide()
        {
            if (EXPAND_ALL || m_depth < 3)
                return m_depth < MaxDepth;

            // Condition nécessaire.
            bool depthOK = m_depth < MaxDepth;

            Generation.ChunkGeography chunk = GetGeography();
            Util.SphericalCoords playerPlanetCoords = Util.SphericalCoords.FromCartesian(Scene.Instance.Camera.Position - PlanetPosition);
            Vector2 player = new Vector2(playerPlanetCoords.Theta, playerPlanetCoords.Phi);

            float factor = 1.005f + Math.Max(0, 8 - m_depth)*0.007f;
            bool thetaOK = player.X < chunk.ThetaMax * factor && player.X > chunk.ThetaMin / factor;
            bool phiOK = player.Y < chunk.PhiMax * factor && player.Y > chunk.PhiMin / factor;
            bool radiusOK = Math.Abs(playerPlanetCoords.Radius - PlanetRadius) < 1+(MaxDepth -  m_depth)*1.5f; //playerPlanetCoords.Radius > PlanetRadius * 0.9f && playerPlanetCoords.Radius < PlanetRadius * 5;

            return depthOK && (thetaOK && phiOK) && radiusOK;

        }
        /// <summary>
        /// Mise à jour de la cellule.
        /// </summary>
        public void Update(GameTime time)
        {
            
            if (AllResssourcesReady())
            {
                m_boundingBox = ComputeBoundingBox(); // TODO : ne faire ça qu'une fois.
                // Si la cellule n'est pas subdivisée et qu'on y rentre, alors on la divise.
                if (ShouldDivide() && !m_isExpanding && !m_isSubdivided)
                {
                    Expand();
                    m_isExpanding = true;
                }

                // Gère la transition expansion.
                if (m_isExpanding)
                {
                    bool childrenReady = true;
                    foreach (QuadTreeCell cell in m_subCells)
                    {
                        if (!cell.IsReady)
                        {
                            childrenReady = false;
                            break;
                        }
                    }

                    // Fin de la division.
                    if (childrenReady)
                    {
                        DisposeRessources();
                        m_isSubdivided = true;
                        m_isExpanding = false;
                    }
                }
            }
            else
            {
                // Commencement de la rétractation.
                if (m_isSubdivided && ChildrenAreLeaves() && !m_isRetracting && !m_isExpanding)
                {
                    // Si la cellule est divisée et qu'on s'éloigne trop : on la rétracte.
                    // Si on a commencé à se rétracter, ShouldRetract() vaudra false.
                    if (ShouldRetract())
                    {
                        Retract();
                        m_isRetracting = true;
                    }
                }





            }
            // Si en cours d'expansion (non fini)
            if(m_isExpanding && !ShouldDivide() && ChildrenAreLeaves())
            {
                foreach(QuadTreeCell cell in m_subCells)
                {
                    cell.AbortRessourceGeneration();
                    m_isExpanding = false;
                    m_isSubdivided = false;
                }
                return;
            }
            // Si enfants : 
            if (m_isSubdivided || m_isExpanding)
            {
                foreach (QuadTreeCell cell in m_subCells)
                {
                    cell.Update(time);
                }
            }
            // Gère la transition de rétractation.
            if (m_isSubdivided && m_isRetracting)
            {
                if (AllResssourcesReady())
                {
                    m_isSubdivided = false;
                    foreach (QuadTreeCell cell in m_subCells)
                    {
                        cell.Dispose();
                    }
                    m_isRetracting = false;
                }
            }



        }
        /// <summary>
        /// Dessine cette cellule.
        /// </summary>
        public void Draw()
        {
            if(m_isSubdivided)
            {
                foreach (QuadTreeCell cell in m_subCells)
                {
                    cell.Draw();
                }
            }
            else if(AllResssourcesReady())
            {
                /* Dessin des cellules filles */
                foreach(QuadTreeCellRessource rsc in m_ressources)
                {
                    if(Scene.Instance.Camera.Frustum.Intersect(rsc.GetAABB()) != Util.Frustum.ContainmentType.NoIntersection)
                        rsc.Draw();
                }

                /*Debug.Renderers.BoundingBoxRenderer.Render(Bounds,
                    Scene.GetGraphicsDevice(),
                    Matrix.Identity,
                    Scene.Instance.Camera.View,
                    Scene.Instance.Camera.Projection,
                    new Color4(255, 0, 0, 255)); // */
            }


        }
        #endregion

        
        #region Dispose
        /// <summary>
        /// Supprime les ressources allouées par cette cellule.
        /// </summary>
        public void Dispose()
        {
            DisposeRessources();
        }
        #endregion

        #region Utils
        /// <summary>
        /// Calcule la bouding box contenant cette cellule.
        /// </summary>
        /// <returns></returns>
        BoundingBox ComputeBoundingBox()
        {
            BoundingBox box = new BoundingBox(new Vector3(float.MaxValue), new Vector3(float.MinValue));
            foreach(QuadTreeCellRessource rsc in m_ressources)
            {
                BoundingBox b = rsc.GetAABB();
                box.Minimum = Util.MathHelper.Min(b.Minimum, box.Minimum);
                box.Maximum = Util.MathHelper.Max(b.Maximum, box.Maximum);
            }
            return box;
        }
        /// <summary>
        /// Indique si toutes les ressources contenues dans cette cellule sont prêtes à être affichées.
        /// </summary>
        /// <returns></returns>
        bool AllResssourcesReady()
        {
            foreach(QuadTreeCellRessource rsc in m_ressources)
            {
                if (!rsc.IsRessourceReady)
                    return false;
            }
            return true;
        }
        /// <summary>
        /// Calcule et retourne la matrice de transformation subie par cette cellule.
        /// </summary>
        /// <returns></returns>
        public Matrix ComputeTransform()
        {
            Matrix scaling = Matrix.Scaling(m_globalScale.X * m_scale, m_globalScale.Y * m_scale, m_globalScale.Z);
            Vector3 position = new Vector3((m_gridPosition - new Vector2(0.5f, 0.5f)), 0);
            position.X *= m_globalScale.X; position.Y *= m_globalScale.Y; position.Z *= m_globalScale.Z;
            position += m_planetPosition;

            Matrix world = scaling * Matrix.Translation(position);
            return world;
        }

        /// <summary>
        /// Retourne vrai si tous les enfants de cette cellule sont des feuilles.
        /// </summary>
        /// <returns></returns>
        public bool ChildrenAreLeaves()
        {
            foreach(QuadTreeCell child in m_subCells)
            {
                if (child != null && !child.IsLeaf)
                    return false;
            }
            return true;
        }
        /// <summary>
        /// Retourne des informations concernant la géographie de la cellule.
        /// </summary>
        /// <returns></returns>
        public Generation.ChunkGeography GetGeography()
        {
            Generation.ChunkGeography chunk = new Generation.ChunkGeography();
            float xMin = GridPosition.X;
            float yMin = GridPosition.Y;
            float xMax = xMin + Scale;
            float yMax = yMin + Scale;

            // Phi selon X
            chunk.PhiMin = xMin * (float)Math.PI * 2;
            chunk.PhiMax = xMax * (float)Math.PI * 2;

            // Theta selon Y
            chunk.ThetaMin = yMin * (float)Math.PI;
            chunk.ThetaMax = yMax * (float)Math.PI;

            return chunk;
        }
        #endregion
        #endregion
    }
}
