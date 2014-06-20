#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Advanced\ContextEditor\Wizard\BackupFeature\KeyboardBackup.cs) is part of CiviKey. 
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

using System;
using System.IO;
using CK.Keyboard.Model;

namespace KeyboardEditor
{

    /// <summary>
    /// Object that links an <see cref="IKeyboard"/> to the file which contains its XML backup.
    /// <see cref="BackUpFilePath"/> can be null of whitespace. In this case, the keyboard is a new one, it has no backup.
    /// </summary>
    public class KeyboardBackup
    {
        public string Name { get; private set; }

        /// <summary>
        /// Gets the IKeyboard that has been backed up.
        /// </summary>
        public IKeyboard BackedUpKeyboard { get; private set; }

        /// <summary>
        /// Get the path to the Backed up keyboard
        /// can be null of whitespace. In this case, the keyboard is a new one, it has no backup.
        /// </summary>
        public string BackUpFilePath { get; private set; }

        /// <summary>
        /// Gets whether the backup corresponds to a new keyboard (a backup that.. dosen't backup anything)
        /// </summary>
        public bool IsNew { get { return String.IsNullOrWhiteSpace( BackUpFilePath ); } }

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="backedUpKeyboard">The <see cref="IKeyboard"/> that has been backedup</param>
        /// <param name="backedUpFilePath">Th elinked back up file path</param>
        public KeyboardBackup( IKeyboard backedUpKeyboard, string backedUpFilePath )
        {
            if( backedUpKeyboard == null ) throw new ArgumentNullException( "backedUpKeyboard", "The backed up keyboard can't be null in the ctor of a KeyboardBackup object" );
            if( !String.IsNullOrWhiteSpace( backedUpFilePath ) && !File.Exists( backedUpFilePath ) ) throw new ArgumentException( "backedUpFilePath", "The keyboard's backup file path must be either null (in the case of a new keyboard) or be an existing file." );

            Name = backedUpKeyboard.Name;
            BackedUpKeyboard = backedUpKeyboard;
            BackUpFilePath = backedUpFilePath;
        }
    }
}
