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
        /// Get the list of the TextFragment
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
            List<IText> textFragmenst = new List<IText>();
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
                    int newLine = staticText.IndexOf(Environment.NewLine);

                    if (newLine > -1)
                    {
                        var fragments = staticText.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                        for (int i = 0; i < fragments.Length; i++ )
                        {
                            //newLine at the begining
                            if (newLine == 0) textFragmenst.Add(new NewLine());

                            text = new TextFragment(false, fragments[i]);
                            textFragmenst.Add(text);

                            //newLine not at the begining
                            if (newLine > 0) textFragmenst.Add(new NewLine());
                        }
                    }
                    else
                    {
                        text = new TextFragment(false, staticText);
                        textFragmenst.Add(text);
                    }
                }

                //Editable text
                text = textFragmenst.SingleOrDefault(x => x.IsEditable == true && x.Placeholder == m.Value  );    //Search if there is an IText placeholder equals to the matched value
                if(text == null) text = new TextFragment(true, m.Groups["token"].Value); //if not, create a new one
                text.Placeholder = m.Value;
                textFragmenst.Add(text);

                template._stringToFormat += staticText + "{" + (textFragmenst.Count - 1) + "}";

                prevIndex = m.Index + m.Length;
                m = m.NextMatch();
            }

            if (prevIndex < tmpl.Length)
            {
                staticText = tmpl.Substring(prevIndex);
                text = new TextFragment(false, staticText);
                textFragmenst.Add(text);
                template._stringToFormat += staticText;
            }
            template.TextFragments = new CKReadOnlyListOnIList<IText>(textFragmenst);

            return template;
        }

        /// <summary>
        /// Return the well formated string with the user values
        /// </summary>
        /// <returns></returns>
        public string GenerateFormatedString()
        {
            return String.Format(_stringToFormat, TextFragments.Select(x => x.Text).ToArray());
        }
    }
}
