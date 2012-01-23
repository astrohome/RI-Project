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
            openFileDialog1.InitialDirectory = Path.GetDirectoryName(Application.ExecutablePath);
            openFileDialog1.Filter = "All files (*.*)|*.*";
            openFileDialog1.ShowDialog();
            Statistics stats = new Statistics();
            Task task = Task.Factory.StartNew(() =>
                 {
                     stats.Proceed(openFileDialog1.FileName);
                 }).ContinueWith(_ =>
            {
                MessageBox.Show(Statistics.docsltn.First() + " " + Statistics.docsltn.Last() + " " + Statistics.docsltn.Count + " ");
                stats.Proceedltn();
                StreamWriter sw = new StreamWriter("IzotovRoulyAllaoui_01_ltn_articles.txt", false);
                
                foreach (var doc in Statistics.docsltn)
                {
                    sw.WriteLine("Doc " + doc.Key.ToString() + " has score (ltn): " + doc.Value.ToString());
                }

                sw.Close();
                //_statistics_Statistics(Statistics.tf);
            });
        }

        
    }
}
