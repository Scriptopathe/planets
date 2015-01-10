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
using Buffer = SlimDX.Direct3D11.Buffer;
namespace SimpleTriangle.Debug.Renderers
{
    /// <summary>
    /// Provides a set of methods for the rendering BoundingBoxes.
    /// </summary>
    public static class BoundingBoxRenderer
    {
        #region Fields

        static Vector4[] verts = new Vector4[8];
        static int[] indices = new int[]
        {
            0, 1,
            1, 2,
            2, 3,
            3, 0,
            0, 4,
            1, 5,
            2, 6,
            3, 7,
            4, 5,
            5, 6,
            6, 7,
            7, 4,
        };

        static Effect effect;
        static Buffer vertexBuffer;
        static Buffer indexBuffer;
        static InputLayout inLayout;
        #endregion

        /// <summary>
        /// Renders the bounding box for debugging purposes.
        /// </summary>
        /// <param name="box">The box to render.</param>
        /// <param name="graphicsDevice">The graphics device to use when rendering.</param>
        /// <param name="view">The current view matrix.</param>
        /// <param name="projection">The current projection matrix.</param>
        /// <param name="color">The color to use for drawing the lines of the box.</param>
        public static void Render(
            BoundingBox box,
            Device graphicsDevice,
            Matrix world,
            Matrix view,
            Matrix projection,
            Color4 color)
        {
            if (effect == null)
            {
                effect = Ressources.EffectCache.Get("Shaders\\basic_effect.fx");
                vertexBuffer = new Buffer(Scene.GetGraphicsDevice(), 8 * sizeof(float) * 4, ResourceUsage.Dynamic, BindFlags.VertexBuffer, CpuAccessFlags.Write, ResourceOptionFlags.None, 0);
                /*
                 * 
            vBuffer = new SlimDX.Direct3D11.Buffer(Scene.GetGraphicsDevice(), vBuffStream, new BufferDescription()
            {
                BindFlags = BindFlags.VertexBuffer,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None,
                SizeInBytes = (int)vBuffStream.Length,
                Usage = ResourceUsage.Default
            });*/


                DataStream stream = new DataStream(sizeof(int) * indices.Length, false, true);
                stream.WriteRange<int>(indices);
                stream.Position = 0;
                indexBuffer = new SlimDX.Direct3D11.Buffer(Scene.GetGraphicsDevice(), stream, new BufferDescription()
                {
                    BindFlags = BindFlags.IndexBuffer,
                    CpuAccessFlags = CpuAccessFlags.None,
                    OptionFlags = ResourceOptionFlags.None,
                    SizeInBytes = (int)stream.Length,
                    Usage = ResourceUsage.Default
                });
                stream.Dispose();
                var sign = effect.GetTechniqueByIndex(0).GetPassByIndex(0).Description.Signature;
                inLayout = new InputLayout(
                    Scene.GetGraphicsDevice(),
                    effect.GetTechniqueByIndex(0).GetPassByIndex(0).Description.Signature, 
                    new InputElement[] {
                        new InputElement("POSITION", 0, Format.R32G32B32A32_Float, 0, 0) 
                    });
            }
            effect.GetVariableByName("xColor").AsVector().Set(color);

            Vector3[] corners = box.GetCorners();

            for (int i = 0; i < 8; i++)
            {
                verts[i] = new Vector4(corners[i], 1.0f);
            }

            var data = Scene.GetGraphicsDevice().ImmediateContext.MapSubresource(vertexBuffer, 0,  MapMode.WriteDiscard, SlimDX.Direct3D11.MapFlags.None);
            data.Data.WriteRange(verts);
            Scene.GetGraphicsDevice().ImmediateContext.UnmapSubresource(vertexBuffer, 0);

            Scene.GetGraphicsDevice().ImmediateContext.InputAssembler.InputLayout = inLayout;
            Scene.GetGraphicsDevice().ImmediateContext.InputAssembler.SetIndexBuffer(indexBuffer, Format.R32_UInt, 0);
            Scene.GetGraphicsDevice().ImmediateContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(vertexBuffer, 16, 0));
            Scene.GetGraphicsDevice().ImmediateContext.InputAssembler.PrimitiveTopology = (PrimitiveTopology.LineList);

            effect.GetVariableByName("World").AsMatrix().SetMatrix(world);
            effect.GetVariableByName("View").AsMatrix().SetMatrix(view);
            effect.GetVariableByName("Projection").AsMatrix().SetMatrix(projection);

            for (int i = 0; i < effect.GetTechniqueByIndex(0).Description.PassCount; i++)
            {
                effect.GetTechniqueByIndex(0).GetPassByIndex(i).Apply(Scene.GetGraphicsDevice().ImmediateContext);
                Scene.GetGraphicsDevice().ImmediateContext.DrawIndexed(indexBuffer.Description.SizeInBytes / sizeof(int), 0, 0);
            }
        }
    }
}
