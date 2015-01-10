﻿// Copyright (C) 2013, 2014 Alvarez Josué
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

namespace SimpleTriangle.Noise
{
    class GradientNoise : NoiseBase
    {
        #region Methods
        /// <summary>
        /// Crée une nouvelle instance de WhiteNoise.
        /// </summary>
        public GradientNoise()
        {

        }


        public override float GetValue (float x, float y, float z)
        {
            return GradientNoise2D(x * m_frequency, y * m_frequency, (int)(x * m_frequency), (int)(y * m_frequency), m_seed);
        }

        #endregion
    }


}