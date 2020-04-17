using System;
using System.Threading;
using System.IO.Ports;
using System.Diagnostics;
using EasyModbus;
using System.Collections.Generic;

namespace RootTools
{
    class TK4SGroup
    {
        ModbusClient m_client = new ModbusClient();
        Thread m_threadCommunicate;
        bool m_bRunThread;

        string m_sSerialPort = "COM0";
        int m_nBaudrate = 9600;
        Parity m_eParity = Parity.None;
        StopBits m_eStopBit = StopBits.Two;

        int m_nTryConnect = 3;
        int m_nInterval = 1000;
        int m_nCountModule = 1;

        List<TK4S> m_aTK4S = new List<TK4S>(); 
        bool m_bBusy = false;
        Stopwatch m_swWaitRecive = new Stopwatch();
        int m_nTimeout = 3000;

        public bool Init()
        {
            m_client.SerialPort = m_sSerialPort;
            m_client.Baudrate = m_nBaudrate;
            m_client.Parity = m_eParity;
            m_client.StopBits = m_eStopBit;
            for (int n = 0; n < m_nTryConnect; n++)
            {
                m_client.Connect();
                if (m_client.Connected) 
                {
                    break;
                }
                Thread.Sleep(100);
            }
            m_client.SendDataChanged += m_client_SendDataChanged;
            m_client.ReceiveDataChanged += m_client_ReceiveDataChanged;
            m_threadCommunicate = new Thread(RunThread);
            m_threadCommunicate.Start();
            return m_client.Connected;
        }

        void RunThread()
        {
            m_bRunThread = true;
            Thread.Sleep(5000);
            while (m_bRunThread)
            {
                Thread.Sleep(10);
                if (!m_client.Connected)
                {
                    Thread.Sleep(3000);
                    m_client.Connect();
                    continue;
                }

                for (int n = 0; n < m_aTK4S.Count; n++)
                {
                    if (m_bRunThread == false) return;
                    try
                    {
                        m_bBusy = true;
                        m_client.UnitIdentifier = (byte)m_aTK4S[n].m_nAddress;
                        m_client.ReadInputRegisters(1000, 2);
                        if (WaitReciveOK() == false)
                        {
                            m_bBusy = false;
                        }
                    }
                    catch (Exception)
                    {
                        m_bBusy = false;
                        // Log추가
                    }
                    Thread.Sleep(m_nInterval);
                }
            }
        }

        bool WaitReciveOK()
        {
            m_swWaitRecive.Start();
            while (m_bBusy && m_swWaitRecive.ElapsedMilliseconds < m_nTimeout)
            {
                Thread.Sleep(10);
            }
            if (m_bBusy) return false;
            return true;
        }

        void m_client_SendDataChanged(object sender)
        {
            string strHex;
            strHex = BitConverter.ToString(m_client.sendData);
            //if (m_bUseLog) m_log.Add(m_id + "--> : " + strHex);
        }

        void m_client_ReceiveDataChanged(object sender)
        {
            try
            {
                string strHex;
                strHex = BitConverter.ToString(m_client.receiveData);
                //if (m_bUseLog) m_log.Add(m_id + "<-- : " + strHex);
                int nAddress = (int)m_client.receiveData[0];
                int nFunc = (int)m_client.receiveData[1];
                int nLength = (int)m_client.receiveData[2];
                Int16 nValue = (Int16)(m_client.receiveData[3] * 256 + m_client.receiveData[4]);
                int nDecimal = (int)(m_client.receiveData[3] * 256 + m_client.receiveData[4]);
                double dValue = nValue / ((nDecimal + 1) * 10);
                m_aTK4S[nAddress].m_dValue = dValue;
            }
            catch (Exception)
            {

            }
            m_bBusy = false;
        }

        void Reallocate()
        {
            while (m_aTK4S.Count < m_nCountModule)
            {
                m_aTK4S.Add(new TK4S(m_aTK4S.Count));
            }
        }

        public class TK4S
        {
            public int m_nAddress = 0;
            public bool m_bAlarm = false;
            public string m_sAlarmMessage = "";
            public double m_dValue = 0.0;
            
            public TK4S(int nAddress)
            {
                m_nAddress = nAddress;
            }
        }
    }
}
