using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleTriangle
{
    /// <summary>
    /// Représente un indicateur du temps de jeu : temps écoulé entre la frame précédente et la suivante + temps total.
    /// </summary>
    public class GameTime
    {
        #region Properties
        /// <summary>
        /// Temps écoulé lors de la dernière frame.
        /// </summary>
        public TimeSpan LastFrameElapsedTime { get; set; }
        /// <summary>
        /// Obtient le temps de jeu total, avant la dernière frame.
        /// </summary>
        public TimeSpan TotalGameTime { get; set; }
        #endregion

        #region Methods
        public GameTime()
        {
            LastFrameElapsedTime = new TimeSpan(0, 0, 0, 0, 25);
            TotalGameTime = new TimeSpan(0);
        }
        #endregion
    }
}
