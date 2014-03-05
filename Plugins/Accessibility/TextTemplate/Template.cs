using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CK.Core;

namespace TextTemplate
{
    public class Template
    {
        static readonly Regex ParseRegex = new Regex(String.Format("{0}(?<token>.*?){1}", TextTemplate.PlaceholderOpenTag, TextTemplate.PlaceholderCloseTag));
        
        string _stringToFormat;
        
        /// <summary>
        /// Get the word list
        /// </summary>
        public IReadOnlyList<IText> TextFragments { get; private set; }

        /// <summary>
        /// A public name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Return a new Template from the given string
        /// </summary>
        /// <param name="tmpl"></param>
        /// <returns></returns>
        public static Template Load(string tmpl, TextTemplate tt)
        {
            Template template = new Template();
            List<IText> textFragments = new List<IText>();
            template._stringToFormat = "";
            string staticText = "";
            IText text;
            var prevIndex = 0;
         
            Match m = ParseRegex.Match(tmpl);
            while(m.Success)
            {
                //Non editable text
                if (m.Index > 0) //false if the template start with an editable
                {
                    staticText = tmpl.Substring(prevIndex, m.Index - prevIndex);
                    ParseStaticText(staticText, textFragments, tt);
                }

                //Editable text
                text = textFragments.FirstOrDefault( x => x.IsEditable == true && x.Placeholder == m.Value );    //Search if the placeholder already exists 
                if(text == null) text = new Word(true, m.Groups["token"].Value, tt); //if not, create a new one
                text.Placeholder = m.Value;
                textFragments.Add(text);

                //escape curly braces to avoid conflicts during the string.Format
                template._stringToFormat += staticText.Replace("{", "{{").Replace("}", "}}") + "{" + (textFragments.IndexOf( text )) + "}";

                prevIndex = m.Index + m.Length;
                m = m.NextMatch();
            }

            if (prevIndex < tmpl.Length)
            {
                staticText = tmpl.Substring(prevIndex);
                ParseStaticText( staticText, textFragments, tt );
                //escape curly braces to avoid conflicts during the string.Format
                template._stringToFormat += staticText.Replace( "{", "{{" ).Replace( "}", "}}" );
            }
            template.TextFragments = new CKReadOnlyListOnIList<IText>(textFragments);

            return template;
        }
        static int IndexOfNewLine(string str)
        {
            int idx = str.IndexOf( "\r\n" );

            return idx == -1 ? str.IndexOf( "\n" ) : idx;
        }

        /// <summary>
        /// Parse the given static text and split it on spaces and new lines
        /// </summary>
        /// <param name="staticText"></param>
        /// <param name="textFragments"></param>
        /// <param name="tt"></param>
        static void ParseStaticText( string staticText, List<IText> textFragments, TextTemplate tt )
        {
            int space = staticText.IndexOf( ' ' );
            if( space > -1 )
            {
                var fragments = staticText.Split( new string[] { " " }, StringSplitOptions.None );
                for( int i = 0; i < fragments.Length; i++ )
                {
                    if( fragments[i].Length > 0 ) SplitNewlines( textFragments, fragments[i], tt );
                    if( i < fragments.Length - 1 ) //not add to the end to avoid duplicates whitespaces
                        textFragments.Add( new WhiteSpace() );
                }
            }
            else
            {
                SplitNewlines( textFragments, staticText, tt );
            }
        }
        
        /// <summary>
        /// Parse the given static text and split it on new lines
        /// </summary>
        /// <param name="textFragments"></param>
        /// <param name="staticText"></param>
        /// <param name="tt"></param>
        static void SplitNewlines(List<IText> textFragments, string staticText, TextTemplate tt)
        {
            IText text;

            int newLine = IndexOfNewLine( staticText );
            if (newLine > -1)
            {
                var newLineFragments = staticText.Split( new string[] { "\r\n", "\n" }, StringSplitOptions.None );
                for (int i = 0; i < newLineFragments.Length; i++)
                {
                    if (newLineFragments[i].Length > 0)
                    {
                        text = new Word(false, newLineFragments[i], tt);
                        textFragments.Add(text);
                    }
                    if (i < newLineFragments.Length - 1) //avoid duplicates new line
                        textFragments.Add(new NewLine());
                }
            }
            else
            {
                text = new Word(false, staticText, tt);
                textFragments.Add(text);
            }
        }

        /// <summary>
        /// Return the well formated string with the user values
        /// </summary>
        /// <returns></returns>
        public string GenerateFormatedString()
        {
            return String.Format(_stringToFormat, TextFragments.Distinct().Select(x => x.Text).ToArray());
        }
    }
}
