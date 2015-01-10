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
namespace SimpleTriangle.Ressources
{
    /// <summary>
    /// Représente un cache permettant le chargement et la gestion d'effets.
    /// </summary>
    public static class ShaderRessourceViewCache
    {
        /// <summary>
        /// Effets chargés et mis en cache.
        /// </summary>
        static Dictionary<string, ShaderResourceView> s_ressources = new Dictionary<string, ShaderResourceView>();

        public static ShaderResourceView Get(string key)
        {
            if (s_ressources.ContainsKey(key))
                return s_ressources[key];
            else
            {
                var rsc = ShaderResourceView.FromFile(Scene.GetGraphicsDevice(), key);
                s_ressources.Add(key, rsc);
                return rsc;
            }
         
        }
        
    }
}
