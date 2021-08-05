using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace RootTools.Database
{
    class Database_ConnectSession
    {
        //string m_sServerName = "localhost";
        //string m_SDbName = "wind2"; // DB명
        //string m_sUid = "root";
        //string m_sPw = "root";
        public bool IsConnected
        {
            get => this.m_bConnected;
        }

        bool m_bConnected = false;
        int m_ThreadID;
        MySqlConnection m_sqlConnection;
        public Database_ConnectSession(int nThreadID, string sServerName = "localhost", string sDBName = "wind2", string sUid = "root", string sPw= "root")
        {
            try
            {
                m_ThreadID = nThreadID;
                string sConeectCommand = string.Format("SERVER={0};DATABASE={1};UID={2};PASSWORD={3};", sServerName, sDBName, sUid, sPw);
                m_sqlConnection = new MySqlConnection(sConeectCommand);
            }
            catch (Exception ex)
            {
                string sError = ex.Message;
            }
        }
        ~Database_ConnectSession()
		{
            if(m_sqlConnection!=null)
			{
                try
                {
                    m_sqlConnection.Close();
                }
                catch (Exception ex)
                {
                    TempLogger.Write("MYSQL", "m_sqlConnection Close Error : " + ex.Message);
                }
                try
                {
                    m_sqlConnection.Dispose();
                }
                catch (Exception ex)
                {
                    TempLogger.Write("MYSQL", "m_sqlConnection Dispose Error : " + ex.Message);
                }
                m_bConnected = false;
            }
        }

        public bool Connect()
        {
            try
            {
                m_sqlConnection.Open();
                if (m_sqlConnection.State == ConnectionState.Open)
                {
                    m_bConnected = true;
                }
            }
            catch (Exception)
            {
                // LOG
                m_bConnected = false;
            }

            return m_bConnected;
        }

        public bool GetConnectionState()
        {
            return m_bConnected;
        }
        public MySqlConnection GetConnection()
        {
            return m_sqlConnection;
        }

		internal void Disconnect()
		{
            if(m_sqlConnection!=null)
			{
                if(IsConnected)
				{
                    m_sqlConnection.Close();

                }
			}
		}
    }
}
