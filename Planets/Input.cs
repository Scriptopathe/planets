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
using SlimDX.DirectInput;
namespace SimpleTriangle
{
    public static class Input
    {
        static DirectInput directInput;
        static Keyboard keyboard;
        static Mouse mouse;
        static KeyboardState s_lastFrameState;
        static KeyboardState s_thisState;
        static MouseState s_lastFrameMouseState;
        static MouseState s_thisMouseState;
        public static MouseState GetMouseState()
        {
            return s_thisMouseState;
            
        }

        /// <summary>
        /// Initialize the input.
        /// </summary>
        public static void ModuleInit()
        {
            directInput = new DirectInput();
            keyboard = new Keyboard(directInput);
            keyboard.SetCooperativeLevel(Scene.Instance.GraphicsEngine.Form, CooperativeLevel.Nonexclusive | CooperativeLevel.Background);
            keyboard.Acquire();
            mouse = new Mouse(directInput);
            mouse.SetCooperativeLevel(Scene.Instance.GraphicsEngine.Form, CooperativeLevel.Nonexclusive | CooperativeLevel.Background);
            mouse.Acquire();


            s_lastFrameState = keyboard.GetCurrentState();
            s_thisState = s_lastFrameState;
            s_lastFrameMouseState = mouse.GetCurrentState();
            s_thisMouseState = s_lastFrameMouseState;
        }
        /// <summary>
        /// Updates the input.
        /// </summary>
        public static void Update()
        {
            s_lastFrameState = s_thisState;
            s_thisState = keyboard.GetCurrentState();


            s_lastFrameMouseState = s_thisMouseState;
            s_thisMouseState = mouse.GetCurrentState();
        }
        /// <summary>
        /// Checks for a trigger.
        /// </summary>
        public static bool IsTrigger(Key key)
        {
            return s_thisState.IsPressed(key) && !s_lastFrameState.IsReleased(key);
        }
        /// <summary>
        /// Checks if a key is pressed.
        /// </summary>
        public static bool IsPressed(Key key)
        {
            return s_thisState.IsPressed(key);
        }

        /// <summary>
        /// Retourne la position actuelle de la souris.
        /// </summary>
        /// <returns></returns>
        public static Vector2 GetMousePos()
        {
            return new Vector2(s_thisMouseState.X, s_thisMouseState.Y);
        }

        public static void SetMousePos(Vector2 pos)
        {

        }

        public static bool IsLeftClickTrigger()
        {
            return (s_thisMouseState.IsPressed(0)) && (s_lastFrameMouseState.IsReleased(0));
        }
        public static bool IsRightClickTrigger()
        {
            return (s_thisMouseState.IsPressed(1)) && (s_lastFrameMouseState.IsReleased(1));
        }
    }
}
