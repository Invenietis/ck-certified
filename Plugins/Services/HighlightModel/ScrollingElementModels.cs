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
