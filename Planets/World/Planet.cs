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
namespace SimpleTriangle.World
{
    /// <summary>
    /// Représente une planète.
    /// </summary>
    public class Planet
    {
        public const float PlanetRadius = 10;
        public const float PlanetRadiusDelta = 0.15f;
        public const float AtmosphereRadius = 10.25f;
        public const float ScaleDepth = 0.25f;
        public const float ESun = 10.0f; // 10.0f
        public const float AltitudeScale = 1/5000.0f;

        #region Variables
        QuadTreeCell m_mainCell;
        Objects.Atmosphere m_atmosphere;
        Objects.Ocean m_ocean;
        #endregion

        #region Methods
        /// <summary>
        /// Crée une nouvelle instance de Planet.
        /// </summary>
        public Planet()
        {
            LoadGraphics();
            // m_planet = new PlanetCell(Vector3.Zero, new Vector2(0, 0), 1.0f, new Vector3(40000, 40000, -20f), 45087027, false, 0);

            m_mainCell = new QuadTreeCell(Vector3.Zero, new Vector2(0, 0), 1.0f, new Vector3(40, 40, -0.020f), 45087027, 0, PlanetRadius);
            m_mainCell.Ressources.Add(new TerrainRessource(m_mainCell));
            //m_mainCell.Ressources.Add(new WaterRessource(m_mainCell));
            // m_mainCell.Ressources.Add(new AtmosphereRessource(m_mainCell));
            m_mainCell.InitializeRessources();

            m_atmosphere = new Objects.Atmosphere();
            m_ocean = new Objects.Ocean();
        }

        /// <summary>
        /// Charge les ressources graphiques associées à cet objet.
        /// </summary>
        public void LoadGraphics()
        {
            
            
        }
        /// <summary>
        /// Mets à jour la planète.
        /// </summary>
        public void Update(GameTime time)
        {
            m_mainCell.Update(time);
            m_atmosphere.Update();
            m_ocean.Update();
        }


        float test = 0.0f;
        /// <summary>
        /// Dessine la planète.
        /// </summary>
        public void Draw()
        {
            m_mainCell.Draw();
            m_atmosphere.Draw();
            m_ocean.Draw();
        }
        /// <summary>
        /// Libère les ressources non managées allouées par cet objet.
        /// </summary>
        public void Dispose()
        {
            m_mainCell.Dispose();
            m_atmosphere.Dispose();
            m_ocean.Dispose();
        }
        #endregion
    }
}
