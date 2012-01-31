using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Threading.Tasks;
using System.Linq;

namespace RIStats
{
    public partial class MainForm : Form
    {
        private bool ltn = false;
        private bool bm25 = true;

        public MainForm()
        {
            InitializeComponent();
        }

        void _statistics_Statistics(Dictionary<string, Dictionary<int, int>> list)
        {
            lock (list)
            {
                foreach (var word in list)
                {
                    var count = word.Value.Sum(i => i.Value); //Global count of usage of concrete word.

                    if (word.Value.Count <= 1) //Here could be a filter of frequence of using the word.
                    //For example, to find words, which are used only once during all the documents.
                    {
                        SetText(count + "=df(" + word.Key + ")" + "\r\n");
                        foreach (var j in word.Value)
                        {
                            SetText("     " + j.Key + " " + j.Value + "\r\n");
                        }
                    }
                }
            }
        }

        // This delegate enables asynchronous calls for setting
        // the text property on a TextBox control.
        delegate void SetTextCallback(string text);

        private void SetText(string text)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (this.textBox1.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(SetText);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                this.textBox1.AppendText(text);
            }
        }
        Statistics stats = new Statistics();
        XMLStatistics statsXML = new XMLStatistics();

        private void bSelectFile_Click(object sender, EventArgs e)
        {
            bSelectFile.Enabled = false;
            openFileDialog1.InitialDirectory = Path.GetDirectoryName(Application.ExecutablePath);
            openFileDialog1.Filter = "All files (*.*)|*.*";
            openFileDialog1.ShowDialog();
            
            stats.B = 0.5;
            stats.K = 1;
            stats.requests.Clear();
            stats.requests.Add("olive");
            stats.requests.Add("oil");
            stats.requests.Add("health");
            stats.requests.Add("benefit");

            Task task = Task.Factory.StartNew(() =>
                 {
                     stats.Proceed(openFileDialog1.FileName);
                 }).ContinueWith(_ =>
            {
                stats.Proceedltn();
                stats.ProceedBM25_TF();
                WriteToFile("2009011");

                stats.clean_n_clear();

                stats.requests.Clear();
                stats.requests.Add("notting");
                stats.requests.Add("hill");
                stats.requests.Add("film");
                stats.requests.Add("actors");

                stats.Proceedltn();
                stats.ProceedBM25_TF();
                WriteToFile("2009036");

                stats.clean_n_clear();

                stats.requests.Clear();
                stats.requests.Add("probabilistic");
                stats.requests.Add("models");
                stats.requests.Add("in");
                stats.requests.Add("information");
                stats.requests.Add("retrieval");

                stats.Proceedltn();
                stats.ProceedBM25_TF();
                WriteToFile("2009067");

                stats.clean_n_clear();

                stats.requests.Clear();
                stats.requests.Add("web");
                stats.requests.Add("link");
                stats.requests.Add("network");
                stats.requests.Add("analysis");

                stats.Proceedltn();
                stats.ProceedBM25_TF();
                WriteToFile("2009073");

                stats.clean_n_clear();

                stats.requests.Clear();
                stats.requests.Add("web");
                stats.requests.Add("ranking");
                stats.requests.Add("scoring");
                stats.requests.Add("algorithm");

                stats.Proceedltn();
                stats.ProceedBM25_TF();
                WriteToFile("2009074");

                stats.clean_n_clear();

                stats.requests.Clear();
                stats.requests.Add("supervised");
                stats.requests.Add("machine");
                stats.requests.Add("learning");
                stats.requests.Add("algorithm");

                stats.Proceedltn();
                stats.ProceedBM25_TF();
                WriteToFile("2009078");

                stats.clean_n_clear();

                stats.requests.Clear();
                stats.requests.Add("operating");
                stats.requests.Add("system");
                stats.requests.Add("+mutual");
                stats.requests.Add("+exclusion");

                stats.Proceedltn();
                stats.ProceedBM25_TF();
                WriteToFile("2009085");
            });

            
        }

        public void WriteToFile(String request)
        {
            StreamWriter sw = new StreamWriter("Izotov_01_ltn_articles.txt", true);

            //2009011 Q0 3945065 21 73.61 OualidSamiYounessYassine /article[1]
            int rank = 0;
            foreach (var doc in stats.docsltn.OrderByDescending(x => x.Value))
            {
                sw.WriteLine(request+ " Q0 " + doc.Key.ToString() + " " + rank + " " + Math.Round(doc.Value, 2) + " ltn /article[1]");
                rank++;
                if (rank == 1500) break;
            }

            sw.Close();

            sw = new StreamWriter("Izotov_01_bm25_articles.txt", true);

            rank = 0;
            foreach (var doc in stats.bm25_tf_finals.OrderByDescending(x => x.Value))
            {
                sw.WriteLine(request+" Q0 " + doc.Key.ToString() + " " + rank + " " + Math.Round(doc.Value, 2) + " ltn /article[1]");
                rank++;
                if (rank == 1500) break;
            }

            sw.Close();
        }

        public void WriteToFileXML(String request)
        {
            StreamWriter sw;
            int rank;
            bool flag;
            bool flag2;
            List<string> paths = new List<string>();

            if (ltn)
            {
                sw = new StreamWriter("IlliaJeanNoelAmineBechar_02_ltn_XML.txt", true);

                //2009011 Q0 3945065 21 73.61 IlliaJeanNoelAmineBechar /article[1]
                rank = 0;

                Dictionary<Int32, Dictionary<String, Double>> newDocsltn = new Dictionary<Int32, Dictionary<String, Double>>();
                foreach (var d in statsXML.docsltn)
                {
                    var t = (from entry in d.Value orderby entry.Value descending select entry).ToDictionary(pair => pair.Key, pair => pair.Value);
                    newDocsltn.Add(d.Key, t);
                }

                flag = false;

                foreach (var doc in newDocsltn.OrderByDescending(x => x.Value.ToList()[0].Value))
                {
                    foreach (var path in doc.Value)
                    {
                        if (path.Value == 0) break;
                        sw.WriteLine(request + " Q0 " + doc.Key.ToString() + " " + rank + " " + Math.Round(path.Value, 2) + " ltn " + path.Key);
                        sw.Flush();
                        rank++;
                        if (rank == 1500)
                        {
                            flag = true;
                            break;
                        }
                    }

                    if (flag) break;
                }

                sw.Close();
            }
            if (bm25)
            {
                sw = new StreamWriter("IlliaJeanNoelAmineBechar_02_bm25_WT=2_XML.txt", true);

                rank = 0;
                Dictionary<Int32, Dictionary<String, Double>> newDocsBM25 = new Dictionary<Int32, Dictionary<String, Double>>();
                foreach (var d in statsXML.docsbm25)
                {
                    var t = (from entry in d.Value orderby entry.Value descending select entry).ToDictionary(pair => pair.Key, pair => pair.Value);
                    newDocsBM25.Add(d.Key, t);
                }

                flag = false;
                //var res = statsXML.docsbm25.Select(f => new KeyValuePair<int, Dictionary<string, double>>(f.Key, (from entry in f.Value orderby entry.Value ascending select entry).ToDictionary(pair => pair.Key, pair => pair.Value))).OrderByDescending(x => x.Value.ToList()[0].Value);
                foreach (var doc in newDocsBM25.OrderByDescending(x => x.Value.ToList()[0].Value))
                {
                    paths.Clear();
                    foreach (var path in doc.Value)
                    {
                        flag2 = false;
                        foreach (var path2 in paths)
                        {
                            if (path.Key.Contains(path2) || path2.Contains(path.Key))
                                flag2 = true;
                        }
                        
                        if (flag2) break;
                        else paths.Add(path.Key);

                        if (path.Value == 0) break;
                        sw.WriteLine(request + " Q0 " + doc.Key.ToString() + " " + rank + " " + Math.Round(path.Value, 2) + " bm25 " + path.Key);
                        rank++;
                        if (rank == 1500)
                        {
                            flag = true;
                            break;
                        }
                    }

                    if (flag) break;
                }

                sw.Close();
            }
        }

        /// <summary>
        /// Here proceeding of XML
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        private void bXML_Click(object sender, EventArgs e)
        {
            bXML.Enabled = false;
            using (FolderBrowserDialog dlg = new FolderBrowserDialog())
            {
                dlg.Description = "Select a folder";
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    Task task = Task.Factory.StartNew(() =>
                 {
                     statsXML.Proceed(dlg.SelectedPath, 200);
                     statsXML.B = 0.5;
                     statsXML.K = 1;
                 }).ContinueWith(_ =>
            {
                statsXML.requests.Clear();
                statsXML.requests.Add("olive");
                statsXML.requests.Add("oil");
                statsXML.requests.Add("health");
                statsXML.requests.Add("benefit");
                statsXML.Proceedltn();
                statsXML.ProceedBM25_TF();
                WriteToFileXML("2009011");
            });
                }
            }

            //statsXML.B = 0.5;
            //statsXML.K = 1;
            
        }
    }
}
