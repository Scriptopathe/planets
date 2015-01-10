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
using System.IO;
namespace SimpleTriangle.Ressources
{
    /// <summary>
    /// Représente un cache permettant le chargement et la gestion d'effets.
    /// </summary>
    public static class EffectCache
    {
        /// <summary>
        /// Effets chargés et mis en cache.
        /// </summary>
        static Dictionary<string, Effect> s_effects = new Dictionary<string,Effect>();

        public static Effect Get(string key)
        {
            if (s_effects.ContainsKey(key))
                return s_effects[key];
            else
            {
                ShaderBytecode bytecode = ShaderBytecode.CompileFromFile(key, "fx_5_0", ShaderFlags.None, EffectFlags.None, null, new IncludeFX());
                Effect e = new Effect(
                    Scene.GetGraphicsEngine().GraphicsDevice,
                    bytecode);
                s_effects.Add(key, e);
                return e;
            }
         
        }


        public class IncludeFX : Include
        {
            static string includeDirectory = ".\\";

            public void Close(Stream stream)
            {
                stream.Dispose();
            }

            public void Open(IncludeType type, string fileName, Stream parentStream, out Stream stream)
            {
                stream = new FileStream(includeDirectory + fileName, FileMode.Open);
            }
        }
    }
}
