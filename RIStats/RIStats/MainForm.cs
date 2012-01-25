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

        private void bSelectFile_Click(object sender, EventArgs e)
        {
            bSelectFile.Enabled = false;
            openFileDialog1.InitialDirectory = Path.GetDirectoryName(Application.ExecutablePath);
            openFileDialog1.Filter = "All files (*.*)|*.*";
            openFileDialog1.ShowDialog();
            Statistics stats = new Statistics();
            stats.B = 0.5;
            stats.K = 1;
            Task task = Task.Factory.StartNew(() =>
                 {
                     stats.Proceed(openFileDialog1.FileName);
                 }).ContinueWith(_ =>
            {
                //MessageBox.Show(stats.docsltn.First() + " " + stats.docsltn.Last() + " " + stats.docsltn.Count + " ");
                stats.Proceedltn();
                StreamWriter sw = new StreamWriter("Izotov_02_ltn_articles.txt", false);

                //2009011 Q0 3945065 21 73.61 OualidSamiYounessYassine /article[1]
                int rank = 0;
                foreach (var doc in stats.docsltn.OrderByDescending(x => x.Value))
                {
                    sw.WriteLine("2009011 Q0 " + doc.Key.ToString() + " " + rank + " " + Math.Round(doc.Value,2) + " ltn /article[1]");
                    rank++;
                }

                sw.Close();

                Task task2 = Task.Factory.StartNew(() =>
                 {
                     stats.ProceedBM25_TF();
                 }).ContinueWith(_2 =>
            {
                sw = new StreamWriter("Izotov_01_bm25_articles.txt", false);
                /*foreach (var word in stats.bm25_tf)
                {
                    foreach (var doc in stats.docsltn)
                    {
                        sw.Write(doc.Key + '\t');
                        foreach (var word_i in stats.bm25_tf)
                        {
                            foreach (var doc_i in word_i.Value)
                            {
                                if (doc_i.Key == doc.Key)
                                    sw.Write(doc_i.Value + '\t');
                                else sw.Write("0" + '\t');
                            }
                        }
                        sw.WriteLine("");
                    }

                }*/
                rank = 0;
                foreach (var doc in stats.bm25_tf_finals.OrderByDescending(x => x.Value))
                {
                    sw.WriteLine("2009011 Q0 " + doc.Key.ToString() + " " + rank + " " + Math.Round(doc.Value, 2) + " ltn /article[1]");
                    rank++;
                }
                sw.Close();
            });
                //_statistics_Statistics(Statistics.tf);
            });
        }

        
    }
}
