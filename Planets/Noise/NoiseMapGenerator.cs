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
using System.Threading;
using SlimDX;
namespace SimpleTriangle.Noise
{
    /// <summary>
    /// Permet la génération d'une carte de bruit dans une heightmap.
    /// </summary>
    public class NoiseMapGenerator
    {
        /// <summary>
        /// Représente une collection de paramètres pouvant être passés à un bruit.
        /// </summary>
        public class NoiseParameters
        {
            /// <summary>
            /// Contient une liste de bruits pouvant être désignés par leur ID.
            /// </summary>
            public static List<Type> Noises = new List<Type>() {
                typeof(PerlinNoise),                    // 0
                typeof(RidgedMultifractalNoise),        // 1
                typeof(VoronoiNoise),                   // 2
                typeof(WhiteNoise),                     // 3
                typeof(GradientNoise),                  // 4
                typeof(GradientCoherentNoise),          // 5
                typeof(ValueCoherentNoise)};            // 6
            static Random s_seedRandom = new Random();
            public const int PERLIN_ID = 0;
            public const int RIDGED_ID = 1;
            public const int VORONOI_ID = 2;
            public const int WHITE_ID = 3;
            /// <summary>
            /// Type de bruit utilisé.
            /// </summary>
            public int NoiseType { get; set; }
            /// <summary>
            /// Nombre d'octaves du bruit.
            /// Plus va valeur est grande, plus le bruit aura du détail.
            /// </summary>
            public int OctaveCount { get; set; }
            /// <summary>
            /// Graine du bruit.
            /// Chaque graine produit un bruit radicalement différent.
            /// 
            /// Si la graine est inférieure à 0, une graine aléatoire sera générée.
            /// </summary>
            public int Seed { get; set; }
            /// <summary>
            /// Coefficient de réduction du bruit entre deux octaves successives.
            /// Plus cette valeur est grande, moins les octaves de bas niveau auront d'importance.
            /// </summary>
            public float Lacunarity { get; set; }
            /// <summary>
            /// Détermine la fréquence initiale du bruit.
            /// 
            /// Plus cette valeur est grande, plus il y aura de variations dans le bruit.
            /// </summary>
            public float Frequency { get; set; }
            /// <summary>
            /// Obtient ou définit la persistance du bruit.
            /// 
            /// Il s'agit du multiplicateur d'amplitude entre deux octaves de bruit successives.
            /// </summary>
            public float Persistence { get; set; }
            public Vector2 NoiseStart { get; set; }
            public Vector2 NoiseEnd { get; set; }
            /// <summary>
            /// Initialise une nouvelle instance de NoiseParameters avec des valeurs par défaut.
            /// </summary>
            public NoiseParameters()
            {
                OctaveCount = 8;
                Seed = -1;
                Frequency = 1.0f;
                Lacunarity = 5.0f;
                Persistence = 1;
                NoiseType = 0;
                NoiseEnd = new Vector2(1.0f, 1.0f);
            }

            /// <summary>
            /// Crée une instance de bruit à partir des paramètres donnés.
            /// </summary>
            /// <returns></returns>
            public NoiseBase CreateNoise()
            {
                NoiseBase noise = (NoiseBase)Activator.CreateInstance(Noises[NoiseType]);
                noise.Seed = Seed < 0 ? s_seedRandom.Next() : Seed;
                noise.Persistence = Persistence;
                noise.OctaveCount = OctaveCount;
                noise.Lacunarity = Lacunarity;
                noise.Quality = NoiseBase.NoiseQuality.QUALITY_BEST;
                noise.Frequency = Frequency;
                return noise;
            }
        }
        
        const int GENERATION_THREADS = 2;

        /// <summary>
        /// Génère une heighmap à partir d'un bruit Ridged Multifratcal.
        /// Version parrallélisée.
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        public static unsafe float[,] GenerateMultiNoiseSleep(int size, NoiseParameters repartitionNoiseParams,
            NoiseParameters noiseHighParams,
            NoiseParameters noiseLowParams)
        {
            Random rand = new Random();
            float[,] heightmap = new float[size, size];

            // Création des bruits.
            NoiseBase repartitionNoise = repartitionNoiseParams.CreateNoise();
            NoiseBase noiseHigh = noiseHighParams.CreateNoise();
            NoiseBase noiseLow = noiseLowParams.CreateNoise();


            // Donne une liste de tâches à effectuer pour chaque core.
            int taskSize = size;
            int taskStart = 0;
            int taskEnd = taskSize;

            for (int x = taskStart; x < taskEnd; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    float sx = LinearInterp(repartitionNoiseParams.NoiseStart.X, repartitionNoiseParams.NoiseEnd.X, x / (float)(size - 1));
                    float sy = LinearInterp(repartitionNoiseParams.NoiseStart.Y, repartitionNoiseParams.NoiseEnd.Y, y / (float)(size - 1));

                    // Obtention des valeurs des bruits.
                    float repartition = repartitionNoise.GetValue(sx, sy, 0);
                    float valueHigh = noiseHigh.GetValue(sx, sy, 0);
                    float valueLow = noiseLow.GetValue(sx, sy, 0);

                    // Interpolation linéaire entre value low et value high de coefficient donné par le bruit de
                    // répartition.
                    float value = (float)Math.Log(Math.Max(0.8,  1 + valueLow)) * 5 + valueHigh * 25 + repartition*3;
                    heightmap[x, y] = value;
                    
                }
            }

            return heightmap;
        }


        static VoronoiNoise vNoise = new VoronoiNoise()
        {
            Frequency = 4.0f,
            Lacunarity = 1.5f,
            OctaveCount = 2,
            Persistence = 0.9f,
            Seed = 47846,
            Quality = NoiseBase.NoiseQuality.QUALITY_BEST
        };

        static PerlinNoise s_continentNoise = new PerlinNoise()
        {
            Frequency   = 6.0f,         // 6.0f
            Lacunarity  = 3.1f,         // 3.1f
            OctaveCount = 5,            // 5
            Persistence = 0.345f,
            Seed = 449815,
            Quality = NoiseBase.NoiseQuality.QUALITY_BEST,

        };
        static PerlinNoise s_mountainsRepNoise = new PerlinNoise()
        {
            Frequency   = 16.0f,        // 16.0f
            Lacunarity  = 2.0f,         // 1.0f
            OctaveCount = 3,            // 1
            Persistence = 0.445f,
            Seed = 5542,
            Quality = NoiseBase.NoiseQuality.QUALITY_FAST,

        };
        static RidgedMultifractalNoise s_mountainsNoise = new RidgedMultifractalNoise()
        {
            Frequency = 12.0f,
            Lacunarity = 2.3f, // 1.8f
            OctaveCount = 8,
            Persistence = 0.105f,
            Seed = 44985715,
            Quality = NoiseBase.NoiseQuality.QUALITY_BEST,

        };


        static RidgedMultifractalNoise s_ridgesNoise = new RidgedMultifractalNoise()
        {
            Frequency = 10.0f,
            Lacunarity = 8.5f,
            OctaveCount = 6,
            Seed = 84494650,
            Quality = NoiseBase.NoiseQuality.QUALITY_BEST
        };

        static PerlinNoise s_bumpNoise = new PerlinNoise()
        {
            Frequency = 32.0f,
            Lacunarity = 1.6f,
            OctaveCount = 4,
            Seed = 74572356,
            Persistence = 0.48f,
            Quality = NoiseBase.NoiseQuality.QUALITY_BEST
        };

        static PerlinNoise s_reliefRepartitionNoise = new PerlinNoise()
        {
            Frequency = 5.0f,
            Lacunarity = 2.0f,
            OctaveCount = 2,
            Persistence = 0.26f,
            Seed = 4540450,
            Quality = NoiseBase.NoiseQuality.QUALITY_BEST
        };

        static PerlinNoise s_beachHeightNoise = new PerlinNoise()
        {
            Frequency = 6.0f,
            Lacunarity = 2.0f,
            OctaveCount = 2,
            Persistence = 0.35f,
            Seed = 5854707,
            Quality = NoiseBase.NoiseQuality.QUALITY_BEST
        };

        static volatile Dictionary<Vector3, float> s_cache = new Dictionary<Vector3, float>();
        /// <summary>
        /// Obtient une valeur de bruit combinée des bruits passés en paramètres, selon une certaine formule magique :p
        /// </summary>
        /// <param name="reparitionNoise"></param>
        /// <param name="noiseHigh"></param>
        /// <param name="noiseLow"></param>
        /// <returns></returns>
        public static float GetMultiNoiseValue(NoiseBase repartitionNoise, NoiseBase noiseHigh, NoiseBase noiseLow, 
            float x, float y, float z)
        {
            if (repartitionNoise is WhiteNoise)
                return repartitionNoise.GetValue(x, y, z);
            else
            {
                if (s_cache.ContainsKey(new Vector3(x, y, z)))
                    return s_cache[new Vector3(x, y, z)];
            }
            
            // Noise modélisant les endroits où sont placés les continents.
            float continentNoise = s_continentNoise.GetValue(x, y, z);
            // Noise modélisant les endroits où sont placées les montagnes.
            float mountainsRepNoise = s_mountainsRepNoise.GetValue(x, y, z);
            // Noise modélisant les montagnes.
            float mountainsNoise = s_mountainsNoise.GetValue(x, y, z);
            // Noise modélisant les détails de la surface au sol.
            float ridgesNoise = s_ridgesNoise.GetValue(x, y, z);
            float bumpNoise = s_bumpNoise.GetValue(x, y, z);
            // Noise permettant la répartition des reliefs ridges / bump.
            float reliefRepartitionNoise = s_reliefRepartitionNoise.GetValue(x, y, z);
            // Noise modélisant la taille des plages.
            float beachNoise = s_beachHeightNoise.GetValue(x, y, z);

            float adjustedContinentNoise = continentNoise - 0.3f; // 0.3f

            // 0.02 pour des côtes, 0.1 pour des plages
            float beachValue = Math.Min(0.050f, Math.Max((beachNoise + 0.5f) / 500.0f, 0.020f));


            // "Coupe" les continents lorsqu'ils dépassent la taille de la plage.
            float continentValue = Math.Min(adjustedContinentNoise, beachValue);
            float continent = continentValue * 1000;

            // Mountains
            float interpMin = 0.0f;
            float interpMax = 0.5f;
            float interpValue = Math.Min(interpMax, Math.Max(interpMin, adjustedContinentNoise));
            float mountainsModulation = lerp(interpMin, interpMax, interpValue) * Math.Max(0, mountainsRepNoise);
            // Crée des montagnes seulement là où le bruit de répartition est élevé.
            float mountains = mountainsModulation * mountainsNoise * 2400;
            mountains = MountainShapeFunc(40, 80, 1, mountains);

            // Ridges
            float ridges = Math.Max(0f, ridgesNoise) * 75; // 250
            float bumps = Math.Max(0f, bumpNoise) * 50; // 125
            float relief = lerp(bumps, ridges, Math.Max(0, Math.Min(1, reliefRepartitionNoise))) * 5;

            // Valeur finale du bruit.
            float value = continent + relief + mountains; // + relief + mountains; // +relief
            s_cache[new Vector3(x, y, z)] = value * World.Planet.AltitudeScale;
            return value * World.Planet.AltitudeScale;
        }

        static float MountainShapeFunc(float t1, float t2, float a, float t)
        {
            float x = t % (t1 + t2);
            float f1 = a * (t1 + t2) * ((t- x) / (t1 + t2) );
            float f2 = t % (t1 + t2) < t1 ? 0 : (t1 + t2) *  (x - t1) / t2;
            return f1+f2;
        }

        static float lerp(float v1, float v2, float factor)
        {
            return v1 * (1 - factor) + v2 * factor;
        }

        /// <summary>
        /// Génère une heighmap à partir d'un bruit Ridged Multifratcal.
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        public static float[,] Generate(int size, NoiseParameters parameters)
        {

            NoiseBase noise = parameters.CreateNoise();
            Random rand = new Random();
            float[,] heightmap = new float[size,size];

            
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    
                    float value = (float)noise.GetValue(
                        LinearInterp(parameters.NoiseStart.X, parameters.NoiseEnd.X, x / (float)(size-1)),
                        LinearInterp(parameters.NoiseStart.Y, parameters.NoiseEnd.Y, y / (float)(size-1)), 
                        0.0f);
                    heightmap[x, y] = value;
                }
            }
            
            return heightmap;
        }

        public static float LinearInterp(float n0, float n1, float a)
        {
            return ((1.0f - a) * n0) + (a * n1);
        }
    }
}
