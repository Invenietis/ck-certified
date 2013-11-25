using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CK.Core;

namespace TextTemplate
{
    public class Template
    {
        static readonly string OpenTag = "{{";
        static readonly string CloseTag = "}}";
        static readonly Regex ParseRegex = new Regex(String.Format("{0}(?<token>.*?){1}", OpenTag, CloseTag));
        
        string _stringToFormat;
        
        /// <summary>
        /// Get the word list
        /// </summary>
        public IReadOnlyList<IText> TextFragments { get; private set; }

        /// <summary>
        /// Return a new Template from the given string
        /// </summary>
        /// <param name="tmpl"></param>
        /// <returns></returns>
        public static Template Load(string tmpl)
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
                    int space = staticText.IndexOf(' ');
                    if ( space > -1)
                    {
                        var fragments = staticText.Split(new string[] { " " }, StringSplitOptions.None);
                        for (int i = 0; i < fragments.Length; i++ )
                        {
                            if (fragments[i].Length > 0 ) SplitNewlines(textFragments, fragments[i]);
                            if (i < fragments.Length - 1) //not add to the end to avoid duplicates whitespaces
                                textFragments.Add(new WhiteSpace());
                        }
                    }
                    else
                    {
                        SplitNewlines(textFragments, staticText);
                    }
                }

                //Editable text
                text = textFragments.SingleOrDefault(x => x.IsEditable == true && x.Placeholder == m.Value  );    //Search if there is an IText placeholder equals to the matched value
                if(text == null) text = new Word(true, m.Groups["token"].Value); //if not, create a new one
                text.Placeholder = m.Value;
                textFragments.Add(text);

                template._stringToFormat += staticText + "{" + (textFragments.IndexOf(text)) + "}";

                prevIndex = m.Index + m.Length;
                m = m.NextMatch();
            }

            if (prevIndex < tmpl.Length)
            {
                staticText = tmpl.Substring(prevIndex);
                text = new Word(false, staticText);
                textFragments.Add(text);
                template._stringToFormat += staticText;
            }
            template.TextFragments = new CKReadOnlyListOnIList<IText>(textFragments);

            return template;
        }

        static void SplitNewlines(List<IText> textFragments, string staticText)
        {
            IText text;

            int newLine = staticText.IndexOf(Environment.NewLine);
            if (newLine > -1)
            {
                var newLineFragments = staticText.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
                for (int i = 0; i < newLineFragments.Length; i++)
                {
                    if (newLineFragments[i].Length > 0)
                    {
                        text = new Word(false, newLineFragments[i]);
                        textFragments.Add(text);
                    }
                    if (i < newLineFragments.Length - 1) //avoid duplicates new line
                        textFragments.Add(new NewLine());
                }
            }
            else
            {
                text = new Word(false, staticText);
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
