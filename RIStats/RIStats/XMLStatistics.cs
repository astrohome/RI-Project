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
        public Dictionary<String, Dictionary<String, Int32>> word_df = new Dictionary<String, Dictionary<String, Int32>>();
        public List<String> words = new List<string>();
        public Dictionary<String, Int32> tagsN = new Dictionary<String, Int32>();

        private Dictionary<String, Dictionary<String, Int32>> tags_df = new Dictionary<string, Dictionary<string, int>>();

        public readonly Dictionary<Int32, Dictionary<String, Double>> docsltn = new Dictionary<Int32, Dictionary<String, Double>>();
        public readonly Dictionary<Int32, Dictionary<String, Double>> docsbm25 = new Dictionary<int, Dictionary<string, double>>();
        public readonly Dictionary<Int32, Dictionary<String, Int32>> dl = new Dictionary<int, Dictionary<string, int>>();

        public Dictionary<string, Int32> balises = new Dictionary<string, int>();
        public System.Collections.ArrayList al = new System.Collections.ArrayList();

        private Double _avdl = 0;
        public Double avdl
        {
            get
            {
                if (_avdl == 0)
                {
                    Int32 count = 0;
                    foreach (var item in dl)
                    {
                        foreach(var p in item.Value)
                            count += p.Value;
                    }
                    return _avdl = count / N;
                }
                return _avdl;
            }
        }

        public Int32 N { get; set; }

        public Double K { get; set; }
        public Double B { get; set; }

        public List<string> requests = new List<string>();

        private Int32 _currentDocument;
        private String _currentPath;

        private void CalulateDF()
        {
            foreach (var doc in tf)
            {
                foreach (var path in doc.Value)
                {
                    foreach (var w in path.Value)
                    {
                        if (!word_df.ContainsKey(w.Key))
                            word_df.Add(w.Key, new Dictionary<string, int>() { { GetLeaf(path.Key), 1 } });
                        else
                            if (word_df[w.Key].ContainsKey(GetLeaf(path.Key)))
                                word_df[w.Key][GetLeaf(path.Key)]++;
                            else word_df[w.Key].Add(GetLeaf(path.Key), 1);
                    }
                }
            }
        }

        private String GetLeaf(String path)
        {
            var t = path.Split(new char[] { '/' });
            return t[t.Length - 1].Remove(t[t.Length - 1].IndexOf('['));
        }

        private Int32 df(String word, String path)
        {
            return word_df[word][GetLeaf(path)];
        }

        public void Proceed(String url, Int32 count)
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
                                if (al.Count == 4) break;
                                al.Add(last);
                                _currentPath = last;

                                if (!tagsN.ContainsKey(reader.Name)) tagsN.Add(reader.Name, 1);
                                else tagsN[reader.Name]++;
                            }
                            break;
                        case XmlNodeType.Text:
                            {
                                ProceedLine(reader.Value);
                            }
                            break;
                        case XmlNodeType.EndElement:
                            {
                                if (al.Count == 0) break;

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
                N++;
                Console.WriteLine("Finished " + N);

                if (N == count) break;
            }
        }

        public void Proceedltn()
        {
            CalulateDF();
            foreach (var doc in tf)
            {
                foreach (var path in doc.Value)
                {
                    foreach (var t in path.Value)
                    {
                        if(requests.Contains(t.Key))
                        {
                            var df2 = df(t.Key, path.Key);
                            if(df2!=0)
                                docsltn[doc.Key][path.Key] += Math.Log10(1 + t.Value) * N / df2;

                            //dl[doc.Key][path.Key] += t.Value;
                        }
                    }
                }
            }
        }
        
        public void ProceedBM25_TF()
        {
            foreach (var doc in tf)
            {
                foreach (var path in doc.Value)
                {
                    foreach (var word in path.Value)
                    {
                        if (!requests.Contains(word.Key))
                            continue;

                        Double bm25 = (Double)(word.Value * (K + 1.0f)) / (K * ((1.0f - B) + B * dl[doc.Key][path.Key] / avdl) + word.Value) * Math.Log((N - df(word.Key, path.Key) + 0.5f) / (df(word.Key, path.Key) + 0.5f));

                        docsbm25[doc.Key][path.Key] += bm25;
                    }
                }
            }
        }

        private String GetParent(String path)
        {
            var nodes = path.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            return (nodes.Length != 0) ? path.Remove(path.IndexOf(nodes[nodes.Length - 1])) : "";
        }

        private void ProceedLine(string line)
        {
            // converting the path into a string
            string path = "";
            for (int j = 0; j < al.Count; j++)
                path += "/" + al[j];

            _currentPath = path;

            if (docsbm25.ContainsKey(_currentDocument))
                if (!docsbm25[_currentDocument].ContainsKey(_currentPath))
                    docsbm25[_currentDocument].Add(_currentPath, 0);
                else { }
            else docsbm25.Add(_currentDocument, new Dictionary<string, double>() { { _currentPath, 0 } });

            if (docsltn.ContainsKey(_currentDocument))
                if (!docsltn[_currentDocument].ContainsKey(_currentPath))
                    docsltn[_currentDocument].Add(_currentPath, 0);
                else { }
            else docsltn.Add(_currentDocument, new Dictionary<string, double>() { { _currentPath, 0 } });

            if (dl.ContainsKey(_currentDocument))
                if (dl[_currentDocument].ContainsKey(_currentPath))
                {
                    dl[_currentDocument][_currentPath]++;
                    String a = GetParent(_currentPath);
                    do
                    {
                        if (dl[_currentDocument].ContainsKey(a))
                            dl[_currentDocument][a]++;
                        else dl[_currentDocument].Add(a, 1);
                    } while (((a = GetParent(a)) != ""));                    
                }
                else
                    dl[_currentDocument].Add(_currentPath, 1);
            else dl.Add(_currentDocument, new Dictionary<string, int>() { { _currentPath, 1 } });

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
    }
}
