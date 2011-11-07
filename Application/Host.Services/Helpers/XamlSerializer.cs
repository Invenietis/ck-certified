using System.Windows.Markup;
using CK.Storage;
using System.Windows;
using System.Xml;

namespace Host.Services.Helper
{
    public class XamlSerializer<T> : IStructuredSerializer<T>
    {
        public object ReadInlineContent( IStructuredReader sr, T o )
        {
            return (T)XamlReader.Parse( sr.Xml.ReadInnerXml() );
        }

        public void WriteInlineContent( IStructuredWriter sw, T o )
        {
            XamlWriter.Save( o, sw.Xml );
        }
    }
}
