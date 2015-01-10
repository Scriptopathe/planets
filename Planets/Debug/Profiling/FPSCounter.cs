// Copyright (C) 2013, 2014 Alvarez Josué
//
// This code is free software; you can redistribute it and/or modify it
// under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2.1 of the License, or (at
// your option) any later version.
//
// This code is distributed in the hope that it will be useful, but WITHOUT
// ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or
// FITNESS FOR A PARTICULAR PURPOSE.  See the GNU Lesser General Public
// License (LICENSE.txt) for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with this library; if not, write to the Free Software Foundation,
// Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
//
// The developer's email is jUNDERSCOREalvareATetudDOOOTinsa-toulouseDOOOTfr (for valid email, replace 
// capital letters by the corresponding character)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleTriangle.Debug.Profiling
{
    /// <summary>
    /// Permet de compter et calculer la moyenne de FPS rendues par une scène.
    /// </summary>
    public class FPSCounter
    {
        const int MAX_COMPUTE_FRAMES = 30;
        List<float> m_lastComputationTime;
        /// <summary>
        /// Initialise une nouvelle instance de FPSCounter.
        /// </summary>
        public FPSCounter()
        {
            m_lastComputationTime = new List<float>();
        }

        /// <summary>
        /// Ajoute une frame en précisant le temps en secondes qu'il lui a fallu pour être calculée.
        /// </summary>
        /// <param name="computationTimeSeconds"></param>
        public void AddFrame(float computationTimeSeconds)
        {
            m_lastComputationTime.Add(computationTimeSeconds);
            if (m_lastComputationTime.Count > MAX_COMPUTE_FRAMES)
                m_lastComputationTime.RemoveAt(0);
        }

        /// <summary>
        /// Retourne la moyenne de FPS rendues par la scène.
        /// </summary>
        /// <returns></returns>
        public int GetAverageFps()
        {
            float totalComputationTimeSeconds = 0;
            foreach (float computeTimeSeconds in m_lastComputationTime)
            {
                totalComputationTimeSeconds += computeTimeSeconds;
            }
            // Calcul de la moyenne.
            float average = totalComputationTimeSeconds / m_lastComputationTime.Count;
            return (int)(1 / average);
        }
    }
}
