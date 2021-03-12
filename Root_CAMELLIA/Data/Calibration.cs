using Met = Root_CAMELLIA.LibSR_Met;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using RootTools;

namespace Root_CAMELLIA
{
    public class Calibration
    {
        public bool CalDone { get; set; } = false;
        public bool InItCalDone { get; set; } = false;
        public string ErrorString { get; set; } = "OK";
        public Calibration()
        {
           
        }

        public string Run(bool bInitialCal, bool bUseThread = true)
        {
            if (bInitialCal)
            {
                bInitialCal = false;
            }
            else
            {
                CalDone = false;
            }

            (Met.SettingData, Met.Nanoview.ERRORCODE_NANOVIEW) m_SettingDataWithErrorCode = App.m_nanoView.LoadSettingParameters();
            if (m_SettingDataWithErrorCode.Item2 == Met.Nanoview.ERRORCODE_NANOVIEW.SR_NO_ERROR)
            {
                if (bUseThread)
                {
                    ThreadPool.QueueUserWorkItem(o => RunThreadPool(m_SettingDataWithErrorCode.Item1.nBGIntTime_VIS, m_SettingDataWithErrorCode.Item1.nBGIntTime_NIR, m_SettingDataWithErrorCode.Item1.nAverage_VIS, 
                        m_SettingDataWithErrorCode.Item1.nAverage_NIR, bInitialCal));
                }
                else
                {
                    App.m_nanoView.Calibration(m_SettingDataWithErrorCode.Item1.nBGIntTime_VIS, m_SettingDataWithErrorCode.Item1.nBGIntTime_NIR, m_SettingDataWithErrorCode.Item1.nAverage_VIS,
                        m_SettingDataWithErrorCode.Item1.nAverage_NIR, bInitialCal);
                }

            }
            else
            {
                return "SettingData Load Error";
            }
            return "OK";
        }

        public void RunThreadPool(int nBGIntTime_VIS, int nBGIntTime_NIR, int nAverage_VIS, int nAverage_NIR, bool bInitialCal)
        {
            if(App.m_nanoView.Calibration(nBGIntTime_VIS, nBGIntTime_NIR, nAverage_VIS, nAverage_NIR, bInitialCal) == Met.Nanoview.ERRORCODE_NANOVIEW.SR_NO_ERROR)
            {
                if (bInitialCal)
                {
                    InItCalDone = true;
                }
                else
                {
                    CalDone = true;
                }
            }
        }
    }
}
