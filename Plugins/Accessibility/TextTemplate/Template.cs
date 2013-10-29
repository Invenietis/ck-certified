using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace TextTemplate
{
    public class Template
    {
        static readonly Regex ParseRegex = new Regex("{{(?<token>.*?)}}");
        
        string _stringToFormat;
        
        /// <summary>
        /// Get the list of the static text
        /// </summary>
        public List<string> TextData { get; private set; }

        /// <summary>
        /// Dictionary that contains user values
        /// </summary>
        public Dictionary<string, string> Placeholders { get; private set; }


        /// <summary>
        /// Return a new Template from the given string
        /// </summary>
        /// <param name="tmpl"></param>
        /// <returns></returns>
        public static Template Load(string tmpl)
        {
            Template t = new Template();
            t.Placeholders = new Dictionary<string, string>();
            t.TextData = new List<string>();
            t._stringToFormat = "";
            string staticText = "";

            var prevIndex = 0;
            var i = 0;

            Match m = ParseRegex.Match(tmpl);
            while(m.Success)
            {
                staticText = tmpl.Substring(prevIndex, m.Index - prevIndex);

                if( !t.Placeholders.ContainsKey(m.Value))
                    t.Placeholders.Add(m.Value, "");
                
                t._stringToFormat += staticText + "{" + IndexOfKey(t.Placeholders.Keys,m.Value)  + "}";
                t.TextData.Add(staticText);

                prevIndex = m.Index + m.Length;
                m = m.NextMatch();
                ++i;
            }
            if (prevIndex < tmpl.Length)
            {
                staticText = tmpl.Substring(prevIndex);
                t.TextData.Add(staticText);
                staticText += staticText;
            }

            return t;
        }
        
        static int IndexOfKey(IEnumerable<string> keys, string key)
        {
            int i = 0;
            foreach(string k in keys)
            {
                if (k == key) return i;
                ++i;
            }

            return -1;
        }

        /// <summary>
        /// Return the well formated string with the user values
        /// </summary>
        /// <returns></returns>
        public string GenerateFormatedString()
        {
            return String.Format(_stringToFormat, Placeholders.Keys.ToArray());
        }
    }

}
