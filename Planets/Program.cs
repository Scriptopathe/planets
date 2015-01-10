using System.Windows.Forms;
using System;
using SlimDX;
using SlimDX.D3DCompiler;
using SlimDX.Direct3D11;
using SlimDX.DXGI;
using SlimDX.Windows;
using Device = SlimDX.Direct3D11.Device;
using Resource = SlimDX.Direct3D11.Resource;

namespace SimpleTriangle
{
    static class Program
    {
        static void Main()
        {
            Scene scene = new Scene();
            System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
            watch.Start();
            GameTime time = new GameTime();
            MessagePump.Run(scene.GraphicsEngine.Form, () =>
            {
                watch.Reset();
                watch.Start();

                int frameTime = 1;
                if (time.LastFrameElapsedTime.TotalMilliseconds < frameTime)
                    System.Threading.Thread.Sleep((int)(frameTime - time.LastFrameElapsedTime.TotalMilliseconds));
                scene.Update(time);
                scene.Draw();

                watch.Stop();
                time.LastFrameElapsedTime = watch.Elapsed;
                time.TotalGameTime = time.TotalGameTime + watch.Elapsed;


            });
            scene.Dispose();

            // TODO ce soir
            // frustrum culling pour la planète.
            // backface culling pour le ground.
            // 
        }
    }
}
