using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace RIStats
{
    public class Statistics
    {
        /// <summary>
        /// Statistics by itself.
        /// </summary>
        public static readonly Dictionary<string, Dictionary<Int32, Int32>> GlobalStatistic = new Dictionary<String, Dictionary<Int32, Int32>>();

        public static Int32 DocumentsNumber { get; set; }

        public static Int32 FinishedNumber { get; set; }

        /// <summary>
        /// Current document. class used here to provide ability of using lock {} construction.
        /// </summary>
        private class CurrentDoc
        {
            public Int32 number;
        }
        private CurrentDoc _currentDocument = new CurrentDoc();

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
            FinishedNumber++;
            Console.WriteLine(FinishedNumber + " finished!");
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
                DocumentsNumber++;
            }
            else
            {
                //for Thread safety reason, we should lock
                lock (GlobalStatistic)
                {
                    var words = current.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    //forech trim symbols to get real words and make everything in lower register
                    foreach (var w in words.Select(word => word.Trim(new[] { ',', '.', ':', ';', '!', '?', '"', ')', '(', '\'' })).Select(w => w.ToLower()))
                    {
                        //if it was only symbol, continue;
                        if (w == "") continue;

                        //If there is such word in dictionary
                        if (GlobalStatistic.ContainsKey(w))
                        {
                            //check if there is current document in list
                            if (GlobalStatistic[w].ContainsKey(_currentDocument.number))
                            {
                                //if so, increment statistic for that document.
                                GlobalStatistic[w][_currentDocument.number]++;
                            }
                            else //or just add document and set 1 - first time this word appeared here
                            {
                                GlobalStatistic[w].Add(_currentDocument.number, 1);
                            }
                        }
                        else //we haven't got any statistics for this word -> create
                        {
                            GlobalStatistic.Add(w, new Dictionary<int, int> { { _currentDocument.number, 1 } });
                        }
                    }
                }
            }
        }
    }
}
