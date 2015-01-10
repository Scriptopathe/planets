
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleTriangle.Noise
{
    class PerlinNoise : NoiseBase
    {

        #region Constants
        const float SQRT_3 = 1.73205f;
        float DEFAULT_FREQUENCY = 1.0f;
        float DEFAULT_LACUNARITY = 2.0f;
        int DEFAULT_OCTAVE_COUNT = 6;
        NoiseQuality DEFAULT_QUALITY = NoiseQuality.QUALITY_BEST;
        int DEFAULT_SEED = 0;
        float DEFAULT_PERSISTANCE = 0.2f;
        int MAX_OCTAVE = 30;
        #endregion

        #region Variables

        #endregion

        #region Properties
        /// <summary>
        /// Obtient ou définit la lacunarité du bruit.
        /// </summary>
        public override float Lacunarity
        {
            get
            {
                return base.Lacunarity;
            }
            set
            {
                base.Lacunarity = value;
            }
        }
        /// <summary>
        /// Obtient ou définit le nombre d'octaves du bruit.
        /// </summary>
        public override int OctaveCount
        {
            get
            {
                return base.OctaveCount;
            }
            set
            {
                if (value > MAX_OCTAVE)
                {
                    throw new Exception();
                }
                base.OctaveCount = value;
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Crée une nouvelle instance de RidgetMultiFractalNoise avec les paramètres par défaut.
        /// </summary>
        public PerlinNoise()
        {
            m_frequency = DEFAULT_FREQUENCY;
            m_lacunarity = DEFAULT_LACUNARITY;
            m_noiseQuality = DEFAULT_QUALITY;
            m_octaveCount = DEFAULT_OCTAVE_COUNT;
            m_seed = DEFAULT_SEED;
            m_persistence = DEFAULT_PERSISTANCE;
        }


        /// <summary>
        /// Code original :  F. Kenton "Doc Mojo" Musgrave, 1998.
        /// Modifié par jas pour la libnoise.
        /// Modifié par team-modouv pour le fonctionnement avec notre moteur.
        /// </summary>
        public override float GetValue (float x, float y, float z)
        {
            float value = 0.0f;
            float signal = 0.0f;
            float curPersistence = 1.0f;
            float nx, ny, nz;
            int seed;

            x *= m_frequency;
            y *= m_frequency;
            z *= m_frequency;

            for (int curOctave = 0; curOctave < m_octaveCount; curOctave++)
            {

                // Make sure that these floating-point values have the same range as a 32-
                // bit integer so that we can pass them to the coherent-noise functions.
                nx = MakeInt32Range(x);
                ny = MakeInt32Range(y);
                nz = MakeInt32Range(z);

                // Get the coherent-noise value from the input value and add it to the
                // final result.
                seed = (m_seed + curOctave) & 0x7fffffff;
                signal = GradientCoherentNoise3D(nx, ny, nz, seed, m_noiseQuality);
                value += signal * curPersistence;

                // Prepare the next octave.
                x *= m_lacunarity;
                y *= m_lacunarity;
                z *= m_lacunarity;
                curPersistence *= m_persistence;
            }

            return value;
        }

    #endregion
    }


}