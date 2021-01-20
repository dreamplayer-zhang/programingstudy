using System;
using System.Text;
using RootTools.Comm;
using RootTools.Module;
using RootTools;
using RootTools.Trees;
using System.Threading;
using RootTools.OHTNew;

namespace Root_EFEM.Module
{
    public class RFID_Brooks : ModuleBase, IRFID
    {
        const int m_nReaderID = 0; // Default Value
        const char m_cEndCharacter = (char)0x0D;
        RS232 m_rs232;
        public IHandler m_handle;
        ILoadport m_loadport;

        public RFID_Brooks(string sID, IEngineer engineer, ILoadport loadport)
        {
            p_id = sID;
            this.InitBase(sID, engineer);
            m_handle = engineer.ClassHandler();
            m_loadport = loadport;
        }

        public override void GetTools(bool bInit)
        {
            p_sInfo = m_toolBox.Get(ref m_rs232, this, "RS232");
            if (bInit)
            {
                m_rs232.OnReceive += M_rs232_OnReceive;
                m_rs232.p_bConnect = true;
            }
        }

        private void M_rs232_OnReceive(string sRead)
        {
            m_sRFID = ConvertMessage(sRead);
            m_bOnRead = false;
        }

        int m_secRS232 = 2;
        void RunTimeoutTree(Tree tree)
        {
            m_secRS232 = tree.Set(m_secRS232, m_secRS232, "RS232", "Timeout (sec)");
        }

        void RunTreeSetup(Tree tree)
        {
            RunTimeoutTree(tree.GetTree("Timeout", false));
        }

        public override void RunTree(Tree tree)
        {
            base.RunTree(tree);
            RunTreeSetup(tree.GetTree("Setup", false));
        }

        #region StateHome
        public override void ButtonHome()
        {
            base.ButtonHome();
        }
        public override string StateHome()
        {
            if (EQ.p_bSimulate)
            {
                p_eState = eState.Ready;
                return "OK";
            }
            p_sInfo = base.StateHome();
            p_eState = (p_sInfo == "OK") ? eState.Ready : eState.Error;
            return p_sInfo;
        }
        #endregion


        StopWatch m_swRead = new StopWatch();
        string m_sRFID = "";
        bool m_bOnRead = false;

        string _sReadID = "";
        public string m_sReadID 
        {
            get { return _sReadID; }
            set
            {
                if (_sReadID == value) return;
                _sReadID = value;
                OnPropertyChanged();
            }
        }

        bool _bReadID = false;
        public bool m_bReadID 
        {
            get { return _bReadID; }
            set
            {
                if (_bReadID == value) return;
                _bReadID = value;
                OnPropertyChanged();
            }
        }



        public string ReadRFID()
        {
            m_bReadID = false;
            m_bOnRead = true;
            m_swRead.Restart();
            string sCmd = "";
            //Package Header (Start + Length1 + Length2)
            sCmd += "S"; // Start
            sCmd += "0"; //Length1
            sCmd += "4"; //Length2

            //Message Structure (Command + Address + Information)
            //Information Depends on Command
            sCmd += "X"; // Command : ReadData
            sCmd += m_nReaderID.ToString(); // Address
            sCmd += "98"; // Information (98 : read more pages until end character or empty character, 01~17 : Read Page #)

            //End of Package (End + CheckSum1 + CheckSum2 + CheckSum3 + CheckSum4)
            sCmd += m_cEndCharacter;
            Int16 nXOR = 0;
            Int16 nAdd = 0;
            byte[] aCMD = Encoding.Default.GetBytes(sCmd);
            for (int n = 0; n < aCMD.Length; n++)
            {
                nXOR ^= aCMD[n];
            }
            for (int n = 0; n < aCMD.Length; n++)
            {
                nAdd += aCMD[n];
            }
            sCmd += (char)(nXOR >> 8); // CheckSum1
            sCmd += (char)(nXOR); // CheckSum2
            sCmd += (char)(nAdd >> 8); // CheckSum3
            sCmd += (char)(nAdd); // CheckSum4

            //m_serial.Write(sCmd);
            m_rs232.Send(sCmd);
            while (m_swRead.ElapsedMilliseconds < m_secRS232 * 1000)
            {
                if (m_bOnRead == false)
                {
                    m_sReadID = m_sRFID;
                    m_bReadID = true;
                    return "OK";
                }
                Thread.Sleep(20);
            }
            m_sReadID = "";
            return "RFID Read Fail : Timeout";
        }

        string ConvertMessage(string sBuffer)
        {
            int nLength, nLengthHigh, nLengthLow, nReaderID, nPage;
            string sData = "";
            if (sBuffer.Length <= 3 || sBuffer[0] != 'S')
            {
                return "NG";
            }
            nLengthHigh = Convert16To10(sBuffer[1]);
            nLengthLow = Convert16To10(sBuffer[2]);
            nLength = nLengthHigh * 16 + nLengthLow;
            if (sBuffer.Length < nLength)
            {
                return "NG";
            }
            switch (sBuffer[3])
            {
                case 'x':
                    nReaderID = Convert.ToInt32(sBuffer[4]);
                    nPage = Convert.ToInt32(sBuffer[5]) * 10 + Convert.ToInt32(sBuffer[6]);
                    for (int n = 7; n < 23; n += 2)
                    {
                        int nHigh = Convert16To10(sBuffer[n]);
                        int nLow = Convert16To10(sBuffer[n + 1]);
                        int nDecode = nHigh * 16 + nLow;
                        char cDecode = (char)nDecode;
                        if (cDecode == (char)m_cEndCharacter) break;
                        sData += cDecode;
                    }
                    break;
                case 'e':
                    nReaderID = Convert.ToInt32(sBuffer[4]);
                    string sErrorMessage = ConvertErrorMessage(sBuffer[5]);
                    //Log Error

                    break;
                default:
                    break;
            }
            return sData;
        }

        int Convert16To10(char ch)
        {
            int nDigit10 = -1;
            if (ch >= (char)'0' && ch <= (char)'9')
            {
                nDigit10 = (int)(ch - '0');
            }
            else if (ch >= (char)'A' && ch <= (char)'F')
            {
                nDigit10 = (int)(ch - 'A');
            }
            else if (ch >= (char)'a' && ch <= (char)'f')
            {
                nDigit10 = (int)(ch - 'a');
            }
            return nDigit10;
        }

        string ConvertErrorMessage(char cHex)
        {
            string sErrorMessage = "Can Not Find Error";
            switch (cHex)
            {
                case '0':
                    sErrorMessage = "No Error";
                    break;
                case '1':
                    sErrorMessage = "Auto Fail (automatic reading is not possible)";
                    break;
                case '2':
                    sErrorMessage = "Ex Fail (read or write initiated from the host and/or other actions cannot be carried out)";
                    break;
                case '3':
                    sErrorMessage = "Write Fail (data transfer to ther transponder not possible)";
                    break;
                case '4':
                    sErrorMessage = "No Tag (no transponder or antenna is installed)";
                    break;
                case '5':
                    sErrorMessage = "Invalid (invalid parameter or data)";
                    break;
                case '6':
                    sErrorMessage = "Unknown (unknown error)";
                    break;
                case '7':
                    sErrorMessage = "unconfig (the device is not configured)";
                    break;
                case '8':
                    sErrorMessage = "Check (parity and/or checksum error)";
                    break;
                case '9':
                    sErrorMessage = "void ackn (no valid acknowledge)";
                    break;
                case 'A':
                    sErrorMessage = "lcoked (locked page cannot be written)";
                    break;
                case ':':
                    sErrorMessage = "Msg Length (message too long or too short or message is not received completely)";
                    break;
                case ';':
                    sErrorMessage = "Invalid (invalid command)";
                    break;
                case 'B':
                    sErrorMessage = "No ackn (the message which has to be confirmed has been sent the maximum number of time)";
                    break;
                case 'C':
                    sErrorMessage = "Bad Type (incorrect transponder type)";
                    break;
            }
            return sErrorMessage;
        }



        #region ModuleRun
        ModuleRunBase _runReadID;
        public ModuleRunBase m_runReadID
        {
            get { return _runReadID; }
            set
            {
                _runReadID = value;
                OnPropertyChanged();
            }
        }
        protected override void InitModuleRuns()
        {
            _runReadID = AddModuleRunList(new Run_ReadRFID(this), true, "RFID Read");
        }

        public string ReadRFID(byte nCh, out string sRFID)
        {
            throw new NotImplementedException();
        }

        public class Run_ReadRFID : ModuleRunBase
        {
            RFID_Brooks m_module;
            public string m_sRFID = "";
            public Run_ReadRFID(RFID_Brooks module)
            {
                m_module = module;
                InitModuleRun(module);
            }
            bool m_bRFID = false;
            string m_sSimulCarrierID = "CarrierID";
            public override ModuleRunBase Clone()
            {
                Run_ReadRFID run = new Run_ReadRFID(m_module);
                run.m_bRFID = m_bRFID;
                run.m_sSimulCarrierID = m_sSimulCarrierID;
                return run;
            }
            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_bRFID = tree.Set(m_bRFID, m_bRFID, "Use", "Run ReadRFID", bVisible);
                m_sSimulCarrierID = tree.Set(m_sSimulCarrierID, m_sSimulCarrierID, "Simulation CarrierID", "CarrierID When p_bSimulation", bVisible && EQ.p_bSimulate);
            }

            public override string Run()
            {
                string sResult = "OK";
                if (EQ.p_bSimulate) m_module.m_sReadID = m_sSimulCarrierID;
                else
                {
                    if (m_bRFID)
                    {
                        sResult = m_module.ReadRFID();
                    }
                }
                if (sResult == "OK") m_module.m_loadport.p_infoCarrier.SendCarrierID(m_module.m_sReadID);
                return sResult;
            }
        }
        #endregion
    }
}
