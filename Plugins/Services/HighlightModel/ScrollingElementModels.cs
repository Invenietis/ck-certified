#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Services\HighlightModel\ScrollingElementModels.cs) is part of CiviKey. 
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Xml;
using CK.Storage;

namespace HighlightModel
{
    public class ScrollingElement : IStructuredSerializable
    {
        public ScrollingElement( string internalName, string displayName, ICommand command, string commandDescription )
        {
            InternalName = internalName;
            DisplayName = displayName;

            CommandDescription = commandDescription;
            Command = command;
        }

        public ScrollingElement()
        {
        }
        
        public string InternalName { get; set; }
        public string DisplayName { get; set; }

        public string CommandDescription { get; set; }
        public ICommand Command { get; set; }

        #region IStructuredSerializable Members

        public void ReadContent( IStructuredReader sr )
        {
            XmlReader r = sr.Xml;

            DisplayName = r["DisplayName"];
            InternalName = r["InternalName"];

            r.ReadStartElement( "Module" );
            r.ReadEndElement();
        }

        public void WriteContent( IStructuredWriter sw )
        {
            XmlWriter w = sw.Xml;
            w.WriteStartElement( "Module" );
            w.WriteAttributeString( "DisplayName", DisplayName );
            w.WriteAttributeString( "InternalName", InternalName );
            w.WriteFullEndElement();
        }

        #endregion
    }

    public class ScrollingElementConfiguration : List<ScrollingElement>, IStructuredSerializable
    {
        public ScrollingElementConfiguration()
            : base()
        {

        }

        public ScrollingElementConfiguration( IEnumerable<ScrollingElement> elements )
            : base()
        {
            foreach( var e in elements )
            {
                Add( e );
            }
        }

        #region IStructuredSerializable Members

        public void ReadContent( IStructuredReader sr )
        {
            XmlReader r = sr.Xml;
            r.Read();
            r.ReadStartElement( "DisabledModules" );
            while( r.IsStartElement( "Module" ) )
            {
                ScrollingElement e = new ScrollingElement();
                e.ReadContent( sr );
                Add( e );
            }

            r.ReadEndElement();
        }

        public void WriteContent( IStructuredWriter sw )
        {
            XmlWriter w = sw.Xml;

            w.WriteStartElement( "DisabledModules" );

            foreach( var scrollingElement in this )
            {
                scrollingElement.WriteContent( sw );
            }

            w.WriteFullEndElement();
        }

        #endregion
    }
}
