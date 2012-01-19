/*
 * To change this template, choose Tools | Templates
 * and open the template in the editor.
 */
package tp;

import java.util.Hashtable;

/**
 *
 * @author FeLiX
 */
public class Statistic {
            
    
    public static Hashtable<String, Integer> GlobalStatistic = new Hashtable<>();

    public static int DocumentsNumber;
    /// <summary>
        /// Proceed one line, analyze, and fill dictionary with statistics.
        /// </summary>
        /// <param name="line">Line in file to proceed.</param>
        private void ProceedOneLine(String line)
        {
            String current = line.trim(); //trim spaces
            if (current.length() == 0) return; //it could be only spaces in line - skip.

            //<doc><docno>12345</docno>
            //...
            if (line.charAt(0) == '<')
            {
                if (line.contains("</doc>")) return; //that's end of document

                int start = current.indexOf("no>") + 3; //find first occurence
                int end = current.indexOf("</do"); //last
                char[] number = new char[end - start]; //copy the number of document
                current.copyValueOf(number, start, number.length);
                int docno = Integer.parseInt(new String(number)); //and convert to Int32
                DocumentsNumber++;
            }
            else
            {
                    String[] words = current.split(" ");

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
}
