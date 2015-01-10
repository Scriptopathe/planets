using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX;
using SlimDX.Direct3D11;
using SlimDX.DXGI;
using SlimDX.Windows;
using Device = SlimDX.Direct3D11.Device;
namespace SimpleTriangle.World.Graphics
{
    /// <summary>
    /// Représente un effet de base comportant une flopée d'options.
    /// </summary>
    public class WaterEffect
    {
        #region Variables
        /// <summary>
        /// Effet utilisé en interne.
        /// </summary>
        Effect m_effect;
        DirectionalLight m_directionalLight;
        PointLight m_pointLight;
        Vector3 m_eyePosition;
        InputLayout m_inputLayout;
        Device m_device;
        ShaderResourceView m_texture;
        ShaderResourceView m_texture2;
        ShaderResourceView m_texture3;
        #endregion

        #region Properties
        /// <summary>
        /// Obtient ou définit la position de la caméra (telle que vue par cet effet).
        /// </summary>
        public Vector3 EyePosition
        {
            get { return m_eyePosition; }
            set
            {
                m_eyePosition = value;
                m_effect.GetVariableByName("xEyePosW").AsVector().Set(value);
            }
        }
        /// <summary>
        /// Obtient ou définit la lumière directionnelle utilisée par cet effet.
        /// </summary>
        public DirectionalLight DirLight
        {
            get { return m_directionalLight; }
            set
            {
                m_directionalLight = value;
                var arr = Util.MarshalHelper.GetArray(m_directionalLight);
                var stream = new DataStream(arr, false, false);
                m_effect.GetVariableByName("xDirLight").SetRawValue(stream, arr.Length);
                stream.Dispose();
            }
        }

        /// <summary>
        /// Obtient ou définit la lumière ponctuelle utilisée par cet effet.
        /// </summary>
        public PointLight PtLight
        {
            get { return m_pointLight; }
            set
            {
                m_pointLight = value;
                var arr = Util.MarshalHelper.GetArray(m_pointLight);
                var stream = new DataStream(arr, false, false);
                m_effect.GetVariableByName("xPointLight").SetRawValue(stream, arr.Length);
                stream.Dispose();
            }
        }

        /// <summary>
        /// Obtient ou définit la texture utilisée pour le dessin.
        /// </summary>
        public ShaderResourceView Texture
        {
            get
            {
                return m_texture;
            }
            set
            {
                m_texture = value;
                m_effect.GetVariableByName("xTexture").AsResource().SetResource(value);
            }
        }

        /// <summary>
        /// Obtient ou définit la 2e texture utilisée pour le dessin.
        /// </summary>
        public ShaderResourceView Texture2
        {
            get
            {
                return m_texture2;
            }
            set
            {
                m_texture2 = value;
                m_effect.GetVariableByName("xTexture2").AsResource().SetResource(value);
            }
        }

        /// <summary>
        /// Obtient ou définit la 3e texture utilisée pour le dessin.
        /// </summary>
        public ShaderResourceView Texture3
        {
            get
            {
                return m_texture3;
            }
            set
            {
                m_texture3 = value;
                m_effect.GetVariableByName("xTexture3").AsResource().SetResource(value);
            }
        }
        #endregion

        /// <summary>
        /// Crée une nouvelle instance de BasicEffect.
        /// </summary>
        public WaterEffect(Device device)
        {
            m_effect = Ressources.EffectCache.Get("Shaders\\water_effect.fx");
            m_inputLayout = new InputLayout(
                device,
                m_effect.GetTechniqueByIndex(0).GetPassByIndex(0).Description.Signature,
                VertexPositionTextureNormal.LayoutElements);

            float Km = 0.0025f;
            float Kr = 0.0015f;
            float ESun = Planet.ESun;
            float fOuterRadius = Planet.AtmosphereRadius; // 50 
            float fInnerRadius = Planet.PlanetRadius; // 44
            float fScale = 1.0f / (fOuterRadius - fInnerRadius);
            float fScaleDepth = Planet.ScaleDepth;

            m_effect.GetVariableByName("v3InvWavelength").AsVector().Set(new Vector3(1.0f / (float)Math.Pow(0.650, 4),
                1.0f / (float)Math.Pow(0.570f, 4),
                1.0f / (float)Math.Pow(0.475f, 4)));
            m_effect.GetVariableByName("fOuterRadius").AsScalar().Set(fOuterRadius);
            m_effect.GetVariableByName("fOuterRadius2").AsScalar().Set(fOuterRadius * fOuterRadius);
            m_effect.GetVariableByName("fInnerRadius").AsScalar().Set(fInnerRadius);
            m_effect.GetVariableByName("fInnerRadius2").AsScalar().Set(fInnerRadius * fInnerRadius);
            m_effect.GetVariableByName("fKrESun").AsScalar().Set(Kr * ESun);
            m_effect.GetVariableByName("fKmESun").AsScalar().Set(Km * ESun);
            m_effect.GetVariableByName("fKr4PI").AsScalar().Set(Kr * 4.0f * (float)Math.PI);
            m_effect.GetVariableByName("fKm4PI").AsScalar().Set(Km * 4.0f * (float)Math.PI);
            m_effect.GetVariableByName("fScaleDepth").AsScalar().Set(fScaleDepth);
            m_effect.GetVariableByName("fInvScaleDepth").AsScalar().Set(1.0f / fScaleDepth);
            m_effect.GetVariableByName("fScale").AsScalar().Set(fScale);
            m_effect.GetVariableByName("fScaleOverScaleDepth").AsScalar().Set(fScale / fScaleDepth);
            m_effect.GetVariableByName("xFar").AsScalar().Set(100f);
            m_effect.GetVariableByName("xNear").AsScalar().Set(0.1f);




            m_device = device;
        }
        /// <summary>
        /// Applique cet effet.
        /// </summary>
        public void Apply(Matrix World, Matrix View, Matrix Projection, Material material)
        {
            m_device.ImmediateContext.InputAssembler.InputLayout = m_inputLayout;
            m_effect.GetVariableByName("xWorld").AsMatrix().SetMatrix(World);
            m_effect.GetVariableByName("xWorldViewProj").AsMatrix().SetMatrix(World * View * Projection);
            m_effect.GetVariableByName("xWorldInvTranspose").AsMatrix().SetMatrix(Matrix.Invert(Matrix.Transpose(World)));
            m_effect.GetVariableByName("xMaterial").SetRawValue(material.Bytes, (int)material.Bytes.Length);
            m_effect.GetVariableByName("xCameraPos").AsVector().Set(Scene.Instance.Camera.Position);
            float cameraHeight = Scene.Instance.Camera.Position.Length();
            m_effect.GetVariableByName("fCameraHeight").AsScalar().Set(cameraHeight);
            m_effect.GetVariableByName("fCameraHeight2").AsScalar().Set(cameraHeight * cameraHeight);
            m_effect.GetTechniqueByIndex(0).GetPassByIndex(0).Apply(m_device.ImmediateContext);


        }

    }
}
