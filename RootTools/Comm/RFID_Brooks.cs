using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Collections;
using System.Runtime;
using System.Diagnostics;

namespace RootTools
{
    public class RFID_Brooks : IRFID
    {
        string m_sID;
        const int m_nReaderID = 0; // Default Value
        const char m_cEndCharacter = (char)0x0D;
        SerialPort m_serial = new SerialPort();
        char[] m_byteBuffer = new char[4096];
        string m_sSerialPort = "COM0";
        int m_nBaudrate = 9600;
        Parity m_eParity = Parity.None;
        StopBits m_eStopBit = StopBits.Two;

        int m_nTryConnect = 3;
        int m_nWaitRecive = 3000;
        bool m_bBusy = false;

//        string m_sReadData = "";

        public RFID_Brooks()
        {

        }

        public void Init(string sID)
        {
            m_sID = sID;

            //RunTree

            m_serial.PortName = m_sSerialPort;
            m_serial.BaudRate = m_nBaudrate;
            m_serial.Parity = m_eParity;
            m_serial.StopBits = m_eStopBit;
            m_serial.DataReceived += m_serial_DataReceived;
            try
            {
                for (int n = 0; n < m_nTryConnect; n++)
                {
                    m_serial.Open();
                    if (m_serial.IsOpen)
                    {
                        break;
                    }
                    Thread.Sleep(100);
                }
            }
            catch (Exception)
            {
                // 로그
            }
        }

        public bool ReadID(ref string sID)
        {
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

            m_serial.Write(sCmd);
            m_bBusy = true;

            return true;
        }

        void m_serial_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            int nRead = m_serial.BytesToRead;
            if (nRead > m_byteBuffer.Length)
            {
                m_byteBuffer = new char[nRead];
            }
            m_serial.Read(m_byteBuffer, 0, nRead);
            string sBuffer = new string(m_byteBuffer);

            // Log sBuffer

            ConvertMessage(ref sBuffer);
            m_bBusy = false;
        }

        bool WaitRecive()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            while (m_bBusy && sw.ElapsedMilliseconds < m_nWaitRecive)
            {
                Thread.Sleep(100);
            }
            return !m_bBusy;
        }

        bool ConvertMessage(ref string sBuffer)
        {
            bool bError = false;
            int nLength, nLengthHigh, nLengthLow, nReaderID, nPage;
            string sData = "";
            if (sBuffer.Length <= 3|| sBuffer[0] != 'S')
            {
                return false;
            }
            nLengthHigh = Convert16To10(sBuffer[1]);
            nLengthLow = Convert16To10(sBuffer[2]);
            nLength = nLengthHigh * 16 + nLengthLow;
            if (sBuffer.Length < nLength) 
            {
                return false;
            }
            switch (sBuffer[3])
            {
                case 'x':
                    nReaderID = Convert.ToInt32(sBuffer[4]);
                    nPage = Convert.ToInt32(sBuffer[5]) * 10 + Convert.ToInt32(sBuffer[6]);
                    for (int n = 7; n < 23; n +=2)
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
                    bError = true;
                    nReaderID = Convert.ToInt32(sBuffer[4]);
                    string sErrorMessage = ConvertErrorMessage(sBuffer[5]);
                    //Log Error

                    break;
                default:
                    break;
            }
            return bError;
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
    }
}
