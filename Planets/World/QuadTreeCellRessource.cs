using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX;
namespace SimpleTriangle.World
{
    /// <summary>
    /// Représente une ressource contenue dans une cellule de quadtree;
    /// </summary>
    public abstract class QuadTreeCellRessource
    {
        #region Properties
        /// <summary>
        /// Obtient ou définit la cellule parente de cette ressource.
        /// </summary>
        public QuadTreeCell Parent { get; set; }

        #endregion

        #region Methods
        /// <summary>
        /// Crée une nouvelle instance de QuadTreeCellRessource ayant pour parant la cellule précisée.
        /// </summary>
        /// <param name="parentCell"></param>
        public QuadTreeCellRessource(QuadTreeCell parentCell)
        {
            Parent = parentCell;
        }
        /// <summary>
        /// Mets à jour la cellule.
        /// </summary>
        public abstract void Update();

        /// <summary>
        /// Dessine la cellule.
        /// </summary>
        public abstract void Draw();

        /// <summary>
        /// Annule la génération de la cellule.
        /// </summary>
        public abstract void AbortGeneration();
        #endregion

        #region Expansion API
        /// <summary>
        /// Retourne l'AABB contenant cette ressource.
        /// </summary>
        /// <returns></returns>
        public abstract BoundingBox GetAABB();
        /// <summary>
        /// Crée et retourne la ressource fille correspondant à la cellule parent passée en argument.
        /// </summary>
        /// <returns></returns>
        public abstract QuadTreeCellRessource Subdivide(QuadTreeCell parentCells);

        /// <summary>
        /// Lance la génération de la ressource. 
        /// </summary>
        public abstract void StartRessourceGeneration();
        /// <summary>
        /// Indique si la ressource actuelle est prête à être utilisée.
        /// </summary>
        public abstract bool IsRessourceReady { get; }
        /// <summary>
        /// Supprime définitivement cette ressource.
        /// </summary>
        public abstract void Dispose();
        #endregion
    }
}
