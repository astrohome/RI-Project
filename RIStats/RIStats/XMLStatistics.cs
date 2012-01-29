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
        //                Doc               Path                Word   Tf
        public Dictionary<Int32, Dictionary<String, Dictionary<String, Int32>>> tf = new Dictionary<Int32, Dictionary<String, Dictionary<String, Int32>>>(); 

        public Dictionary<String, Int32> tag_df = new Dictionary<string, int>();
        public Dictionary<String, Int32> word_df = new Dictionary<string, int>();
        public List<String> words = new List<string>();
        public Dictionary<String, Int32> tags = new Dictionary<String, Int32>();

        private Dictionary<String, Dictionary<String, Int32>> tags_df = new Dictionary<string, Dictionary<string, int>>();

        public Dictionary<string, Int32> balises = new Dictionary<string, int>();
        public System.Collections.ArrayList al = new System.Collections.ArrayList();

        private Int32 _currentDocument;
        private String _currentPath;
        public Int32 DocumentsNumber = 0;

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
                                
                                if (!tags.ContainsKey(reader.Name)) tags.Add(reader.Name, 1);
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

                al.Clear();
                balises.Clear();
                DocumentsNumber++;
                Console.WriteLine("Finished " + DocumentsNumber);
            }
        }

        public void Proceedltn()
        {

        }

        private void ProceedLine(string line)
        {
            // converting the path into a string
            string path = "";
            for (int j = 0; j < al.Count; j++)
                path += "/" + al[j];

            _currentPath = path;

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

                    tf.Add(_currentDocument, new Dictionary<string,Dictionary<string,int>>());
                }
                else
                {
                    if (tf[_currentDocument].ContainsKey(_currentPath))
                    {
                        if (tf[_currentDocument][_currentPath].ContainsKey(w))
                        {
                            tf[_currentDocument][_currentPath][w]++;
                        }
                        else
                        {
                            tf[_currentDocument][_currentPath].Add(w, 1);
                        }
                    }
                    else
                    {
                        tf[_currentDocument].Add(_currentPath, new Dictionary<string, int>() { { w, 1 } });
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
