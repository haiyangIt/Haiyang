using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Configuration;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;

namespace ExGrtAzure.Tests
{
    [TestClass]
    public class LogReadTest
    {


        private ICollection<string> GetFiles()
        {
            string filePrefixName = "20160512_";
            int index = 9;
            int endIndex = 85;
            string logFolder = @"E:\TestLog";
            bool isFileExist = true;
            List<string> files = new List<string>();
            for (index = 0; index < endIndex; index++)
            {
                var filePath = Path.Combine(logFolder, string.Format("{0}{1}.txt", filePrefixName, index++));
                if (File.Exists(filePath))
                    files.Add(filePath);
            }
            return files;
        }

        private ICollection<string> GetTraceFiles()
        {
            return new string[] { @"E:\TestLog\2016_05_12_19_47_22Trace.txt" };
        }
        private void ReadLogs(Action<string, string, int> findOperator)
        {
            var files = GetTraceFiles();

            foreach (var filePath in files)
            {

                int lineIndex = 0;
                using (StreamReader reader = new StreamReader(filePath))
                {
                    do
                    {
                        lineIndex++;
                        var line = reader.ReadLine();
                        findOperator(line, filePath, lineIndex);
                    } while (!reader.EndOfStream);
                }
            }
        }

        [TestMethod]
        public void ReadErrorLogs()
        {
            HashSet<string> fileNames = new HashSet<string>();
            ReadLogs((line, filePath, lineIndex) =>
            {
                if (line.IndexOf("\tE\t") > 0)
                {
                    if (!fileNames.Contains(filePath))
                    {
                        Debug.WriteLine("");
                        Debug.WriteLine(filePath);
                        Debug.WriteLine("");
                        fileNames.Add(filePath);
                    }
                    Debug.Indent();
                    Debug.Write(lineIndex);
                    Debug.Write(" : ");
                    Debug.WriteLine(line);
                    Debug.Unindent();
                }
            });
        }

        [TestMethod]
        public void ReadWarnLogs()
        {
            HashSet<string> fileNames = new HashSet<string>();
            ReadLogs((line, filePath, lineIndex) =>
            {
                if (line.IndexOf("\tW\t") > 0)
                {
                    if (!fileNames.Contains(filePath))
                    {
                        Debug.WriteLine("");
                        Debug.WriteLine(filePath);
                        Debug.WriteLine("");
                        fileNames.Add(filePath);
                    }
                    Debug.Indent();
                    Debug.Write(lineIndex);
                    Debug.Write(" : ");
                    Debug.WriteLine(line);
                    Debug.Unindent();
                }
            });
        }

        [TestMethod]
        public void GetMaxMinItemTime()
        {
            HashSet<string> fileNames = new HashSet<string>();
            double maxTime = 0;
            double minTime = 1000;
            var findStr = " end\tTotalTime:";
            int findStrLen = findStr.Length;
            string maxLine = string.Empty;
            string minLine = string.Empty;

            string maxFile = string.Empty;
            string minFile = string.Empty;
            int maxIndex = 0;
            int minIndex = 0;
            ReadLogs((line, filePath, lineIndex) =>
            {
                var index = line.IndexOf(findStr);
                if (index > 0)
                {
                    double result = 0;
                    var start = index + findStrLen;
                    if (double.TryParse(line.Substring(start, line.Length - start), out result))
                    {
                        if (maxTime < result)
                        {
                            maxTime = result;
                            maxFile = filePath;
                            maxIndex = lineIndex;
                            maxLine = line;
                        }
                        if (minTime > result)
                        {
                            minTime = result;
                            minFile = filePath;
                            minIndex = lineIndex;
                            minLine = line;
                        }
                    }
                }
            });

            Debug.WriteLine("maxTime:{0}, minTime:{1}", maxTime, minTime);
        }
    }
}
