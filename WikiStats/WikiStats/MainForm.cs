using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WikiStats
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

                    if (word.Value.Count >= 1) //Here could be a filter of frequence of using the word.
                        //For example, to find words, which are used only once during all the documents.
                    {
                        Console.WriteLine(count + "=df(" + word.Key + ")");
                        foreach (var j in word.Value)
                        {
                            Console.WriteLine("     " + j.Key + " " + j.Value);
                        }
                    }
                }
            }
        }

        void _statistics_SetDataSource(Dictionary<string, string> list)
        {
            if (InvokeRequired)
            {
                Invoke(new Statistics.SetListBoxDataSource(_statistics_SetDataSource), new object[] { list });
            }
            else
            {
                if (list != null)
                {
                    lbFiles.DataSource = new BindingSource(list, null);
                    lbFiles.DisplayMember = "Key";
                    lbFiles.ValueMember = "Value";
                    trackBar1.Maximum = lbFiles.Items.Count;
                    bProceed.Enabled = true;
                }
            }
        }

        void _statistics_Notification(int percentage, string text)
        {
            if (InvokeRequired)
            {
                Invoke(new Statistics.UpdateProgress(_statistics_Notification), new object[] { percentage, text });
            }
            else
            {
                if (text != null)
                {
                    pbDownload.Value = percentage;
                    tssStatus.Text = text;
                }
            }
        }

        

        private void bList_Click(object sender, EventArgs e)
        {
            bList.Enabled = false;
            Statistics _statistics = new Statistics();
            _statistics.Notification += new Statistics.UpdateProgress(_statistics_Notification);
            _statistics.SetDataSource += new Statistics.SetListBoxDataSource(_statistics_SetDataSource);
            //_statistics.ShowStats += new Statistics.ShowStatistics(_statistics_Statistics);
            _statistics.GetFileList("http://www.emse.fr/~mbeig/ORI-2011/INEX-BASED-Wikipedia2009-COLLECTIONS/");
        }
        private void bProceed_Click(object sender, EventArgs e)
        {
            bProceed.Enabled = false;
            DateTime start = DateTime.Now;
            for (int i = 0; i < trackBar1.Value; i++)
            {
                Statistics _statistics = new Statistics();
                _statistics.Notification += new Statistics.UpdateProgress(_statistics_Notification);
                //_statistics.SetDataSource += new Statistics.SetListBoxDataSource(_statistics_SetDataSource);
                //_statistics.ShowStats += new Statistics.ShowStatistics(_statistics_Statistics);
                _statistics.Work(
                    (lbFiles.Items[i] is KeyValuePair<string, string>
                         ? (KeyValuePair<string, string>) lbFiles.Items[i]
                         : new KeyValuePair<string, string>()).Value);
            }

            int tasks = trackBar1.Value;

            Task f = Task.Factory.StartNew(() =>
                                               {
                                                   while (Statistics.FinishedNumber != tasks)
                                                   {
                                                       //Wait to complete a job.
                                                   }
                                               }).ContinueWith(_ =>
                                                                   {
                                                                       var time = (DateTime.Now - start).TotalSeconds;
                                                                       Console.WriteLine("Execution time: " + time);
                                                                       Console.WriteLine("Total amount of words: " + Statistics.GlobalStatistic.Count);
                                                                       Console.WriteLine("Total amount of documents: " + Statistics.DocumentsNumber);

                                                                       StreamWriter writer = new StreamWriter("stats.txt", true);
                                                                       writer.WriteLine(time.ToString() + "\t" +
                                                                                        Statistics.GlobalStatistic.Count +
                                                                                        "\t" +
                                                                                        Statistics.DocumentsNumber);
                                                                       writer.Close();

                                                                       if(MessageBox.Show("Would you like to print statistics?","Show statistics?",MessageBoxButtons.YesNo) == DialogResult.Yes)
                                                                        _statistics_Statistics(Statistics.GlobalStatistic);

                                                                       Statistics.GlobalStatistic.Clear();
                                                                       Statistics.FinishedNumber = 0;
                                                                       Statistics.DocumentsNumber = 0;
                                                                       bProceed.Enabled = true;
                                                                       
                                                                   });
        }
    }
}
