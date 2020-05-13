using NLog;
using System;

namespace RootTools
{
    public class Log
    {
        #region Write
        public void Info(string str)
        {
            m_logger.Info(str);
        }

        public void Warn(string str)
        {
            m_logger.Warn(str);
        }

        public void Error(string str)
        {
            m_logger.Error(str);
        }

        public void Error(string str, string sArgument)
        {
            m_logger.Error(str, sArgument);
        }

        public void Error(Exception e, string str)
        {
            m_logger.Error(e, str);
        }

        public void Fatal(string str)
        {
            m_logger.Fatal(str);
        }
        #endregion

        public Logger m_logger;
        public Log(Logger logger)
        {
            m_logger = logger; 
        }
    }
}
