using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleTriangle.Noise
{
    class VoronoiNoise : NoiseBase
    {

        #region Constants
        const float SQRT_3 = 1.73205f;
        const float SQRT_2 = 1.41421f;
        float DEFAULT_FREQUENCY = 1.0f;
        float DEFAULT_LACUNARITY = 2.0f;
        int DEFAULT_OCTAVE_COUNT = 6;
        float DEFAULT_DISPLACEMENT = 1.0f;
        NoiseQuality DEFAULT_QUALITY = NoiseQuality.QUALITY_BEST;
        int DEFAULT_SEED = 0;

        int MAX_OCTAVE = 30;
        #endregion

        #region Variables
        bool m_enableDistance;
        float m_displacement;
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

        public float Displacement
        {
            get
            {
                return m_displacement;
            }
            set
            {
                m_displacement = value;
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
        public VoronoiNoise()
        {
            m_frequency = DEFAULT_FREQUENCY;
            m_lacunarity = DEFAULT_LACUNARITY;
            m_noiseQuality = DEFAULT_QUALITY;
            m_octaveCount = DEFAULT_OCTAVE_COUNT;
            m_seed = DEFAULT_SEED;
            m_enableDistance = true;
            m_displacement = DEFAULT_DISPLACEMENT;
        }


        float MakeInt32Range(float value)
        {
            return value % Int32.MaxValue;
        }
        /// <summary>
        /// Code original :  F. Kenton "Doc Mojo" Musgrave, 1998.
        /// Modifié par jas pour la libnoise.
        /// Modifié par team-modouv pour le fonctionnement avec notre moteur.
        /// </summary>
        public override float GetValue(float x, float y, float z)
        {
            // This method could be more efficient by caching the seed values.  Fix
            // later.

            x *= m_frequency;
            y *= m_frequency;

            int xInt = (x > 0.0 ? (int)x : (int)x - 1);
            int yInt = (y > 0.0 ? (int)y : (int)y - 1);

            float minDist = 2147483647.0f;
            float xCandidate = 0;
            float yCandidate = 0;

            // Inside each unit cube, there is a seed point at a random position.  Go
            // through each of the nearby cubes until we find a cube with a seed point
            // that is closest to the specified position.
            for (int yCur = yInt - 2; yCur <= yInt + 2; yCur++)
            {
                for (int xCur = xInt - 2; xCur <= xInt + 2; xCur++)
                {

                    // Calculate the position and distance to the seed point inside of
                    // this unit cube.
                    float xPos = xCur + ValueNoise3D(xCur, yCur, 0, m_seed);
                    float yPos = yCur + ValueNoise3D(xCur, yCur, 0, m_seed + 1);
                    float xDist = xPos - x;
                    float yDist = yPos - y;
                    float dist = xDist * xDist + yDist * yDist;

                    if (dist < minDist)
                    {
                        // This seed point is closer to any others found so far, so record
                        // this seed point.
                        minDist = dist;
                        xCandidate = xPos;
                        yCandidate = yPos;
                    }
                }
            
            }

            float value;
            if (m_enableDistance)
            {
                // Determine the distance to the nearest seed point.
                float xDist = xCandidate - x;
                float yDist = yCandidate - y;
                value = ((float)Math.Sqrt(xDist * xDist + yDist * yDist)
                  ) * SQRT_2 - 1.0f;
            }
            else
            {
                value = 0.0f;
            }

            // Return the calculated distance with the displacement value applied.
            return value + (m_displacement * (float)ValueNoise3D(
              (int)(Math.Floor(xCandidate)),
              (int)(Math.Floor(yCandidate)),
              0));
        }
        // Multifractal code originally written by F. Kenton "Doc Mojo" Musgrave,
        // 1998.  Modified by jas for use with libnoise.
        public float GetValue3D (float x, float y, float z)
        {
            // This method could be more efficient by caching the seed values.  Fix
            // later.

            x *= m_frequency;
            y *= m_frequency;
            z *= m_frequency;

            int xInt = (x > 0.0 ? (int)x : (int)x - 1);
            int yInt = (y > 0.0 ? (int)y : (int)y - 1);
            int zInt = (z > 0.0 ? (int)z : (int)z - 1);

            float minDist = 2147483647.0f;
            float xCandidate = 0;
            float yCandidate = 0;
            float zCandidate = 0;

            // Inside each unit cube, there is a seed point at a random position.  Go
            // through each of the nearby cubes until we find a cube with a seed point
            // that is closest to the specified position.
            for (int zCur = zInt - 2; zCur <= zInt + 2; zCur++)
            {
                for (int yCur = yInt - 2; yCur <= yInt + 2; yCur++)
                {
                    for (int xCur = xInt - 2; xCur <= xInt + 2; xCur++)
                    {

                        // Calculate the position and distance to the seed point inside of
                        // this unit cube.
                        float xPos = xCur + ValueNoise3D(xCur, yCur, zCur, m_seed);
                        float yPos = yCur + ValueNoise3D(xCur, yCur, zCur, m_seed + 1);
                        float zPos = zCur + ValueNoise3D(xCur, yCur, zCur, m_seed + 2);
                        float xDist = xPos - x;
                        float yDist = yPos - y;
                        float zDist = zPos - z;
                        float dist = xDist * xDist + yDist * yDist + zDist * zDist;

                        if (dist < minDist)
                        {
                            // This seed point is closer to any others found so far, so record
                            // this seed point.
                            minDist = dist;
                            xCandidate = xPos;
                            yCandidate = yPos;
                            zCandidate = zPos;
                        }
                    }
                }
            }

            float value;
            if (m_enableDistance)
            {
                // Determine the distance to the nearest seed point.
                float xDist = xCandidate - x;
                float yDist = yCandidate - y;
                float zDist = zCandidate - z;
                value = ((float)Math.Sqrt(xDist * xDist + yDist * yDist + zDist * zDist)
                  ) * SQRT_3;
            }
            else
            {
                value = 0.0f;
            }

            // Return the calculated distance with the displacement value applied.
            return value + (m_displacement * (float)ValueNoise3D(
              (int)(Math.Floor(xCandidate)),
              (int)(Math.Floor(yCandidate)),
              (int)(Math.Floor(zCandidate))));
        }

    #endregion
}


}