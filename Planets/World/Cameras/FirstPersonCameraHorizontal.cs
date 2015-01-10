// Copyright (C) 2013, 2014 Jacques Lucas, 
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

// The developer's email is jacquesATetudDOOOTinsa-toulouseDOOOTfr (for valid email, replace 
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
    public class FirstPersonCameraHorizontal
    {
        #region Variables
        /// <summary>
        /// Matrice "view" permettant la projection de l'image selon l'angle et la position de la caméra.
        /// </summary>
        Matrix m_view;
        /// <summary>
        /// Position de la caméra.
        /// </summary>
        Vector3 m_up = -Vector3.UnitZ; //L'axe Z du monde est situé vers le  bas
        Vector3 m_upRoll = -Vector3.UnitZ; //Axe "up" après rotation
        /// <summary>
        /// Cible de la caméra.
        /// </summary>
        Vector3 m_front = -Vector3.UnitX; //Repère de la caméra sens direct

        /// <summary>
        /// Position de la caméra.
        /// </summary>
        Vector3 m_position;
        /// <summary>
        /// Vaut vrai si View doit être mise à jour.
        /// </summary>
        bool m_needCompute = false;


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
            get { return m_upRoll; }
            set { m_upRoll = value; m_needCompute = true; }
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
        /// Obtient la matrice de projection de cette caméra.
        /// </summary>
        public Matrix Projection
        {
            get;
            set;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Crée une nouvelle instance de FirstPersonCamera.
        /// </summary>
        public FirstPersonCameraHorizontal()
        {
            // Projection
            float aspectRatio = (float)Scene.Instance.ResolutionWidth / (float)Scene.Instance.ResolutionHeight;
            float fov = (float)Math.PI / 4.0f * aspectRatio * 0.75f;//0.75f;//3 / 4;
            Projection = Matrix.PerspectiveFovRH(fov, aspectRatio, 1f, 80000f);
        }
        /// <summary>
        /// Calcule la matrice View actuellement vue par la caméra.
        /// </summary>
        /// <returns></returns>
        public Matrix ComputeView()
        {
            return Matrix.LookAtRH(m_position, m_position + m_front, m_upRoll);
        }
        /// <summary>
        /// Retourne la matrice "View" de la caméra.
        /// </summary>
        public Matrix View
        {
            get
            {
                if (m_needCompute)
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
            Matrix rotate = Matrix.RotationAxis(new Vector3(0, 0, 1), -1 * value);
            m_front = v3(Vector4.Normalize(Vector3.Transform(m_front, rotate)));
            m_up = v3(Vector4.Normalize(Vector3.Transform(m_up, rotate)));
            m_upRoll = v3(Vector4.Normalize(Vector3.Transform(m_upRoll, rotate)));
            m_needCompute = true;
        }

        /// <summary>
        /// Fait effectuer une rotation de la caméra autour de l'axe "Front".
        /// </summary>
        /// <param name="value"></param>
        public void RotateRoll(float value)
        {
            Matrix rotate = Matrix.RotationAxis(m_front, -value);
            m_upRoll = v3(Vector4.Normalize(Vector3.Transform(m_up, rotate)));

            m_needCompute = true;
        }

        /// <summary>
        /// Fait effectuer une rotation de la caméra de "haut en bas".
        /// </summary>
        /// <param name="value"></param>
        public void RotateUpDown(float value)
        {
            Vector3 side = Vector3.Cross(m_up, m_front);
            Matrix rotate = Matrix.RotationAxis(side, -1 * value); //Convention trigo
            m_front = v3(Vector4.Normalize(Vector3.Transform(m_front, rotate)));
            m_up = v3(Vector4.Normalize(Vector3.Transform(m_up, rotate)));
            m_upRoll = v3(Vector4.Normalize(Vector3.Transform(m_upRoll, rotate)));
            m_needCompute = true;
        }
        /// <summary>
        /// Avance ou recule de la valeur désirée.
        /// </summary>
        /// <param name="value"></param>
        public void MoveForward(float value)
        {
            m_position += Vector3.Normalize(m_front) * value;
            m_needCompute = true;
        }
        /// <summary>
        /// Avance ou recule de la valeur désirée.
        /// </summary>
        /// <param name="value"></param>
        public void MoveSide(float value)
        {
            Vector3 cameraSide = Vector3.Cross(m_up, m_front);
            m_position += Vector3.Normalize(cameraSide) * value;
            m_needCompute = true;
        }
        /// <summary>
        /// Monde ou descend de la valeur désirée.
        /// </summary>
        /// <param name="value"></param>
        public void MoveUp(float value)
        {
            m_position += Vector3.Normalize(m_up) * value;
        }
        #endregion

        Vector3 v3(Vector4 input)
        {
            return new Vector3(input.X, input.Y, input.Z);
        }
    }
}
