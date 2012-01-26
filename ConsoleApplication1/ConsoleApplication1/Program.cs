using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using System.Text;

namespace ConsoleApplication1
{
    class Program
    {
        public static readonly Dictionary<string, Dictionary<Int32, Dictionary<string, Int32>>> GlobalStatistic = new Dictionary<String, Dictionary<Int32, Dictionary<string, Int32>>>();
        public static System.Collections.ArrayList al = new System.Collections.ArrayList();
        public static Dictionary<string, Int32> balises = new Dictionary<string, int>();
        static TextWriter twr = new StreamWriter("tee.txt");


        private class CurrentDoc
        {
            public Int32 number;
        }
        private static CurrentDoc _currentDocument = new CurrentDoc();


        static void Main(string[] args)
        {
            String folder = "D:\\projects\\Sciences.RI2011.complete\\M2WI7Q_2011_12\\XML_Coll_MWI_withSem.tar\\coll_projet_M2_2011\\coll\\";
            ProceedXML(folder);
        }
        public static void ProceedXML(String folder)
        {
            
            int max = 19738249;
            String url;
            for (int i = 600; i < max; i++)
            {
                url=folder+i+".xml";
                if(File.Exists(url))
                {
                    //Console.WriteLine(i);
                    ProceedOneXML_File(url); 
                }
            }
        }
        public static void ProceedOneXML_File(String url)
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



        private static void ProceedOneLineXML(String line)
        {
            String current = line.Trim(); //trim spaces
            if (current.Length == 0) return; //it could be only spaces in line - skip.

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

    }
}
