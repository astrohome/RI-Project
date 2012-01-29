using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Text.RegularExpressions;

namespace RIStats
{
    public struct Data
    {
        public String path;
        public String word;
        public Int32 tf;
    }

    public class XMLStatistics
    {
        public Dictionary<Int32, List<Data>> tf = new Dictionary<int, List<Data>>(); 

        public Dictionary<String, Int32> tag_df = new Dictionary<string, int>();
        public Dictionary<String, Int32> word_df = new Dictionary<string, int>();
        public List<String> words = new List<string>();
        public List<String> tags = new List<string>();

        public Dictionary<string, Int32> balises = new Dictionary<string, int>();
        public System.Collections.ArrayList al = new System.Collections.ArrayList();

        private Int32 _currentDocument;
        private String _currentPath;

        public void Proceed(String url)
        {
            string[] filePaths = Directory.GetFiles(url, "*.xml");
            string last = "last";
            foreach (var file in filePaths)
            {
                _currentDocument = Convert.ToInt32(Path.GetFileNameWithoutExtension(file));
                
                XmlTextReader reader = new XmlTextReader(file);
                reader.XmlResolver = null;
                while(reader.Read())
                {
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element:
                            {
                                int k = 1;
                                last = reader.Name;
                                if (balises.ContainsKey(last))
                                    balises[last]++;
                                else
                                    balises.Add(last, 1);

                                k = balises[last];
                                last += "[" + k + "]";
                                al.Add(last);
                                _currentPath = last;
                                if (!tags.Contains(reader.Name)) tags.Add(reader.Name);
                            }
                            break;
                        case XmlNodeType.Text:
                            {
                                ProceedLine(reader.Value);
                            }
                            break;
                        case XmlNodeType.EndElement:
                            {
                                last = (al[al.Count - 1]).ToString();
                                string nom_balise = reader.Name;
                                if (last.Contains(nom_balise))
                                {

                                    al.RemoveAt(al.Count - 1);

                                }
                            }
                            break;
                    }
                }
            }
        }

        private void ProceedLine(string line)
        {
            var wordsline = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            //forech trim symbols to get real words and make everything in lower register
            foreach (var w in wordsline.Select(word => Regex.Replace(word, "[^a-zA-Z0-9_]+", "", RegexOptions.Compiled)).Select(w => w.ToLower()))
            {
                if (!words.Contains(w))
                {
                    words.Add(w);
                }

                if (!tf.ContainsKey(_currentDocument))
                {
                    tf.Add(_currentDocument, new List<Data>());
                }
                else
                {
                    foreach (var st in tf[_currentDocument])
                    {
                        if (st.path == _currentPath)
                        {
                        }
                    }
                }
            }
        }

        private void ProceedInternal(XmlNode node)
        {
            if (node.HasChildNodes)
                foreach (XmlNode nd in node.ChildNodes)
                    ProceedInternal(nd);
            else
            {
                switch (node.NodeType)
                {
                    case XmlNodeType.Element:
                        {
                            if (!tags.Contains(node.Name)) tags.Add(node.Name);
                        }
                        break;
                    case XmlNodeType.Text:
                        {
                            if (!words.Contains(node.Name)) words.Add(node.Name);
                        }
                        break;
                    case XmlNodeType.EndElement:
                        {
                            
                        }
                        break;
                }
            }
        }
    }
}
