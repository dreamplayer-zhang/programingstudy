using RootTools.Comm;
using RootTools.Module;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RootTools.RFIDs
{
    public class RFID_Brooks
    {
        #region RS232
        RS232 m_rs232;
        void InitRS232()
        {
            m_rs232 = new RS232(p_id, m_log);
            m_rs232.OnReceive += M_rs232_OnReceive;
            m_rs232.p_bConnect = true;
        }

        string m_sRFID = ""; 
        private void M_rs232_OnReceive(string sRead)
        {
            m_sRFID = OnReceive(sRead);
            m_bOnRead = false; 
        }

        string OnReceive(string sRead)
        {
            if (sRead.Length <= 3 || sRead[0] != 'S') return "Invalid Head";
            char nLength = GetChar(sRead, 1);
            if (sRead.Length < nLength) return "Short Message";
            string sData = "";
            switch (sRead[3])
            {
                case 'x': 
                    for (int n = 7; n < 23; n += 2) sData += GetChar(sRead, n);
                    return sData;
                case 'e': return "Error Code : " + GetError(sRead[5]);
            }
            return "Invalid Protocol";
        }

        char GetChar(string sRead, int nIndex)
        {
            return (char)(16 * Hex2Dec(sRead[nIndex]) + Hex2Dec(sRead[nIndex + 1])); 
        }

        int Hex2Dec(char ch)
        {
            if ((ch >= '0') && (ch <= '9')) return ch - '0';
            if ((ch >= 'A') && (ch <= 'F')) return ch - 'A' + 10;
            if ((ch >= 'a') && (ch <= 'f')) return ch - 'a' + 10;
            return 0; 
        }

        string GetError(char cError)
        {
            switch (cError)
            {
                case '0': return "No Error";
                case '1': return "Auto Fail (automatic reading is not possible)";
                case '2': return "Ex Fail (read or write initiated from the host and/or other actions cannot be carried out)";
                case '3': return "Write Fail (data transfer to ther transponder not possible)";
                case '4': return "No Tag (no transponder or antenna is installed)";
                case '5': return "Invalid (invalid parameter or data)";
                case '6': return "Unknown (unknown error)";
                case '7': return "unconfig (the device is not configured)";
                case '8': return "Check (parity and/or checksum error)";
                case '9': return "void ackn (no valid acknowledge)";
                case 'A': return "lcoked (locked page cannot be written)";
                case ':': return "Msg Length (message too long or too short or message is not received completely)";
                case ';': return "Invalid (invalid command)";
                case 'B': return "No ackn (the message which has to be confirmed has been sent the maximum number of time)";
                case 'C': return "Bad Type (incorrect transponder type)";
            }
            return "Unknown"; 
        }
        #endregion

        #region Read Command
        bool m_bOnRead = false; 
        public string Read(out string sRead)
        {
            StopWatch sw = new StopWatch();
            m_bOnRead = true;
            sRead = ""; 
            try
            {
                m_rs232.Send(m_sReadCmd);
                while (m_bOnRead)
                {
                    Thread.Sleep(10);
                    if (sw.ElapsedMilliseconds > m_msRS232) return "Read Timeout";
                }
                sRead = m_sRFID;
                return "OK"; 
            }
            finally { m_bOnRead = false; }
        }

        string m_sReadCmd;
        void InitReadCmd()
        {
            m_sReadCmd = "S04X";
            m_sReadCmd += m_nReadID.ToString("X"); 
            m_sReadCmd += "98" + (char)0x0d;
            Int32 nXOR = 0;
            Int32 nAdd = 0;
            byte[] aCmd = Encoding.Default.GetBytes(m_sReadCmd);
            for (int n = 0; n < aCmd.Length; n++) nXOR ^= aCmd[n];
            for (int n = 0; n < aCmd.Length; n++) nAdd += aCmd[n];
            m_sReadCmd += GetHex(nXOR);
            m_sReadCmd += GetHex(nAdd);
        }

        string GetHex(int n)
        {
            n = n % 256; 
            return (n / 16).ToString("X") + (n % 16).ToString("X"); 
        }

        int m_nReadID = 0;
        int m_secRS232 = 2;
        int m_msRS232 = 2000;
        void RunTree(Tree tree)
        {
            m_nReadID = tree.Set(m_nReadID, m_nReadID, "Reader ID", "RFID Reader ID (0 ~ 15)");
            m_secRS232 = tree.Set(m_secRS232, m_secRS232, "RS232 Timeout", "RS232 Receive Timeout (sec)");
            m_msRS232 = 1000 * m_secRS232; 
            InitReadCmd(); 
        }
        #endregion

        public string p_id { get; set; }
        Log m_log; 
        public RFID_Brooks(string sModule)
        {
            p_id = sModule + ".RFID";
            m_log = LogView.GetLog(sModule); 
            InitRS232(); 
        }
    }
}
