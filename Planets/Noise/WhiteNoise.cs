using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleTriangle.Noise
{
    class WhiteNoise : NoiseBase
    {
        #region Methods
        /// <summary>
        /// Crée une nouvelle instance de WhiteNoise.
        /// </summary>
        public WhiteNoise()
        {

        }


        double MakeInt32Range(double value)
        {
            return value % Int32.MaxValue;
        }

        public override float GetValue (float x, float y, float z)
        {
            return 0; // m_seed < 100000000 ? -1 : 1;
        }

    #endregion
    }


}