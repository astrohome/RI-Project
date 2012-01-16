using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace WikiStats
{
    public class Statistics
    {
        /// <summary>
        /// Size of buffer, used for unGzipping.
        /// </summary>
        public Int32 BUFFER_SIZE = 10000;
        /// <summary>
        /// Number of Mb to write to disk at a time.
        /// </summary>
        public Int32 WRITE_EVERY_MB = 20;

        public delegate void UpdateProgress(int percentage, String text);
        public event UpdateProgress Notification;

        public delegate void SetListBoxDataSource(Dictionary<String, String> list);
        public event SetListBoxDataSource SetDataSource;

        public static Int32 FinishedNumber { get; set; }

        /// <summary>
        /// Statistics by itself.
        /// </summary>
        public static readonly Dictionary<string, Dictionary<Int32, Int32>> GlobalStatistic = new Dictionary<String, Dictionary<Int32, Int32>>();

        public static Int32 DocumentsNumber { get; set; }

        /// <summary>
        /// Proceed the whole file, line-by-line
        /// </summary>
        /// <param name="url">URL of file - will be automaticaly converted to name of unGzipped file.</param>
        private void Proceed(String url)
        {
            Task task = Task.Factory.StartNew(() =>
                                                  {
                                                      StreamReader fs = new StreamReader(Path.GetFileName(url) + ".txt");
                                                      while (!fs.EndOfStream)
                                                      {
                                                          ProceedOneLine(fs.ReadLine());
                                                      }
                                                      fs.Close();
                                                  }) /*.ContinueWith(_ => ShowStats(GlobalStatistic))*/;
            task.Wait();
            FinishedNumber++;
            Console.WriteLine(FinishedNumber+" finished!");
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
                    var words = current.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);

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
                            GlobalStatistic.Add(w, new Dictionary<int, int> {{_currentDocument.number, 1}});
                        }
                    }
                }
            }
        }




        /// <summary>
        /// Current document. class used here to provide ability of using lock {} construction.
        /// </summary>
        private class CurrentDoc
        {
            public Int32 number;
        }
        private CurrentDoc _currentDocument = new CurrentDoc();

        /// <summary>
        /// Download list of files and set it as datasource for ListBox. Do not forget to subscribe to event SetDataSource!
        /// </summary>
        /// <param name="url">URL of site</param>
        public void GetFileList(String url)
        {
            Notification(0, "Starting downloading of files list...");

            //Create return value list
            var result = new Dictionary<String, String>();

            //Async task not to freeze UI
            Task task = Task.Factory.StartNew(() =>
                                                  {
                                                      //Request for proceed
                                                      var request = (HttpWebRequest) WebRequest.Create(url);

                                                      //Get response from site
                                                      using (var response = (HttpWebResponse) request.GetResponse())
                                                      {
                                                          //Using reader, we can extract needed information (URLs of files)
                                                          using (
                                                              var reader = new StreamReader(response.GetResponseStream())
                                                              )
                                                          {
                                                              var html = reader.ReadToEnd();
                                                              var m1 = Regex.Matches(html, @"(<a.*?>.*?</a>)",
                                                                                     RegexOptions.Singleline);
                                                              //If no matches - return empty response.
                                                              if (m1.Count <= 0) return;
                                                              //Go throught all "href"'s and get m2 - URL.
                                                              foreach (
                                                                  var m2 in
                                                                      from Match m in m1
                                                                      /* From all matches on the page */
                                                                      select m.Groups[1].Value
                                                                      into value /* select value */
                                                                      select
                                                                          Regex.Match(value, @"href=\""(.*?)\""",
                                                                                      /* and try to find 'href=' */
                                                                                      RegexOptions.Singleline)
                                                                      into m2 /* put result into m2 */
                                                                      where m2.Success /* if m2 fulfill requerements */
                                                                      select m2) /* select it */
                                                              {
                                                                  //Add name as primary key (will be visible) and URL to Value (not visible to user).
                                                                  result.Add(m2.Groups[1].Value,
                                                                             url + m2.Groups[1].Value);

                                                                  Notification(result.Count/m2.Captures.Count,
                                                                               "Starting downloading of files list...");
                                                              }
                                                          }
                                                      }
                                                  }).ContinueWith(
                                                      _ =>
                                                          {
                                                              Notification(0, "List of files successfully downloaded!");
                                                              SetDataSource(result);
                                                          });
        }

        /// <summary>
        /// UnGzipping the file to *.txt
        /// </summary>
        /// <param name="url"></param>
        private void UnZip(string url)
        {
            Console.WriteLine(Thread.CurrentThread.ManagedThreadId+" Thread started!");
            // Start a task - this runs on the background thread...
                 Task task = Task.Factory.StartNew(() =>
                 {
                     Uri uri = new Uri(url, UriKind.Absolute);

                     using (WebClient wc = new WebClient())
                     {
                         wc.DownloadProgressChanged += wc_DownloadProgressChanged;
                         wc.DownloadFileCompleted += wc_DownloadFileCompleted;
                         wc.DownloadFile(uri,
                                        Path.GetFileName(url));
                     }

                     // Will open the file to be decompressed
                     FileStream fsSource = new FileStream(Path.GetFileName(url), FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read);

                     // Will hold the compressed stream created from the destination stream
                     GZipStream gzDecompressed = new GZipStream(fsSource, CompressionMode.Decompress, true);

                     // Retrieve the size of the file from the compressed archive's footer
                     var bufferWrite = new byte[4];

                     //Go to last 4 bytes - it's size of archive
                     fsSource.Position = (int)fsSource.Length - 4;

                     // Write the first 4 bytes of data from the compressed file into the buffer
                     fsSource.Read(bufferWrite, 0, 4);

                     // Set the position back at the start
                     fsSource.Position = 0;

                     int totalLength = BitConverter.ToInt32(bufferWrite, 0);
                     byte[] buffer = new byte[BUFFER_SIZE];
                     int totalBytes = 0;

                     // Write the content of the buffer to the destination stream (file)
                     FileStream fsDest = new FileStream(Path.GetFileName(url) + ".txt", FileMode.Create);
                     int last=0;
                     // Loop through the compressed stream and put it into the buffer
                     while (true)
                     {

                         int bytesRead = gzDecompressed.Read(buffer, 0, BUFFER_SIZE);

                         // If we reached the end of the data
                         if (bytesRead == 0)
                             break;
                         
                         totalBytes += bytesRead;

                         Notification((int) (((double) totalBytes/totalLength)*100.0f),
                                      "Unzipped " + totalBytes + " bytes of total " + totalLength);
                         
                         fsDest.Write(buffer, 0, bytesRead);

                         //Write every 20 Mb.
                         if (totalBytes / (WRITE_EVERY_MB*1000000) > last)
                         {
                             fsDest.Flush(true);
                             //Console.WriteLine("Flushed!");
                         }
                         last = (totalBytes / (WRITE_EVERY_MB * 1000000));
                     }

                     fsDest.Flush(true);

                     // Close the streams
                     fsSource.Close();
                     gzDecompressed.Close();
                     fsDest.Close();

                     //Check the number of bytes read from header and actually read
                     if (totalBytes != totalLength)
                         Notification(0, "Error checking summ of bytes!");
                 })
                 
                
                .ContinueWith(_ =>
                                  {
                                      Notification(100, "Unzipping finished!");
                                      Proceed(url);
                                  });
             }

        /// <summary>
        /// Proceed analyzing of data for single file (archive).
        /// </summary>
        /// <param name="url"></param>
        public void Work(String url)
        {
            Notification(0, "UnGZipping the file...");
            UnZip(url);
        }

        void wc_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            Notification(100,"Download completed!");
        }

        void wc_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            Notification(e.ProgressPercentage,
                         "Downloaded " + e.BytesReceived + " bytes of total " + e.TotalBytesToReceive + " bytes");
        }
    }
}
