using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleTriangle.Noise
{
    class ValueCoherentNoise : NoiseBase
    {
        #region Methods
        /// <summary>
        /// Crée une nouvelle instance de WhiteNoise.
        /// </summary>
        public ValueCoherentNoise()
        {

        }


        double MakeInt32Range(double value)
        {
            return value % Int32.MaxValue;
        }

        public override float GetValue (float x, float y, float z)
        {
            // return ValueNoise3D((int)x, (int)y, (int)z, m_seed);
            return ValueCoherentNoise3D(x * m_frequency, y * m_frequency, 0, m_seed, NoiseQuality.QUALITY_BEST);
        }

        #endregion
    }


}