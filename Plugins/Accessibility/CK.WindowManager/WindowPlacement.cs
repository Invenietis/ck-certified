using System.Xml;
using CK.Storage;

namespace CK.WindowManager
{
    public class WindowPlacement : IStructuredSerializable
    {
        public string WindowName { get; private set; }
        public int Top { get; private set; }
        public int Left { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }

        public WindowPlacement()
        {
        }

        public WindowPlacement( string windowName, double top, double left, double width, double height )
        {
            Top = (int)top;
            Left = (int)left;
            Width = (int)width;
            Height = (int)height;
            WindowName = windowName;
        }

        #region IStructuredSerializable Members

        public void ReadContent( IStructuredReader sr )
        {
            XmlReader r = sr.Xml;

            r.Read();
            
            WindowName = r["Name"];
            Top = int.Parse( r["Top"] );
            Left = int.Parse( r["Left"] );
            Width = int.Parse( r["Width"] );
            Height = int.Parse( r["Height"] );

            r.ReadStartElement( "Placement" );
            r.ReadEndElement();
        }

        public void WriteContent( IStructuredWriter sw )
        {
            XmlWriter w = sw.Xml;
            w.WriteStartElement( "Placement" );
            w.WriteAttributeString( "Name", WindowName );
            w.WriteAttributeString( "Top", Top.ToString() );
            w.WriteAttributeString( "Left", Left.ToString() );
            w.WriteAttributeString( "Width", Width.ToString() );
            w.WriteAttributeString( "Height", Height.ToString() );
            w.WriteFullEndElement();
        }

        #endregion

    }
}
