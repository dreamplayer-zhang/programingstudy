using RootTools;
using RootTools.Trees;
using System.Collections.ObjectModel;
using System.IO;

namespace Root_MarsLogView
{
    public class ListError : NotifyProperty
    {
        #region Error Data
        public class Error
        {
            public string p_sError { get; set; }
            public string p_sLog { get; set; }

            public Error(string sError, string sLog)
            {
                p_sError = sError;
                p_sLog = sLog; 
            }
        }

        public ObservableCollection<Error> p_aError { get; set; }
        public void Add(string sError, string sLog)
        {
            Error error = new Error(sError, sLog);
            p_aError.Add(error);
            if (p_aError.Count > m_maxView) p_aError.RemoveAt(0);
            WriteError(error); 
        }
        #endregion

        #region File
        void WriteError(Error error)
        {
            string sFile = GetFileName(error.p_sLog);
            StreamWriter sw = new StreamWriter(new FileStream(sFile, FileMode.Append));
            sw.WriteLine(error.p_sError + " :-> " + error.p_sLog);
            sw.Close();
        }

        string GetFileName(string sLog)
        {
            string sTime = sLog.Substring(0, 4) + sLog.Substring(5, 2) + sLog.Substring(8, 2);
            return m_mars.m_sFilePath + "\\Logs\\EventLog\\EventLog" + sTime + "_Error.txt";
        }
        #endregion

        int m_maxView = 250;
        public void RunTree(Tree tree)
        {
            m_maxView = tree.Set(m_maxView, m_maxView, "Max List", "FNC Max List View Count");
        }

        MarsLogViewer m_mars;
        public ListError(MarsLogViewer mars)
        {
            m_mars = mars;
            p_aError = new ObservableCollection<Error>();
        }
    }
}
