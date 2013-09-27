#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Services\CommonServices\Accessibility\IPointerDeviceDriver.cs) is part of CiviKey. 
*  
* CiviKey is free software: you can redistribute it and/or modify 
* it under the terms of the GNU Lesser General Public License as published 
* by the Free Software Foundation, either version 3 of the License, or 
* (at your option) any later version. 
*  
* CiviKey is distributed in the hope that it will be useful, 
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
* GNU Lesser General Public License for more details. 
* You should have received a copy of the GNU Lesser General Public License 
* along with CiviKey.  If not, see <http://www.gnu.org/licenses/>. 
*  
* Copyright © 2007-2012, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

//****************************************************
// Author : Isaac Duplan (Duplan@intechinfo.fr)
// Date : 01-05-2008
//****************************************************
using System;
using CK.Plugin;

namespace CommonServices
{
    /// <summary>
    /// Interface which define a basic interraction with a pointer system
    /// </summary>
    public interface IPointerDeviceDriver : IDynamicService
    {
        /// <summary>
        /// Fired at each PointerDevice move
        /// </summary>
        event PointerDeviceEventHandler PointerMove;

        /// <summary>
        /// Fired when a Pointer Button is Down
        /// </summary>
        event PointerDeviceEventHandler PointerButtonDown;

        /// <summary>
        /// Fired when a Pointer Button is Up
        /// </summary>
        event PointerDeviceEventHandler PointerButtonUp;

        int CurrentPointerXLocation { get; }

        int CurrentPointerYLocation { get; }


        /// <summary>
        /// Moves the pointer to x,y screen coordinates.
        /// </summary>
        void MovePointer(int x, int y);

        ///// <summary>
        ///// Simulate a device button down
        ///// </summary>
        ///// <param name="btInfo">Button Informations, use DefaultButton to keep it generic</param>
        ///// <param name="extraInfo">Used to specify custom info about a XButton, look at the current implementation</param>
        //void SimulateButtonDown(ButtonInfo btInfo, string extraInfo);

        ///// <summary>
        ///// Simulate a device button up
        ///// </summary>
        ///// <param name="btInfo">Button Informations, use DefaultButton to keep it generic</param>
        ///// <param name="extraInfo">Used to specify custom info about a XButton, look at the current implementation of IPointerDeviceDriver</param>
        //void SimulateButtonUp(ButtonInfo btInfo, string extraInfo);

    }

    public delegate void PointerDeviceEventHandler(object sender, PointerDeviceEventArgs e);

    /// <summary>
    /// EventArgs which define basic parrameters for a PointerDevice action.
    /// </summary>
    public class PointerDeviceEventArgs : EventArgs
    {
        /// <summary>
        /// Screen x coordinate of the pointer.
        /// </summary>
        public readonly int X;

        /// <summary>
        /// Screen y coordinate of the pointer.
        /// </summary>
        public readonly int Y;

        // Current button used, if XButton look at ExtraInfo.
        public readonly ButtonInfo ButtonInfo;

        // If using an XButton, specify custom info about the button, Look at the current implementation of IPointerDeviceDriver
        public readonly string ExtraInfo;

        private bool _cancel = false;

        public PointerDeviceEventArgs(int x, int y, ButtonInfo buttonInfo, string extraInfo, InputSource source)
        {
            X = x;
            Y = y;
            ButtonInfo = buttonInfo;
            ExtraInfo = extraInfo;
            Source = source;
        }

        public InputSource Source { get; set; }

        /// <summary>
        /// It allows you to Cancel the current PointerDevice event, stoping the propagation to other applications.
        /// </summary>
        public bool Cancel
        {
            get { return _cancel; }
            set { _cancel = value; }
        }

    }

    /// <summary>
    /// Basic list of button, which should be able to hold all kind of DevicePointer.
    /// We purpose a single DefaultButton because most devices have at least 1 contactor
    /// Using the ButtonX with an ExtraInfo, we can hold a device pointer with no button limit.
    /// <remarks>
    /// MouseDriver will use DefaultButton for Left, XButton + ExtraInfo = "Right" for Right...
    /// In this case both Driver and Client Plugins must know the ExtraInfo signification.
    /// </remarks>
    /// </summary>
    public enum ButtonInfo
    {
        None = 0,
        DefaultButton = 1048576,
        XButton = 2097152
    }



}
