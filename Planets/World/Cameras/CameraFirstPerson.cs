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
namespace SimpleTriangle.World.Cameras
{
    /// <summary>
    /// Représente une caméra : l'objet qui "voit" ce qu'il y a à l'écran.
    /// </summary>
    public class FirstPersonCamera
    {
        #region Variables
        /// <summary>
        /// Matrice "view" permettant la projection de l'image selon l'angle et la position de la caméra.
        /// </summary>
        Matrix m_view;
        /// <summary>
        /// Position de la caméra.
        /// </summary>
        Vector3 m_up = -Vector3.UnitY;
        /// <summary>
        /// Cible de la caméra.
        /// </summary>
        Vector3 m_front = Vector3.UnitX;
        /// <summary>
        /// Position de la caméra.
        /// </summary>
        Vector3 m_position;
        /// <summary>
        /// Vaut vrai si View doit être mise à jour.
        /// </summary>
        bool m_needCompute = false;

        /// <summary>
        /// Obtient le frustum associé à cette caméra.
        /// </summary>
        public Util.Frustum Frustum
        {
            get;
            private set;
        }
        /// <summary>
        /// Obtient ou définit la position de la caméra.
        /// </summary>
        public Vector3 Position
        {
            get { return m_position; }
            set { m_position = value; m_needCompute = true; }
        }
        /// <summary>
        /// Obtient ou Modifie la direction "haut" de la caméra.
        /// </summary>
        public Vector3 Right
        {
            get { return Vector3.Cross(m_up, m_front); }
        }
        /// <summary>
        /// Obtient ou Modifie la direction "haut" de la caméra.
        /// </summary>
        public Vector3 Up
        {
            get { return m_up; }
            set { m_up = value; m_needCompute = true; }
        }
        /// <summary>
        /// Obtient ou modifie la direction "front" de la caméra.
        /// </summary>
        public Vector3 Front
        {
            get { return m_front; }
            set { m_front = value; m_needCompute = true; }
        }

        /// <summary>
        /// Obtient la matrice de projection de la caméra.
        /// </summary>
        public Matrix Projection
        {
            get;
            set;
        }
        #endregion

        #region Properties
        public FirstPersonCamera()
        {

        }
        /// <summary>
        /// Calcule la matrice View actuellement vue par la caméra.
        /// </summary>
        /// <returns></returns>
        public Matrix ComputeView()
        {
            if(Position.Length() < Planet.AtmosphereRadius)
            {
                // Projection
                float aspectRatio = (float)Scene.Instance.ResolutionWidth / (float)Scene.Instance.ResolutionHeight;
                float fov = (float)Math.PI / 4.0f * aspectRatio * 0.75f;//0.75f;//3 / 4;
                Projection = Matrix.PerspectiveFovRH(fov, aspectRatio, 0.001f, 10f);
            }
            else
            {
                // Projection
                float aspectRatio = (float)Scene.Instance.ResolutionWidth / (float)Scene.Instance.ResolutionHeight;
                float fov = (float)Math.PI / 4.0f * aspectRatio * 0.75f;//0.75f;//0.75f;//3 / 4;
                Projection = Matrix.PerspectiveFovRH(fov, aspectRatio, 0.1f, 100f);
            }
            var view = Matrix.LookAtRH(m_position, m_position + m_front, m_up);
            Frustum = new Util.Frustum(view * Projection);
            return view;
        }
        /// <summary>
        /// Retourne la matrice "View" de la caméra.
        /// </summary>
        public Matrix View
        {
            get
            {
                if(m_needCompute)
                {
                    m_view = ComputeView();
                    m_needCompute = false;
                }
                return m_view;
            }
        }
        /// <summary>
        /// Fait effectuer une rotation de la caméra autour de l'axe "UP".
        /// </summary>
        /// <param name="value"></param>
        public void RotateSide(float value)
        {
            m_front = Vector3.Normalize(
                Util.MathHelper.ReduceXYZ(
                Vector3.Transform(m_front, Matrix.RotationAxis(m_up, value))
                ));
            m_needCompute = true;

        }
        /// <summary>
        /// Fait effectuer une rotation de la caméra autour de l'axe "Front".
        /// </summary>
        /// <param name="value"></param>
        public void RotateRoll(float value)
        {
            m_up = Vector3.Normalize(Util.MathHelper.ReduceXYZ(Vector3.Transform(m_up,
                                    Matrix.RotationAxis(m_front, -value))));
            m_needCompute = true;
        }
        /// <summary>
        /// Fait effectuer une rotation de la caméra de "haut en bas".
        /// </summary>
        /// <param name="value"></param>
        public void RotateUpDown(float value)
        {
            Vector3 cameraSide = Vector3.Cross(m_up, m_front);
            m_front = Vector3.Normalize(Util.MathHelper.ReduceXYZ(Vector3.Transform(m_front, Matrix.RotationAxis(cameraSide, -value))));
            // Recalcule le vecteur "up" de la caméra par ce qu'on a bougé le front vers up et du coup
            // ils sont plus orthogonaux et quand on bouge trop ça fait de la merde.
            // Cette ligne là résoud des pb bizarres de caméra ; elle est bancale MAIS ça marche donc
            // OSEF.
            m_up = -Vector3.Normalize(Vector3.Cross(cameraSide, m_front));
            m_needCompute = true;
        }
        /// <summary>
        /// Avance ou recule de la valeur désirée.
        /// </summary>
        /// <param name="value"></param>
        public void MoveForward(float value)
        {
            Position += Vector3.Normalize(m_front) * value;
        }
        /// <summary>
        /// Avance ou recule de la valeur désirée.
        /// </summary>
        /// <param name="value"></param>
        public void MoveSide(float value)
        {
            Vector3 cameraSide = Vector3.Cross(m_up, m_front);
            Position += Vector3.Normalize(cameraSide) * value;
        }
        /// <summary>
        /// Monde ou descend de la valeur désirée.
        /// </summary>
        /// <param name="value"></param>
        public void MoveUp(float value)
        {
            Position += Vector3.Normalize(m_up) * value;
        }
        #endregion

    }
}
