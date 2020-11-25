using RootTools;
using RootTools.Comm;
using RootTools.GAFs;
using RootTools.Module;
using RootTools.Trees;
using System;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading;
using System.Windows.Media;

namespace Root_Vega.Module
{
    public class FDC : ModuleBase
    {
        #region ToolBox
        public RS232byte m_rs232;
        public override void GetTools(bool bInit)
        {
            p_sInfo = m_toolBox.Get(ref m_rs232, this, "RS232");
            if (bInit)
            {
                m_rs232.OnRecieve += M_rs232_OnRecieve; 
                m_rs232.p_bConnect = true;
            }
        }
        #endregion

        #region RS232
        private void M_rs232_OnRecieve(byte[] aRead, ref int nRead)
        {
            
        }
        
        string ReadFDC(string sRead)
        {
            if (sRead.Length < 7) return "Short Message : " + sRead.Length.ToString();
            byte[] aByte = Encoding.UTF8.GetBytes(sRead);
            int nFDC = aByte[0] - 1;
            if (nFDC < 0) return "Invalid FDC Module ID : " + nFDC.ToString();
            if (nFDC >= m_aData.Count) return "Invalid FDC Module ID : " + nFDC.ToString();
            if (aByte[1] != 0x04) return "Function is not Read Input Register : " + aByte[1].ToString();
            if (aByte[2] != 2) return "Data Byte is not 2 : " + aByte[2].ToString();
            ushort uValue = aByte[3];
            uValue = (ushort)((uValue << 8) + aByte[4]);
            ushort uCRC = aByte[6];
            uCRC = (ushort)((uCRC << 8) + aByte[5]);
            if (uCRC != CalcCRC(aByte, 5)) return "Invalid CRC";
            m_aData[nFDC].p_fValue = uValue;
            return "OK"; 
        }

        byte[] m_aSend = new byte[8]; 
        void SendQuery(int nFDC, int nAdd)
        {
            m_aSend[0] = (byte)(nFDC + 1);
            m_aSend[1] = 0x04;
            m_aSend[2] = (byte)(nAdd >> 8);
            m_aSend[3] = (byte)(nAdd & 0xff);
            m_aSend[4] = 0;
            m_aSend[5] = 2;
            uint uCRC = CalcCRC(m_aSend, 6);
            m_aSend[6] = (byte)(uCRC & 0xff); 
            m_aSend[7] = (byte)(uCRC >> 8);
            m_rs232.Send(m_aSend, 8);
            if (m_aData.Count < nFDC) m_aData[nFDC].p_bSend = true; 
        }

        ushort[] m_uCRC =
        {
            0x0000,    0x1021,    0x2042,    0x3063,    0x4084,    0x50a5,    0x60c6,    0x70e7,
            0x8108,    0x9129,    0xa14a,    0xb16b,    0xc18c,    0xd1ad,    0xe1ce,    0xf1ef,
            0x1231,    0x0210,    0x3273,    0x2252,    0x52b5,    0x4294,    0x72f7,    0x62d6,
            0x9339,    0x8318,    0xb37b,    0xa35a,    0xd3bd,    0xc39c,    0xf3ff,    0xe3de,
            0x2462,    0x3443,    0x0420,    0x1401,    0x64e6,    0x74c7,    0x44a4,    0x5485,
            0xa56a,    0xb54b,    0x8528,    0x9509,    0xe5ee,    0xf5cf,    0xc5ac,    0xd58d,
            0x3653,    0x2672,    0x1611,    0x0630,    0x76d7,    0x66f6,    0x5695,    0x46b4,
            0xb75b,    0xa77a,    0x9719,    0x8738,    0xf7df,    0xe7fe,    0xd79d,    0xc7bc,
            0x48c4,    0x58e5,    0x6886,    0x78a7,    0x0840,    0x1861,    0x2802,    0x3823,
            0xc9cc,    0xd9ed,    0xe98e,    0xf9af,    0x8948,    0x9969,    0xa90a,    0xb92b,
            0x5af5,    0x4ad4,    0x7ab7,    0x6a96,    0x1a71,    0x0a50,    0x3a33,    0x2a12,
            0xdbfd,    0xcbdc,    0xfbbf,    0xeb9e,    0x9b79,    0x8b58,    0xbb3b,    0xab1a,
            0x6ca6,    0x7c87,    0x4ce4,    0x5cc5,    0x2c22,    0x3c03,    0x0c60,    0x1c41,
            0xedae,    0xfd8f,    0xcdec,    0xddcd,    0xad2a,    0xbd0b,    0x8d68,    0x9d49,
            0x7e97,    0x6eb6,    0x5ed5,    0x4ef4,    0x3e13,    0x2e32,    0x1e51,    0x0e70,
            0xff9f,    0xefbe,    0xdfdd,    0xcffc,    0xbf1b,    0xaf3a,    0x9f59,    0x8f78,
            0x9188,    0x81a9,    0xb1ca,    0xa1eb,    0xd10c,    0xc12d,    0xf14e,    0xe16f,
            0x1080,    0x00a1,    0x30c2,    0x20e3,    0x5004,    0x4025,    0x7046,    0x6067,
            0x83b9,    0x9398,    0xa3fb,    0xb3da,    0xc33d,    0xd31c,    0xe37f,    0xf35e,
            0x02b1,    0x1290,    0x22f3,    0x32d2,    0x4235,    0x5214,    0x6277,    0x7256,
            0xb5ea,    0xa5cb,    0x95a8,    0x8589,    0xf56e,    0xe54f,    0xd52c,    0xc50d,
            0x34e2,    0x24c3,    0x14a0,    0x0481,    0x7466,    0x6447,    0x5424,    0x4405,
            0xa7db,    0xb7fa,    0x8799,    0x97b8,    0xe75f,    0xf77e,    0xc71d,    0xd73c,
            0x26d3,    0x36f2,    0x0691,    0x16b0,    0x6657,    0x7676,    0x4615,    0x5634,
            0xd94c,    0xc96d,    0xf90e,    0xe92f,    0x99c8,    0x89e9,    0xb98a,    0xa9ab,
            0x5844,    0x4865,    0x7806,    0x6827,    0x18c0,    0x08e1,    0x3882,    0x28a3,
            0xcb7d,    0xdb5c,    0xeb3f,    0xfb1e,    0x8bf9,    0x9bd8,    0xabbb,    0xbb9a,
            0x4a75,    0x5a54,    0x6a37,    0x7a16,    0x0af1,    0x1ad0,    0x2ab3,    0x3a92,
            0xfd2e,    0xed0f,    0xdd6c,    0xcd4d,    0xbdaa,    0xad8b,    0x9de8,    0x8dc9,
            0x7c26,    0x6c07,    0x5c64,    0x4c45,    0x3ca2,    0x2c83,    0x1ce0,    0x0cc1,
            0xef1f,    0xff3e,    0xcf5d,    0xdf7c,    0xaf9b,    0xbfba,    0x8fd9,    0x9ff8,
            0x6e17,    0x7e36,    0x4e55,    0x5e74,    0x2e93,    0x3eb2,    0x0ed1,    0x1ef0
        };

        uint CalcCRC(byte[] aByte, int nCount)
        {
            //ushort uCRC = 0xffff;
            //for (int n = 0; n < nCount; n++)
            //{
            //    uCRC = (ushort)((uCRC >> 8) ^ m_uCRC[(uCRC ^ aByte[n]) & 0xff]); 
            //}
            //return uCRC;
            UInt32 usCRC = 0xffff;
            byte bytTemp;

            for(int i =0;i<nCount; i+=1)
			{
                bytTemp = aByte[i];
                usCRC = Convert.ToUInt32(usCRC ^ bytTemp);
                for(int j =1;j<9;j++)
				{
                    if((usCRC & 1)==1)
					{
                        usCRC = usCRC >> 1;
                        usCRC = Convert.ToUInt32(usCRC ^ Convert.ToUInt32(0xa001));
					}
					else
                    {
                        usCRC = usCRC >> 1;
                    }
				}
			}
            return usCRC;
        }
        #endregion

        #region Data
        public enum eUnit
        {
            None,
            KPA,
            MPA,
            Temp,
            Voltage,
        }

        public class Data : NotifyProperty
        {
            public string p_id { get; set; }

            eUnit _eUnit = eUnit.None;
            public eUnit p_eUnit
            {
                get { return _eUnit; }
                set
                {
                    if (_eUnit == value) return;
                    _eUnit = value;
                    OnPropertyChanged();
                }
            }

            int m_nDigit = 2;
            double m_fDiv = 100;
            public int[] m_aLimit = new int[2] { 0, 0 };
            ALID[] m_alid = new ALID[2] { null, null };

            bool _bSend = false; 
            public bool p_bSend
            {
                get { return _bSend; }
                set
                {
                    if (_bSend && value) m_alidSend.p_bSet = true;
                    _bSend = value; 
                }
            }
            ALID m_alidSend;

            SVID m_svValue;
            int _nValue = 0; 
            public double p_fValue
            {
                get { return m_svValue.p_value; }
                set
                {
                    p_bSend = false;
                    if (_nValue == value) return;
                    _nValue = (int)value; 
                    m_svValue.p_value = value / m_fDiv;
                    OnPropertyChanged();
                    m_alid[0].p_bSet = (m_svValue.p_value < m_aLimit[0]);
                    m_alid[1].p_bSet = (m_svValue.p_value > m_aLimit[1]);
                    double dValue = Math.Abs(m_svValue.p_value - (m_aLimit[0] + m_aLimit[1]) / 2);
                    int nRed = (int)(500 * dValue / (m_aLimit[1] - m_aLimit[0]));
                    if (nRed > 250) nRed = 250;
                    p_color = Color.FromRgb((byte)nRed, (byte)(250 - nRed), 0); 
                }
            }

            Color _color = Colors.Green;
            public Color p_color
            {
                get { return _color; }
                set
                {
                    if (_color == value) return;
                    _color = value;
                    OnPropertyChanged(); 
                }
            }

            ModuleBase m_module;
            public Data(ModuleBase module, string id)
            {
                m_module = module;
                p_id = id; 
            }

            public void RunTree(Tree tree, int module_number)
            {
                p_id = tree.Set(p_id, p_id, "ID." + module_number.ToString("00"), "FDC Module Name");

                p_eUnit = (eUnit)tree.Set(p_eUnit, p_eUnit, "Unit", "FDC Unit");
                m_nDigit = tree.Set(m_nDigit, m_nDigit, "Digit", "FDC Decimal Point");
                m_fDiv = 1;
                for (int n = 0; n < m_nDigit; n++) m_fDiv *= 10;
                m_aLimit[0] = tree.Set(m_aLimit[0], m_aLimit[0], "Lower Limit", "FDC Lower Limit");
                m_aLimit[1] = tree.Set(m_aLimit[1], m_aLimit[1], "Upper Limit", "FDC Upper Limit");
                if (m_alid[0] == null)
                {
                    m_alid[0] = m_module.m_gaf.GetALID(m_module, ".LowerLimit", "FDC Lower Limit");
                    m_alid[0].p_sMsg = "FDC Value Smaller then Lower Limit";
                    m_alid[1] = m_module.m_gaf.GetALID(m_module, ".UpperLimit", "FDC Upper Limit");
                    m_alid[1].p_sMsg = "FDC Value Larger then Upper Limit";
                    m_alidSend = m_module.m_gaf.GetALID(m_module, ".Timeout", "FDC Communicate Timeout");
                    m_alidSend.p_sMsg = "FDC Communicate Timeout";
                    m_svValue = m_module.m_gaf.GetSVID(m_module, p_id); 
                }
                m_alid[0].p_id = "LowerLimit";
                m_alid[1].p_id = "UpperLimit";
                m_alidSend.p_id = "Timeout"; 
            }
        }
        #endregion

        #region List Data
        public ObservableCollection<Data> m_aData = new ObservableCollection<Data>();

        public int p_lData
        {
            get { return m_aData.Count; }
            set
            {
                if (m_aData.Count == value) return;
                while (m_aData.Count < value) m_aData.Add(new Data(this, m_aData.Count.ToString()));
                while (m_aData.Count > value) m_aData.RemoveAt(m_aData.Count - 1);
            }
        }
        
        void RunTreeData(Tree tree)
        {
            int module_number = 0;
            p_lData = tree.Set(p_lData, p_lData, "Count", "FDC Module Count");
            for (int n = 0; n < m_aData.Count; n++)
            {
                Data data = m_aData[n];
            }
            foreach (Data data in m_aData)
            {
                module_number++;
                data.RunTree(tree.GetTree(data.p_id), module_number); 
            }
        }
        #endregion

        #region Check Thread
        int m_iData = 0; 
        protected override void RunThread()
        {
            base.RunThread();
            Thread.Sleep(m_msInterval);
            if (m_aData.Count > 0)
            {
                SendQuery(m_iData, 1000);
                m_iData = (m_iData + 1) % m_aData.Count;
            }
        }

        int m_msInterval = 100; 
        void RunTreeThread(Tree tree)
        {
            m_msInterval = tree.Set(m_msInterval, m_msInterval, "Interval", "Check Interval (ms)"); 
        }
        #endregion

        #region Tree
        public override void RunTree(Tree tree)
        {
            base.RunTree(tree);
            RunTreeData(tree.GetTree("FDC Module", false));
            RunTreeThread(tree.GetTree("Thread", false)); 
        }
        #endregion

        public FDC(string id, IEngineer engineer)
        {
            p_id = id;
            base.InitBase(id, engineer);
        }

        public override void ThreadStop()
        {
            base.ThreadStop();
        }
    }
}
