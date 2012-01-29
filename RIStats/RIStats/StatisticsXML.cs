using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;

namespace RIStats
{
    class StatisticsXML
    {
        public readonly Dictionary<string, Dictionary<Int32, Dictionary<string, Int32>>> GlobalStatistic = new Dictionary<String, Dictionary<Int32, Dictionary<string, Int32>>>();
        public System.Collections.ArrayList al = new System.Collections.ArrayList();
        public Dictionary<string, Int32> balises = new Dictionary<string, int>();
        private static CurrentDoc _currentDocument = new CurrentDoc();
        public readonly Dictionary<String, Int32> df = new Dictionary<string, int>();
        public readonly Dictionary<Int32, Int32> dl = new Dictionary<Int32, Int32>();
        public readonly Dictionary<Int32, Double> docsltn = new Dictionary<Int32, Double>();
        public readonly Dictionary<Int32, Double> bm25_tf_finals = new Dictionary<Int32, Double>();
        public readonly Dictionary<String, Dictionary<Int32, Double>> bm25_tf = new Dictionary<String, Dictionary<Int32, Double>>();

        private class CurrentDoc
        {
            public Int32 number;
        }

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
                        count += item.Value;
                    }
                    return _avdl = count / N;
                }
                return _avdl;
            }
        }

        public Int32 N { get; set; }

        public Double K { get; set; }
        public Double B { get; set; }



        private Int32 doc_all(KeyValuePair<int, Dictionary<string, int>> doc)
        {
            int docvalue=0;
            Dictionary<string, Int32> paths = doc.Value;
            foreach (int val in paths.Values)
            {
                docvalue += val;
            }
            return docvalue;
        }

        private Int32 dfw(String word)
        {
            Dictionary<Int32, Dictionary<string, Int32>> temp = new Dictionary<Int32, Dictionary<string, Int32>>();
            GlobalStatistic.TryGetValue(word, out temp);
            return temp.Keys.Count;
        }

        private Int32 tf_td(String word, Int32 docno)
        {
            int val=0;
            Dictionary<string, Int32> paths = GlobalStatistic[word][docno];
            foreach(string path in paths.Keys)
            {
                val += paths[path];
            }
            return val;
        }

        public void Proceedltn()
        {
            foreach (var w in GlobalStatistic)
            {
                Int32 df2 = dfw(w.Key);
                df[w.Key] = df2;

                foreach (var doc in w.Value)
                {
                    double ltn = 0;
                    dl[doc.Key] = df2;
                    if (df2 != 0)
                        ltn = Math.Log10(1 + doc_all(doc)) * N / df2;

                    dl[doc.Key] += df2;

                    docsltn[doc.Key] += ltn;
                }
            }
        }


        public void ProceedBM25_TF()
        {
            foreach (var w in GlobalStatistic)
            {
                foreach (var doc in w.Value)
                {
                    Double bm25 = (Double)(doc_all(doc) * (K + 1.0f)) / (K * ((1.0f - B) + B * dl[doc.Key] / avdl) + doc_all(doc)) * Math.Log((N - df[w.Key] + 0.5f) / (df[w.Key] + 0.5f));

                    bm25_tf_finals[doc.Key] += bm25;

                    Dictionary<Int32, Double> dict = new Dictionary<int, double>();

                    if (bm25_tf.TryGetValue(w.Key, out dict))
                    {
                        if (bm25_tf[w.Key] == null)
                            bm25_tf[w.Key] = new Dictionary<int, double>() { { doc.Key, bm25 } };
                        else
                            bm25_tf[w.Key].Add(doc.Key, bm25);
                    }
                    else
                        bm25_tf.Add(w.Key, new Dictionary<int, double> { { doc.Key, bm25 } });
                }
            }
        }


        public void ProceedXML(String url)
        {
            XmlTextReader reader = new XmlTextReader(url);
            String[] delim = new String[2];
            string last = "last";
            delim[0] = "coll\\";
            delim[1]=".xml";
            String[] s = url.Split(delim,System.StringSplitOptions.None);
            Int32.TryParse(s[s.Length -2],out _currentDocument.number);
            bool tof = true;
            while (tof)

                {
                    try
                    {
                        tof = reader.Read();

                        if (reader.NodeType == XmlNodeType.Element && !(reader.NodeType == XmlNodeType.EndElement))
                        {
                            int k = 1;
                            last = reader.Name;
                            if (balises.ContainsKey(last))
                                balises[last]++;
                            else
                                balises.Add(last, 1);

                            k = balises[last];
                            last += "["+k+"]";
                            al.Add(last);
                            
                        }
                        if (reader.NodeType == XmlNodeType.EndElement)
                        {
                            last = (al[al.Count - 1]).ToString();
                            string nom_balise = reader.Name;
                            if (last.Contains(nom_balise))
                            {
                                
                                al.RemoveAt(al.Count - 1);

                            }
                        }
                        if (reader.NodeType == XmlNodeType.Text)
                        {
                            ProceedOneLineXML(reader.Value);

                        }
                    }
                    catch (Exception e)
                    {
                    }
                }
            reader.Close();
        }



        private void ProceedOneLineXML(String line)
        {
            String current = line.Trim(); //trim spaces
            if (current.Length == 0) return; //it could be only spaces in line - skip.



            lock (docsltn)
            {
                docsltn.Add(_currentDocument.number, 0);
                dl.Add(_currentDocument.number, 0);
                bm25_tf_finals.Add(_currentDocument.number, 0);
            }

            N++;
            //forech trim symbols to get real words and make everything in lower register
            lock (GlobalStatistic)
            {
                var words = current.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                Char[] symbols = new[] { ',', '.', ':', ';', '!', '?', '"', ')', '(', '\'','=','|','~','\"','-','<','>','[',']'};
                //forech trim symbols to get real words and make everything in lower register
                foreach (var w in words.Select(word => word.Trim(symbols)).Select(w => w.ToLower()))
                {
                    //if it was only symbol, continue;
                    if (w == "") continue;

                    // converting the path into a string
                    string path="";
                    for (int j = 0; j < al.Count; j++)
                        path += "/" + al[j];
                    
                    //If there is such word in dictionary
                    if (GlobalStatistic.ContainsKey(w))
                    {
                        //check if there is current document in list
                        if (GlobalStatistic[w].ContainsKey(_currentDocument.number))
                        {
                            //if so, check if the current tag (balise) is in list
                            if (GlobalStatistic[w][_currentDocument.number].ContainsKey(path))
                                (GlobalStatistic[w][_currentDocument.number])[path]++;

                            //increment statistic for that document.
                            else
                                GlobalStatistic[w][_currentDocument.number].Add(path, 1);
                        }
                        else //or just add document and set 1 - first time this word appeared here
                        {
                            GlobalStatistic[w].Add(_currentDocument.number,new Dictionary<string,int> { { path ,1} } );
                        }
                    }
                    else //we haven't got any statistics for this word -> create
                    {
                        GlobalStatistic.Add(w, new Dictionary<int , Dictionary<string,int>> { { _currentDocument.number, new Dictionary<string, int> { { path ,1 } } } });
                        
                    }
                }
            }
        }

        // this function return the intersection of paths
        public string get_path(string[] words, int docno)
        {
            string path = "/";
            string[] paths = new string[words.Length];
            Dictionary<Int32, Dictionary<string, Int32>> temp = new Dictionary<Int32, Dictionary<string, Int32>>();
            int k=0;
            Dictionary<string, string[]> w_path= new Dictionary<string, string[]>();
            foreach (string word in words)
            {
                temp = GlobalStatistic[word];
                w_path.Add(word, temp[docno].Keys.ToArray<string>());

            }
            path = best_path_in_doc(best_path_match(w_path));
            return path;
        }

        public string best_path_in_doc(string[] paths)
        {
            string path="/";
            for (int i = 0; i < paths.Length; i++)
            {
                if (paths[i].Length > path.Length)
                    path = paths[i];
            }
            return path;
        }

        // a tester
        public string[] best_path_match(Dictionary<string, string[]> dict)
        {
            if(dict.Count<2)
                return null;

            string[] keys= dict.Keys.ToArray();
            string[] resu=best_path_match_2_words(dict[keys[0]],dict[keys[1]]);
            for(int i=1;i<keys.Length;i++)
            {
                resu=best_path_match_2_words(resu,dict[keys[1]]);
            }
            return resu;
        }

        public string[] best_path_match_2_words(string[] paths1, string[] paths2)
        {
            int l1 = paths1.Length;
            int l2 = paths2.Length;
            string s1, s2;
            int cut;
            string[] temp = new string[l2];

            string[] resu = new string[paths1.Length];
            for (int i = 0; i < paths1.Length; i++)
            {


                for (int j = 0; j < paths2.Length; j++)
                {
                    s1 = paths1[i];
                    s2 = paths2[j];
                    if (s1.Length < s2.Length)
                    {
                        while (!s2.Contains(s1))
                        {
                            cut = last_index_of_slash(s1);
                            if (cut != -1)
                                s1 = s1.Substring(0, cut);
                            else
                                break;

                        }
                        temp[j] = s1;
                    }
                    else
                    {
                        while (!s1.Contains(s2))
                        {
                            cut = last_index_of_slash(s2);
                            if (cut != -1)
                                s2 = s2.Substring(0, cut);
                            else
                                break;
                        }

                        temp[j] = s2;
                    }
                }

                resu[i] = "/";

                for (int j = 0; j < temp.Length; j++)
                {
                    if (temp[j].Length > resu[i].Length)
                    {

                        resu[i] = temp[j];
                    }

                }
            }

            return resu;
        }

        public int last_index_of_slash(string s)
        {
            for (int i = s.Length - 1; i > 0; i--)
            {
                if (s[i] == '/')
                    return i;
            }
            return -1;

        }
    }
}