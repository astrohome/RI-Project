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
        Stati

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

        private void bXML_Click(object sender, EventArgs e)
        {

        }
    }
}
