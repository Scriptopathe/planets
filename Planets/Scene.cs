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
namespace SimpleTriangle
{
    /// <summary>
    /// Représente la scène qui sera exécutée.
    /// </summary>
    public class Scene
    {
        /* ---------------------------------------------------------------------------------
         * Static
         * -------------------------------------------------------------------------------*/
        #region Static
        /// <summary>
        /// Représente l'instance en cours d'exécution de la Scène.
        /// </summary>
        public static Scene Instance;
        public static World.Graphics.Engine GetGraphicsEngine() { return Instance.GraphicsEngine; }
        public static Device GetGraphicsDevice() { return Instance.GraphicsEngine.GraphicsDevice; }
        public int ResolutionWidth = 1200;
        public int ResolutionHeight = 800;
        #endregion
        /* ---------------------------------------------------------------------------------
         * Variables
         * -------------------------------------------------------------------------------*/
        #region Variables
        World.Graphics.Engine m_graphicsEngine;
        World.Planet m_landscape;
        World.Cameras.FirstPersonCamera m_camera;
        Util.ThreadPool m_threadPool;
        Debug.DiagnosisWindow m_diagWindow;
        Debug.Profiling.FPSCounter m_fpsCounter;
        #endregion

        /* ---------------------------------------------------------------------------------
         * Properties
         * -------------------------------------------------------------------------------*/
        #region Properties
        /// <summary>
        /// Obtient la fenêtre de diagnostic utilisée par l'application.
        /// </summary>
        public Debug.DiagnosisWindow DiagnosisWindow
        {
            get { return m_diagWindow; }
            private set { m_diagWindow = value; }
        }
        /// <summary>
        /// Obtient une référence vers le moteur graphique.
        /// </summary>
        public World.Graphics.Engine GraphicsEngine
        {
            get { return m_graphicsEngine; }
            private set { m_graphicsEngine = value; }
        }

        /// <summary>
        /// Obtient une référence vers l'objet représentant la planète.
        /// </summary>
        public World.Planet Planet
        {
            get { return m_landscape; }
            set { m_landscape = value; }
        }

        /// <summary>
        /// Obtient une référence vers la caméra actuelle.
        /// </summary>
        public World.Cameras.FirstPersonCamera Camera
        {
            get { return m_camera; }
            private set { m_camera = value; }
        }

        /// <summary>
        /// Obtient la pool de threads associée à cette scène.
        /// </summary>
        public Util.ThreadPool ThreadPool
        {
            get { return m_threadPool; }
            set { m_threadPool = value; }
        }

        /// <summary>
        /// Obtient le compteur de FPS associé à cette scène.
        /// </summary>
        public Debug.Profiling.FPSCounter FPSCounter
        {
            get { return m_fpsCounter; }
            private set { m_fpsCounter = value; }
        }
        #endregion
        /* ---------------------------------------------------------------------------------
         * Methods
         * -------------------------------------------------------------------------------*/
        #region Methods
        /// <summary>
        /// Crée l'instance unique de la scène.
        /// </summary>
        public Scene()
        {
            if (Instance != null)
                throw new InvalidOperationException();

            Instance = this;


            FPSCounter = new Debug.Profiling.FPSCounter();
            GraphicsEngine = new World.Graphics.Engine();
            ThreadPool = new Util.ThreadPool();
            Planet = new World.Planet();
            Camera = new World.Cameras.FirstPersonCamera();
            Camera.Position = new Vector3(20, -5, -20);
            Camera.RotateSide((float)Math.PI);

            // Positionnement des lumières.
            GraphicsEngine.LoadFX();
            Vector3 direction = new Vector3(0.57735f, -0.57735f, 0.57735f);
            direction.Normalize();
            GraphicsEngine.BasicEffect.DirLight = new World.Graphics.DirectionalLight()
            {
                Ambient = new Color4(0.2f, 0.2f, 0.2f),
                Diffuse = new Color4(0.8f, 0.8f, 0.8f),
                Specular = new Color4(0.5f, 0.5f, 0.5f),
                Direction = direction
                
            };
            GraphicsEngine.BasicEffect.PtLight = new World.Graphics.PointLight()
            {
                Ambient = new Color4(0.2f, 0.2f, 0.2f),
                Diffuse = new Color4(0.8f, 0.8f, 0.8f),
                Specular = new Color4(1f, 1f, 1f),
                Attenuation = new Vector3(0.1f, 0f, 0f),
                Range = 100f,
                Position = - GraphicsEngine.BasicEffect.DirLight.Direction * 1
            };
            GraphicsEngine.WaterEffect.DirLight = GraphicsEngine.BasicEffect.DirLight;
            GraphicsEngine.AtmosphereEffect.DirLight = GraphicsEngine.BasicEffect.DirLight;
            GraphicsEngine.BasicEffect.Texture = Ressources.ShaderRessourceViewCache.Get("Textures\\tex_3.jpg");
            GraphicsEngine.BasicEffect.Texture2 = Ressources.ShaderRessourceViewCache.Get("Textures\\tex_2.jpg");
            GraphicsEngine.BasicEffect.Texture3 = Ressources.ShaderRessourceViewCache.Get("Textures\\tex_1.jpg");
            GraphicsEngine.WaterEffect.Texture = Ressources.ShaderRessourceViewCache.Get("Textures\\tex_4.jpg");

            DiagnosisWindow = new Debug.DiagnosisWindow();
            DiagnosisWindow.Show();
            Input.ModuleInit();
        }

        /// <summary>
        /// Mets à jour la caméra contrôlée par la souris.
        /// </summary>
        /// <param name="time"></param>
        void UpdateMouseCamera(GameTime time)
        {
            /*
            float angleSpeed = 30;
            // Angle de la caméra.
            float delta = Math.Min(0.1f, 0.1f);
            Vector2 mouse = Input.GetMousePos();
            int centerX = Scene.Instance.ResolutionWidth / 2;
            int centerY = Scene.Instance.ResolutionHeight / 2;
            float cameraRotationX = -Microsoft.Xna.Framework.MathHelper.ToRadians((mouse.X - centerX) * angleSpeed * 0.01f);
            float cameraRotationY = Microsoft.Xna.Framework.MathHelper.ToRadians((mouse.Y - centerY) * angleSpeed * 0.01f); 
            */
            if (Input.IsPressed(SlimDX.DirectInput.Key.LeftShift))
            {
                float speed = 0.030f * time.LastFrameElapsedTime.Milliseconds / 25.0f;
                if (Input.IsPressed(SlimDX.DirectInput.Key.A))
                    Camera.RotateSide(speed); //cameraRotationX * angleSpeed * delta);
                if (Input.IsPressed(SlimDX.DirectInput.Key.S))
                    Camera.RotateUpDown(-speed); //-cameraRotationY * angleSpeed * delta);
                if (Input.IsPressed(SlimDX.DirectInput.Key.D))
                    Camera.RotateSide(-speed);//cameraRotationX * angleSpeed * delta);
                if (Input.IsPressed(SlimDX.DirectInput.Key.W))
                    Camera.RotateUpDown(speed); //-cameraRotationY * angleSpeed * delta);
            }
            else
            {
                float speed = 0.0005f * time.LastFrameElapsedTime.Milliseconds / 25.0f;
                if (Input.IsPressed(SlimDX.DirectInput.Key.Space))
                    speed *= 100;
                if (Input.IsPressed(SlimDX.DirectInput.Key.A))
                    Camera.MoveSide(speed);//cameraRotationX * angleSpeed * delta);
                if (Input.IsPressed(SlimDX.DirectInput.Key.S))
                    Camera.MoveForward(-speed); //-cameraRotationY * angleSpeed * delta);
                if (Input.IsPressed(SlimDX.DirectInput.Key.D))
                    Camera.MoveSide(-speed);//cameraRotationX * angleSpeed * delta);
                if (Input.IsPressed(SlimDX.DirectInput.Key.W))
                    Camera.MoveForward(speed); //-cameraRotationY * angleSpeed * delta);
            }
            /*GraphicsEngine.BasicEffect.PtLight = new World.Graphics.PointLight()
            {
                Ambient = new Color4(0.3f, 0.3f, 0.3f),
                Diffuse = new Color4(0.7f, 0.7f, 0.7f),
                Specular = new Color4(0.7f, 0.7f, 0.7f),
                Attenuation = new Vector3(0.1f, 0.01f, 0.0f),
                Range = 5000f,
                Position = Camera.Position
            };*/

            GraphicsEngine.WaterEffect.PtLight = GraphicsEngine.BasicEffect.PtLight;
            GraphicsEngine.BasicEffect.EyePosition = Camera.Position;
            GraphicsEngine.AtmosphereEffect.EyePosition = Camera.Position;
            GraphicsEngine.WaterEffect.EyePosition = Camera.Position;
            //GraphicsEngine.BasicEffect.PtLight.Position = Camera.Position;
        }
        
        /// <summary>
        /// Mets à jour les éléments de la scène.
        /// </summary>
        public void Update(GameTime time)
        {
            Input.Update();
            FPSCounter.AddFrame((float)time.LastFrameElapsedTime.TotalSeconds);
            DiagnosisWindow.Text = "Perf Diagnostic (" + FPSCounter.GetAverageFps().ToString() + " fps)";
            ThreadPool.Update();
            UpdateMouseCamera(time);
            Planet.Update(time);
        }


        /// <summary>
        /// Effectue le rendu de la scène.
        /// </summary>
        public void Draw()
        {
            Device device = GetGraphicsDevice();
            device.ImmediateContext.ClearRenderTargetView(GraphicsEngine.MainRenderTarget, new Color4(System.Drawing.Color.Black));
            device.ImmediateContext.ClearDepthStencilView(GraphicsEngine.MainStencilBuffer, DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, 1.0f, 0);
            Planet.Draw();
            GraphicsEngine.MainSwapChain.Present(0, PresentFlags.None);
        }

        /// <summary>
        /// Dispose les ressources non managées.
        /// </summary>
        public void Dispose()
        {
            GraphicsEngine.Dispose();
            Planet.Dispose();
            // ThreadPool.Dispose(); TODO
        }
        #endregion
    }
}
