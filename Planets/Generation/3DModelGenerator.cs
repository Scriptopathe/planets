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
using SlimDX;
using SlimDX.D3DCompiler;
using SlimDX.Direct3D11;
using SlimDX.DXGI;
using SlimDX.Windows;
using Device = SlimDX.Direct3D11.Device;
using Resource = SlimDX.Direct3D11.Resource;
using Vertex = SimpleTriangle.World.Graphics.VertexPositionTextureNormal.Vertex;
using VWrapper = SimpleTriangle.World.Graphics.VertexPositionTextureNormal.VertexWrapper;
using System.Threading;
namespace SimpleTriangle.Generation
{
    public class ChunkAltitude
    {
        /// <summary>
        /// Altitude max.
        /// </summary>
        public float MaxAltitude;
        public float MinAltitude;
        public ChunkAltitude()
        {
            MaxAltitude = float.MinValue;
            MinAltitude = float.MaxValue;
        }
    }
    public class ChunkGeography
    {

        /// <summary>
        /// Lattitude minimum de la grille.
        /// </summary>
        public float ThetaMin { get; set; }
        /// <summary>
        /// Lattitude maximum de la grille.
        /// </summary>
        public float ThetaMax { get; set; }
        /// <summary>
        /// Longitude minimum de la grille.
        /// </summary>
        public float PhiMin { get; set; }
        /// <summary>
        /// Longitude maximum de la grille.
        /// </summary>
        public float PhiMax { get; set; }
        public ChunkGeography()
        {
        }
    }
    /// <summary>
    /// Générateur de modèle 3D.
    /// </summary>
    public class ModelGenerator
    {
        #region IndexBuffers
        static Dictionary<int, SlimDX.Direct3D11.Buffer> s_indexBuffers = new Dictionary<int, SlimDX.Direct3D11.Buffer>();
        static volatile Dictionary<int, int[]> s_cpuIndexBuffers = new Dictionary<int, int[]>();

        /// <summary>
        /// </summary>
        /// Génère un inde◙ buffer (CPU) pour une grille de la taille donnée.
        /// <param name="gridSize"></param>
        /// <returns></returns>
        static int[] GenerateCpuIndexBuffer(int gridSize)
        {
            // Index buffer contenant l'ordre dans lequel dessiner les vertex. (sous forme de carrés).
            int iBufferSize = (gridSize - 1) * (gridSize - 1) * 6;
            int[] indexBuffer = new int[iBufferSize];
            int sizeX = gridSize;
            int sizeY = gridSize;
            int startIndex = 0;
            for (int x = 0; x < sizeX - 1; x++)
            {
                for (int y = 0; y < sizeY - 1; y++)
                {
                    int firstIndex = x + y * (sizeX);
                    int topLeft = firstIndex;
                    int topRight = firstIndex + 1;
                    int lowerLeft = topLeft + sizeX;
                    int lowerRight = lowerLeft + 1;
                    // Triangle 1 (up right)
                    indexBuffer[startIndex++] = topLeft;
                    indexBuffer[startIndex++] = lowerRight;
                    indexBuffer[startIndex++] = lowerLeft;

                    // Triangle 2 (bottom left)
                    indexBuffer[startIndex++] = topLeft;
                    indexBuffer[startIndex++] = topRight;
                    indexBuffer[startIndex++] = lowerRight;
                }
            }
            return indexBuffer;
        }

        static volatile bool getIndexBufferCpuLock;
        /// <summary>
        /// Obtient l'index buffer (CPU) pour une grille de la taille donnée.
        /// </summary>
        /// <param name="gridSize"></param>
        /// <returns></returns>
        public static int[] GetIndexBufferCpu(int gridSize)
        {
            while (getIndexBufferCpuLock)
                Thread.Sleep(1);
            getIndexBufferCpuLock = true;
            if (!s_cpuIndexBuffers.ContainsKey(gridSize))
            {
                var buff1 = GenerateCpuIndexBuffer(gridSize);
                s_cpuIndexBuffers.Add(gridSize, buff1);   
            }
            var buff = s_cpuIndexBuffers[gridSize];
            getIndexBufferCpuLock = false;
            return buff;
            
        }
        /// <summary>
        /// Génère l'index buffer pour une grille de la taille donnée.
        /// </summary>
        /// <param name="gridSize"></param>
        /// <returns></returns>
        static SlimDX.Direct3D11.Buffer GenerateIndexBuffer(int gridSize)
        {
            int iBufferSize = (gridSize - 1) * (gridSize - 1) * 6;
            int[] indexBuffer = GetIndexBufferCpu(gridSize);

            DataStream iBuffStream = new DataStream(iBufferSize * sizeof(int), true, true);
            iBuffStream.WriteRange<int>(indexBuffer);
            iBuffStream.Position = 0;
            var iBuffer = new SlimDX.Direct3D11.Buffer(Scene.GetGraphicsDevice(), iBuffStream, new BufferDescription()
            {
                BindFlags = BindFlags.IndexBuffer,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None,
                SizeInBytes = (int)iBuffStream.Length,
                Usage = ResourceUsage.Default
            });
            iBuffStream.Dispose();
            return iBuffer;
        }
        /// <summary>
        /// Obtient l'index buffer pour une grille de la taille donnée.
        /// </summary>
        /// <param name="gridSize"></param>
        /// <returns></returns>
        public static SlimDX.Direct3D11.Buffer GetIndexBuffer(int gridSize)
        {
            if (s_indexBuffers.ContainsKey(gridSize))
                return s_indexBuffers[gridSize];
            else
            {
                s_indexBuffers.Add(gridSize, GenerateIndexBuffer(gridSize));
                return s_indexBuffers[gridSize];
            }
        }
        #endregion

        /// <summary>
        /// Génère un modèle 3D en forme de planète à partir de plusieurs bruits.
        /// </summary>
        /// <param name="aabb">Bounding box du morceau de planète généré.</param>
        /// <param name="gridLevelScale">Echelle du morceau de grille à générer. L'échelle est divisée par 2
        /// à chaque étage du quadtree</param>
        /// <param name="gridSize">Taille de la grille à générer.</param>
        /// <param name="highNoise">Bruit "high"</param>
        /// <param name="iBuffer">Index buffer en du mesh créé en sortie.</param>
        /// <param name="initialGridPos">Position du coin supérieur gauche de la grille à générer dans la grille initiale de range [0, 1]</param>
        /// <param name="lowNoise">Bruit "low"</param>
        /// <param name="repNoise">Bruit de répartition</param>
        /// <param name="vBuffer">Vertex buffer en du mesh créé en sortie.</param>
        /// <param name="chunkGeography">Informations de sortie concernant la géographie du morceau généré.</param>
        /// <param name="planetPosition">Position de la planète.</param>
        /// <param name="radius">Rayon de la planète.</param>
        /// <returns></returns>
        public static void GeneratePlanet(Vector3 planetPosition,
            float radius,
            int gridSize, 
            Vector2 initialGridPos,
            float gridLevelScale,
            Noise.NoiseBase lowNoise,
            Noise.NoiseBase highNoise,
            Noise.NoiseBase repNoise,
            out SlimDX.Direct3D11.Buffer vBuffer,
            out BoundingBox aabb,
            out ChunkAltitude geo)
        {
            // Geography
            geo = new ChunkAltitude();

            // Taille du buffer.
            int size = gridSize * gridSize ;
            const bool computeNormals = true;
            Vector3 min = new Vector3(float.MaxValue);
            Vector3 max = new Vector3(float.MinValue);

            // Création du vertex buffer contenant tous les vertex à dessiner.
            VWrapper[] vertexBuffer = new VWrapper[size];

            // Texture noise
            Noise.NoiseMapGenerator.NoiseParameters p = new Noise.NoiseMapGenerator.NoiseParameters()
            {
                Frequency = 128.0f,
                Lacunarity = 2.5f,
                NoiseEnd = new Vector2(initialGridPos.X + gridLevelScale, initialGridPos.Y + gridLevelScale),
                NoiseStart = new Vector2(initialGridPos.X, initialGridPos.Y),
                NoiseType = Noise.NoiseMapGenerator.NoiseParameters.RIDGED_ID,
                Persistence = 0.94f,
                Seed = 456464560
            };
            Noise.NoiseBase texNoise = p.CreateNoise();

            float theta, phi;
            Vector3 transformedPos;
            for (int y = 0; y < gridSize; y++)
            {
                for (int x = 0; x < gridSize; x++)
                {
                    // Cette section a pour but de calculer la normale par rapport au "sol" de la planète, pour cela
                    // Elle calcule les positions des verte
                    Vector2 pos2D = new Vector2(x / ((float)gridSize - 1),
                                    (y / ((float)gridSize - 1)));

                    // Transformation en sphère.
                    float noisePosX = initialGridPos.X + pos2D.X * gridLevelScale;
                    float noisePosY = initialGridPos.Y + pos2D.Y * gridLevelScale;
                    phi = (noisePosX) * (float)Math.PI * 2;
                    theta = (noisePosY) * (float)Math.PI;


                    // TODO : optimiser pour éviter les calculs redondants.
                    transformedPos = Util.SphericalCoords.ToCartesian(theta, phi, radius);//new Vector3(xSph, ySph, zSph);


                    // Création de la normale au sol.
                    Vector3 normal = Vector3.Normalize(transformedPos);

                    // Valeur du bruit
                    float noiseValue = Noise.NoiseMapGenerator.GetMultiNoiseValue(repNoise, highNoise, lowNoise,
                        transformedPos.X / radius, transformedPos.Y / radius, transformedPos.Z / radius);
                    float tNoiseValue = texNoise.GetValue(transformedPos.X / radius, transformedPos.Y / radius, transformedPos.Z / radius);

                    // Création de la position finale
                    Vector3 finalPos = transformedPos + normal * noiseValue;
                    Vector4 pos = new Vector4(finalPos, 1.0f);

                    // Informations de géométrie.
                    min = Util.MathHelper.Min(finalPos, min);
                    max = Util.MathHelper.Max(finalPos, max);
                    geo.MinAltitude = Math.Min(noiseValue, geo.MinAltitude);
                    geo.MaxAltitude = Math.Max(noiseValue, geo.MaxAltitude);

                    // Ajout des données dans le VBuffer.
                    int index = (x + y * gridSize);

                    vertexBuffer[index] = new VWrapper();
                    vertexBuffer[index].SphereNormal = normal;

                    // Position 3D du point avec displacement.
                    vertexBuffer[index].Vertex.Position = pos;
                    // Position 3D du point sans displacement.
                    vertexBuffer[index].Vertex.SpherePosition = new Vector4(transformedPos, 1.0f);
                    vertexBuffer[index].Vertex.SphereNormal = new Vector4(normal, 1.0f);
                    // Coordonnées de texture.
                    vertexBuffer[index].Vertex.Texture = new Vector2(noisePosX, noisePosY);
                    vertexBuffer[index].Vertex.Normal = new Vector4(0);
                    vertexBuffer[index].Vertex.Altitude = noiseValue;

                    // Valeurs additionnelles.
                    vertexBuffer[index].Vertex.TextureId = tNoiseValue;
                }
                
            }

            
            // Index buffer contenant l'ordre dans lequel dessiner les vertex. (sous forme de carrés).
            int iBufferSize = (gridSize - 1) * (gridSize - 1) * 6;
            int[] indexBuffer = GetIndexBufferCpu(gridSize);
            if (computeNormals)
            {
                //Thread.Sleep(1);
                for (int i = 0; i < indexBuffer.Length / 3; i++)
                {
                    Vector4 firstvec = vertexBuffer[indexBuffer[i * 3 + 1]].Vertex.Position - vertexBuffer[indexBuffer[i * 3]].Vertex.Position;
                    Vector4 secondvec = vertexBuffer[indexBuffer[i * 3]].Vertex.Position - vertexBuffer[indexBuffer[i * 3 + 2]].Vertex.Position;
                    Vector4 normal = new Vector4(Vector3.Cross(
                        new Vector3(firstvec.X, firstvec.Y, firstvec.Z),
                        new Vector3(secondvec.X, secondvec.Y, secondvec.Z)), 1.0f);
                    normal.Normalize();
                    vertexBuffer[indexBuffer[(i * 3)]].Vertex.Normal += normal;
                    vertexBuffer[indexBuffer[(i * 3 + 1)]].Vertex.Normal += normal;
                    vertexBuffer[indexBuffer[(i * 3 + 2)]].Vertex.Normal += normal;
                }
                for (int i = 0; i < vertexBuffer.Length; i++)
                {
                    var v = Util.MathHelper.ReduceXYZ(vertexBuffer[i].Vertex.Normal);
                    v.Normalize();
                    vertexBuffer[i].Vertex.Normal = new Vector4(v, 1.0f);
                    vertexBuffer[i].Vertex.SphereNormal = new Vector4(Util.MathHelper.ReduceXYZ(vertexBuffer[i].Vertex.Normal) - vertexBuffer[i].SphereNormal, 1.0f);

                }
            }

            // Création des buffers.
            DataStream vBuffStream = new DataStream(size * Vertex.Stride, true, true);
            for (int i = 0; i < size; i++)
                vBuffStream.Write<Vertex>(vertexBuffer[i].Vertex);
            vBuffStream.Position = 0;

            vBuffer = new SlimDX.Direct3D11.Buffer(Scene.GetGraphicsDevice(), vBuffStream, new BufferDescription()
            {
                BindFlags = BindFlags.VertexBuffer,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None,
                SizeInBytes = (int)vBuffStream.Length,
                Usage = ResourceUsage.Default
            });


            vBuffStream.Dispose();

            aabb = new BoundingBox(min, max);
        }

        /// <summary>
        /// Génère un modèle 3D à partir d'une heightmap.
        /// Range : [0, 1]
        /// </summary>
        /// <param name="heightmap"></param>
        /// <returns></returns>
        public static void GenerateVertexBufferWithTransformFlat(float[,] heightmap, out SlimDX.Direct3D11.Buffer vBuffer,
            out SlimDX.Direct3D11.Buffer iBuffer, Matrix transform, Vector2 initialGridPos, float gridLevelScale)
        {
            // Taille du buffer.
            int size = heightmap.GetLength(0) * heightmap.GetLength(1);


            // Création du vertex buffer contenant tous les vertex à dessiner.
            Vertex[] vertexBuffer = new Vertex[size];
            Noise.RidgedMultifractalNoise noise = new Noise.RidgedMultifractalNoise()
            {
                Frequency = 0.00400f,
                Lacunarity = 2.4f,
                OctaveCount = 2,
                Persistence = 0.9f,
                Quality = Noise.NoiseBase.NoiseQuality.QUALITY_FAST,
                Seed = 56549970
            };

            for (int y = 0; y < heightmap.GetLength(1); y++)
            {
                for (int x = 0; x < heightmap.GetLength(0); x++)
                {
                    Vector2 pos2D = new Vector2(x / ((float)heightmap.GetLength(0) - 1),
                                    (y / ((float)heightmap.GetLength(1) - 1)));


                    Vector4 pos = Vector4.Transform(new Vector4(pos2D.X, pos2D.Y, heightmap[x, y], 1.0f), transform);



                    vertexBuffer[(x + y * heightmap.GetLength(0))].Position = pos;
                    vertexBuffer[(x + y * heightmap.GetLength(0))].Texture = new Vector2(pos.X, pos.Y);
                    float texGen = noise.GetValue(pos.X, pos.Y, 0);
                    vertexBuffer[(x + y * heightmap.GetLength(0))].TextureId = texGen;


                }

            }
            //Thread.Sleep(1);
            // Index buffer contenant l'ordre dans lequel dessiner les vertex. (sous forme de carrés).
            int iBufferSize = (heightmap.GetLength(0) - 1) * (heightmap.GetLength(1) - 1) * 6;
            int[] indexBuffer = new int[iBufferSize];
            int sizeX = heightmap.GetLength(0);
            int sizeY = heightmap.GetLength(1);
            int startIndex = 0;
            for (int x = 0; x < sizeX - 1; x++)
            {
                for (int y = 0; y < sizeY - 1; y++)
                {
                    int firstIndex = x + y * (sizeX);
                    int topLeft = firstIndex;
                    int topRight = firstIndex + 1;
                    int lowerLeft = topLeft + sizeX;
                    int lowerRight = lowerLeft + 1;
                    // Triangle 1 (up right)
                    indexBuffer[startIndex++] = topLeft;
                    indexBuffer[startIndex++] = lowerRight;
                    indexBuffer[startIndex++] = lowerLeft;

                    // Triangle 2 (bottom left)
                    indexBuffer[startIndex++] = topLeft;
                    indexBuffer[startIndex++] = topRight;
                    indexBuffer[startIndex++] = lowerRight;
                }
            }
            //Thread.Sleep(1);
            // Calcule les normales aux surfaces.
            // Merci Riemer's XNA Tutorial :D
            for (int i = 0; i < vertexBuffer.Length; i++)
                vertexBuffer[i].Normal = new Vector4(0, 0, 0, 0);
            //Thread.Sleep(1);
            for (int i = 0; i < indexBuffer.Length / 3; i++)
            {
                Vector4 firstvec = vertexBuffer[indexBuffer[i * 3 + 1]].Position - vertexBuffer[indexBuffer[i * 3]].Position;
                Vector4 secondvec = vertexBuffer[indexBuffer[i * 3]].Position - vertexBuffer[indexBuffer[i * 3 + 2]].Position;
                Vector4 normal = new Vector4(Vector3.Cross(
                    new Vector3(firstvec.X, firstvec.Y, firstvec.Z),
                    new Vector3(secondvec.X, secondvec.Y, secondvec.Z)), 1.0f);
                normal.Normalize();
                vertexBuffer[indexBuffer[(i * 3)]].Normal += normal;
                vertexBuffer[indexBuffer[(i * 3 + 1)]].Normal += normal;
                vertexBuffer[indexBuffer[(i * 3 + 2)]].Normal += normal;
            }
            //Thread.Sleep(1);
            for (int i = 0; i < vertexBuffer.Length; i++)
            {
                vertexBuffer[i].Normal.Z = vertexBuffer[i].Normal.Z;

                var v = Util.MathHelper.ReduceXYZ(vertexBuffer[i].Normal);
                v.Normalize();
                vertexBuffer[i].Normal = new Vector4(v, 1.0f);

            }
            //Thread.Sleep(1);


            DataStream vBuffStream = new DataStream(size * Vertex.Stride, true, true);
            vBuffStream.WriteRange<Vertex>(vertexBuffer);
            vBuffStream.Position = 0;
            //Thread.Sleep(1);
            DataStream iBuffStream = new DataStream(iBufferSize * sizeof(int), true, true);
            iBuffStream.WriteRange<int>(indexBuffer);
            iBuffStream.Position = 0;
            //Thread.Sleep(1);
            vBuffer = new SlimDX.Direct3D11.Buffer(Scene.GetGraphicsDevice(), vBuffStream, new BufferDescription()
            {
                BindFlags = BindFlags.VertexBuffer,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None,
                SizeInBytes = (int)vBuffStream.Length,
                Usage = ResourceUsage.Default
            });

            iBuffer = new SlimDX.Direct3D11.Buffer(Scene.GetGraphicsDevice(), iBuffStream, new BufferDescription()
            {
                BindFlags = BindFlags.IndexBuffer,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None,
                SizeInBytes = (int)iBuffStream.Length,
                Usage = ResourceUsage.Default
            });

            vBuffStream.Dispose();
            iBuffStream.Dispose();
        }
    }
}
