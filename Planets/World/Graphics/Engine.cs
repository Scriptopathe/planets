using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SlimDX;
using SlimDX.D3DCompiler;
using SlimDX.Direct3D11;
using SlimDX.DXGI;
using SlimDX.Windows;
using Device = SlimDX.Direct3D11.Device;
using Resource = SlimDX.Direct3D11.Resource;

namespace SimpleTriangle.World.Graphics
{
    /// <summary>
    /// Moteur graphique.
    /// </summary>
    public class Engine
    {
        /* ---------------------------------------------------------------------------------
         * Variables
         * -------------------------------------------------------------------------------*/
        #region Variables
        Device m_device;
        SwapChain m_swapChain;
        RenderTargetView m_mainRenderTarget;
        RenderForm m_renderForm;
        Texture2D m_depthStencilBuffer;
        DepthStencilView m_depthStencilView;
        DirectionalLight m_dirLight;
        PointLight m_pointLight;
        #endregion

        /* ---------------------------------------------------------------------------------
         * Properties
         * -------------------------------------------------------------------------------*/
        #region Properties
        /// <summary>
        /// Obtient ou définit la lumière directionnelle utilisée par tous les effets.
        /// </summary>
        public DirectionalLight DirLight
        {
            get { return m_dirLight; }
            set
            {
                m_dirLight = value;
                BasicEffect.DirLight = value;
                WaterEffect.DirLight = value;
                AtmosphereEffect.DirLight = value;
            }
        }
        /// <summary>
        /// Obtient ou définit la lumière ponctuelle utilisée par tous les effets.
        /// </summary>
        public PointLight PointLight
        {
            get { return m_pointLight; }
            set
            {
                m_pointLight = value;
                BasicEffect.PtLight = value;
                WaterEffect.PtLight = value;
                AtmosphereEffect.PtLight = value;
            }
        }
        /// <summary>
        /// Obtient une instance de BasicEffect pouvant être utilisée par les objets de la scène.
        /// </summary>
        public BasicEffect BasicEffect
        {
            get;
            set;
        }
        /// <summary>
        /// Obtient une instance de AtmosphereEffect pouvant être utilisée par les objets de la scène.
        /// </summary>
        public AtmosphereEffect AtmosphereEffect
        {
            get;
            set;
        }
        /// <summary>
        /// Obtient une instance de WaterEffect permettant d'effectuer le rendu de l'eau.
        /// </summary>
        public WaterEffect WaterEffect
        {
            get;
            set;
        }

        /// <summary>
        /// Obtient une référence vers l'objet GraphicsDevice lié à ce moteur graphique.
        /// </summary>
        public Device GraphicsDevice
        {
            get { return m_device; }
        }

        /// <summary>
        /// Obtient une référence vers le render target principal.
        /// </summary>
        public RenderTargetView MainRenderTarget
        {
            get { return m_mainRenderTarget; }
        }
        /// <summary>
        /// Obtient une référence vers le depth stencil buffer associé au render target principal.
        /// </summary>
        public DepthStencilView MainStencilBuffer
        {
            get { return m_depthStencilView; }
        }

        /// <summary>
        /// Ontient une référence vers la fenêtre où est effectué le rendu.
        /// </summary>
        public RenderForm Form
        {
            get { return m_renderForm; }
        }

        /// <summary>
        /// Obtient une référence vers la swap chain principale.
        /// </summary>
        public SwapChain MainSwapChain
        {
            get { return m_swapChain; }
        }
        #endregion

        /* ---------------------------------------------------------------------------------
         * Methods
         * -------------------------------------------------------------------------------*/
        #region Methods
        /// <summary>
        /// Crée une nouvelle instance de Engine.
        /// </summary>
        public Engine()
        {
            InitializeGraphics();
        }

        /// <summary>
        /// Charge les effets qui doivent être initialisés après la création du moteur.
        /// </summary>
        public void LoadFX()
        {
            BasicEffect = new BasicEffect(Scene.GetGraphicsDevice());
            WaterEffect = new WaterEffect(Scene.GetGraphicsDevice());
            AtmosphereEffect = new AtmosphereEffect(Scene.GetGraphicsDevice());
        }
        /// <summary>
        /// Initialise les ressources graphiques de base.
        /// </summary>
        void InitializeGraphics()
        {
            var form = new RenderForm("Test SlimDX");
            form.ClientSize = new System.Drawing.Size(Scene.Instance.ResolutionWidth, Scene.Instance.ResolutionHeight);
            m_renderForm = form;
            var description = new SwapChainDescription()
            {
                BufferCount = 2,
                Usage = Usage.RenderTargetOutput,
                OutputHandle = form.Handle,
                IsWindowed = true,
                ModeDescription = new ModeDescription(0, 0, new Rational(60, 1), Format.R8G8B8A8_UNorm),
                SampleDescription = new SampleDescription(2, 0),
                Flags = SwapChainFlags.AllowModeSwitch,
                SwapEffect = SwapEffect.Discard
                
                
               
            };
            Device.CreateWithSwapChain(DriverType.Hardware, DeviceCreationFlags.None, description, out m_device, out m_swapChain);
            
            // Depth stencil buffer
            var depthStencilDesc = new Texture2DDescription()
            {
                Width = Scene.Instance.ResolutionWidth,
                Height = Scene.Instance.ResolutionHeight,
                MipLevels = 1,
                ArraySize = 1,
                Format = Format.D32_Float_S8X24_UInt,
                SampleDescription = new SampleDescription(2, 0),
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.DepthStencil,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None
                
            };
            m_depthStencilBuffer = new Texture2D(m_device, depthStencilDesc);
            m_depthStencilView = new DepthStencilView(m_device, m_depthStencilBuffer);

            // Création d'une vue sur le render target.
            using (var resource = Resource.FromSwapChain<Texture2D>(m_swapChain, 0))
                m_mainRenderTarget = new RenderTargetView(m_device, resource);

            // Création du viewport.
            var context = m_device.ImmediateContext;
            var viewport = new Viewport(0.0f, 0.0f, Scene.Instance.ResolutionWidth, Scene.Instance.ResolutionHeight);
            context.OutputMerger.SetTargets(m_depthStencilView, m_mainRenderTarget);
            context.Rasterizer.SetViewports(viewport);
            
            // Empêche le alt + enter de fonctionner.
            using (var factory = m_swapChain.GetParent<Factory>())
                factory.SetWindowAssociation(form.Handle, WindowAssociationFlags.IgnoreAltEnter);

            

        }

        #endregion

        #region Dispose
        /// <summary>
        /// Supprime les ressources non managées.
        /// </summary>
        public void Dispose()
        {
            m_device.Dispose();
            m_swapChain.Dispose();
        }
        #endregion
    }
}
