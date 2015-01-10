using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleTriangle.Noise
{
    class RidgedMultifractalNoise : NoiseBase
    {

        #region Constants
        float DEFAULT_RIDGED_FREQUENCY = 1.0f;
        float DEFAULT_RIDGED_LACUNARITY = 2.0f;
        int DEFAULT_RIDGED_OCTAVE_COUNT = 6;
        float DEFAULT_RIDGED_PERSISTANCE = 1.0f;
        NoiseQuality DEFAULT_RIDGED_QUALITY = NoiseQuality.QUALITY_BEST;
        int DEFAULT_RIDGED_SEED = 0;
        int RIDGED_MAX_OCTAVE = 30;
        #endregion

        #region Variables
        /// <summary>
        /// Persistance précalculée par octave.
        /// </summary>
        float[] m_octavePersistences;
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
                ComputeOctavePersistances();
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
                if (value > RIDGED_MAX_OCTAVE)
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
        public RidgedMultifractalNoise()
        {
            m_frequency = DEFAULT_RIDGED_FREQUENCY;
            m_lacunarity = DEFAULT_RIDGED_LACUNARITY;
            m_noiseQuality = DEFAULT_RIDGED_QUALITY;
            m_octaveCount = DEFAULT_RIDGED_OCTAVE_COUNT;
            m_seed = DEFAULT_RIDGED_SEED;
            m_persistence = DEFAULT_RIDGED_PERSISTANCE;
            ComputeOctavePersistances();   
        }

        /// <summary>
        /// Pré-calcule la persistance de chaque octave.
        /// </summary>
        void ComputeOctavePersistances ()
        {
          float h = DEFAULT_RIDGED_PERSISTANCE;
          float frequency = 1.0f;
          m_octavePersistences = new float[RIDGED_MAX_OCTAVE];
          for (int i = 0; i < RIDGED_MAX_OCTAVE; i++) {
            m_octavePersistences[i] = (float)Math.Pow (frequency, -h);
            frequency *= m_lacunarity;
          }
        }


        /// <summary>
        /// Code original :  F. Kenton "Doc Mojo" Musgrave, 1998.
        /// Modifié par jas pour la libnoise.
        /// Modifié par team-modouv pour le fonctionnement avec notre moteur.
        /// </summary>
        public override float GetValue (float x, float y, float z)
        {
          x *= m_frequency;
          y *= m_frequency;
          z *= m_frequency;

          float signal = 0.0f;
          float value  = 0.0f;
          float weight = 1.0f;

          // These parameters should be user-defined; they may be exposed in a
          // future version of libnoise.
          float offset = 1.0f;
          float gain = 2.0f;

          for (int curOctave = 0; curOctave < m_octaveCount; curOctave++) {

            // Make sure that these floating-point values have the same range as a 32-
            // bit integer so that we can pass them to the coherent-noise functions.
            float nx, ny, nz;
            nx = MakeInt32Range (x);
            ny = MakeInt32Range (y);
            nz = MakeInt32Range (z);

            // Get the coherent-noise value.
            int seed = (m_seed + curOctave) & 0x7fffffff;
            signal = GradientCoherentNoise3D (nx, ny, nz, seed, m_noiseQuality);

            // Make the ridges.
            signal = Math.Abs (signal);
            signal = offset - signal;

            // Square the signal to increase the sharpness of the ridges.
            signal *= signal;

            // The weighting from the previous octave is applied to the signal.
            // Larger values have higher weights, producing sharp points along the
            // ridges.
            signal *= weight;

            // Weight successive contributions by the previous signal.
            weight = signal * gain;
            if (weight > 1.0f) {
              weight = 1.0f;
            }
            if (weight < 0.0f) {
              weight = 0.0f;
            }

            // Add the signal to the output value.
            value += (signal * m_octavePersistences[curOctave]);

            // Go to the next octave.
            x *= m_lacunarity;
            y *= m_lacunarity;
            z *= m_lacunarity;
          }
          value = value * 1.25f - 1.0f;
          return value;
        }

    #endregion
}


}