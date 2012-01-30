using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Xml;

namespace RIStats
{
    public class Statistics
    {
        /// <summary>
        /// Statistics by itself.
        /// </summary>
        public readonly Dictionary<String, Dictionary<Int32, Int32>> tf = new Dictionary<String, Dictionary<Int32, Int32>>();
        public readonly Dictionary<String, Dictionary<Int32, Double>> bm25_tf = new Dictionary<String, Dictionary<Int32, Double>>();
        public readonly Dictionary<Int32, Double> bm25_tf_finals = new Dictionary<Int32, Double>();
        public readonly Dictionary<Int32, Double> docsltn = new Dictionary<Int32, Double>();
        public readonly Dictionary<Int32, Int32> dl = new Dictionary<Int32, Int32>();
        public readonly Dictionary<String, Int32> df = new Dictionary<string, int>();

        public readonly List<String> requests = new List<string>();

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


        /// <summary>
        /// Current document. class used here to provide ability of using lock {} construction.
        /// </summary>
        private class CurrentDoc
        {
            public Int32 number;
        }
        private CurrentDoc _currentDocument = new CurrentDoc();

        private Int32 dfw(String word)
        {
            Dictionary<Int32, Int32> temp = new Dictionary<int,int>(); 
            tf.TryGetValue(word, out temp);
            return temp.Keys.Count;
        }

        private Int32 tf_td(String word, Int32 docno)
        {
            return tf[word][docno];
        }

        public void ProceedBM25_TF()
        {
            foreach (var w in tf)
            {
                if (!requests.Contains(w.Key.Trim(new char[] { '+' })))
                    continue;

                foreach (var doc in w.Value)
                {
                    Double bm25 = (Double) (doc.Value * (K + 1.0f)) / (K * ((1.0f - B) + B * dl[doc.Key] / avdl) + doc.Value) * Math.Log((N - df[w.Key] + 0.5f) / (df[w.Key] + 0.5f));

                    bm25_tf_finals[doc.Key] += bm25;

                    Dictionary<Int32, Double> dict = new Dictionary<int,double>();

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

        public void clean_n_clear()
        {
            foreach (var x in df.Keys.ToList())
            { df[x] = 0; }
            foreach (var x in dl.Keys.ToList())
            { dl[x] = 0; }
            foreach (var x in docsltn.Keys.ToList())
            { docsltn[x] = 0; }
            foreach (var x in bm25_tf_finals.Keys.ToList())
            { bm25_tf_finals[x] = 0; }
            bm25_tf.Clear();
        }

        public void Proceedltn()
        {
            foreach (var w in tf)
            {
                if (!requests.Contains(w.Key.Trim(new char[] { '+' })))
                    continue;

                Int32 df2 = dfw(w.Key);
                df[w.Key] = df2;
                
                foreach (var doc in w.Value)
                {
                    double ltn = 0;
                    
                    if(df2!=0)
                        ltn = Math.Log10(1 + doc.Value) * N / df2;

                    //dl[doc.Key] += doc.Value;
                    
                    docsltn[doc.Key] += ltn;
                    //Console.WriteLine("Word " + w.Key.ToString() + " in doc " + doc.Key + " has ltn: " + ltn);
                }
            }
        }

        public void ProceedXML(String url)
        {
            XmlTextReader reader = new XmlTextReader(url);
            String[] delim = new String[2];
            delim[0] = "coll\\";
            delim[1] = ".xml";
            String[] s = url.Split(delim, System.StringSplitOptions.None);
            Int32.TryParse(s[s.Length - 2], out _currentDocument.number);
            bool tof = true;
            while (tof)
            {
                try
                {
                    tof = reader.Read();
                    if (reader.NodeType == XmlNodeType.Text)
                    {
                        ProceedOneLineXML(reader.Value);
                    }
                }
                catch (Exception e) { }
            }
            reader.Close();
        }

        private void ProceedOneLineXML(String line)
        {
            String current = line.Trim(); //trim spaces
            if (current.Length == 0) return; //it could be only spaces in line - skip.

            //forech trim symbols to get real words and make everything in lower register
            lock (tf)
            {
                var words = current.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                //forech trim symbols to get real words and make everything in lower register
                foreach (var w in words.Select(word => word.Trim(new[] { ',', '.', ':', ';', '!', '?', '"', ')', '(', '\'', '=', '|', '~', '\"' })).Select(w => w.ToLower()))
                {
                    //if it was only symbol, continue;
                    if (w == "") continue;

                    //If there is such word in dictionary
                    if (tf.ContainsKey(w))
                    {
                        //check if there is current document in list
                        if (tf[w].ContainsKey(_currentDocument.number))
                        {
                            //if so, increment statistic for that document.
                            tf[w][_currentDocument.number]++;
                        }
                        else //or just add document and set 1 - first time this word appeared here
                        {
                            tf[w].Add(_currentDocument.number, 1);
                        }
                    }
                    else //we haven't got any statistics for this word -> create
                    {
                        tf.Add(w, new Dictionary<int, int> { { _currentDocument.number, 1 } });
                    }
                }
            }
        }

        /// <summary>
        /// Proceed the whole file, line-by-line
        /// </summary>
        /// <param name="url">URL of file - will be automaticaly converted to name of unGzipped file.</param>
        public void Proceed(String url)
        {
            Task task = Task.Factory.StartNew(() =>
            {
                StreamReader fs = new StreamReader(url);
                while (!fs.EndOfStream)
                {
                    ProceedOneLine(fs.ReadLine());
                }
                fs.Close();
            }) /*.ContinueWith(_ => ShowStats(GlobalStatistic))*/;
            task.Wait();
        }
        /// <summary>
        /// Proceed one line, analyze, and fill dictionary with statistics.
        /// </summary>
        /// <param name="line">Line in file to proceed.</param>
        private void ProceedOneLine(String line)
        {
            String current = line.Trim(); //trim spaces
            if (current.Length == 0) return; //it could be only spaces in line - skip.

            //<doc><docno>12345</docno>
            //...
            if (line[0] == '<')
            {
                if (line.Contains("</doc>")) return; //that's end of document

                int start = current.IndexOf("no>") + 3; //find first occurence
                int end = current.IndexOf("</do"); //last
                char[] number = new char[end - start]; //copy the number of document
                current.CopyTo(start, number, 0, number.Length);
                Int32.TryParse(new string(number), out _currentDocument.number); //and convert to Int32

                lock (docsltn)
                {
                    docsltn.Add(_currentDocument.number, 0);
                    dl.Add(_currentDocument.number, 0);
                    bm25_tf_finals.Add(_currentDocument.number, 0);
                }
              
                N++;
            }
            else
            {
                //for Thread safety reason, we should lock
                lock (tf)
                {
                    var words = current.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    //forech trim symbols to get real words and make everything in lower register
                    foreach (var w in words.Select(word => word.Trim(new[] { ',', '.', '-', ':', ';', '!', '?', '"', ')', '(', '\'', '`', '~', '%', '@', '^', '=' })).Select(w => w.ToLower()))
                    {

                        //if it was only symbol, continue;
                        if (w == "") continue;

                        double t = 0;

                        if (Double.TryParse(w, out t)) continue;

                        dl[_currentDocument.number]++;

                        //If there is such word in dictionary
                        if (tf.ContainsKey(w))
                        {
                            //check if there is current document in list
                            if (tf[w].ContainsKey(_currentDocument.number))
                            {
                                //if so, increment statistic for that document.
                                tf[w][_currentDocument.number]++;
                            }
                            else //or just add document and set 1 - first time this word appeared here
                            {
                                tf[w].Add(_currentDocument.number, 1);
                            }
                        }
                        else //we haven't got any statistics for this word -> create
                        {
                            tf.Add(w, new Dictionary<int, int> { { _currentDocument.number, 1 } });
                        }
                        int trash;
                        if(!df.TryGetValue(w, out trash))
                        df.Add(w, 0);
                    }
                }
            }
        }
    }
}
