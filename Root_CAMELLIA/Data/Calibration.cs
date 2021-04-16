
using Met = Root_CAMELLIA.LibSR_Met;
using Root_CAMELLIA.Module;
using Root_CAMELLIA.Data;
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
        //Module_Camellia m_module;
        //DataManager m_DataManager;
        

        public bool CalDone { get; set; } = false;
        public bool InItCalDone { get; set; } = false;
        public string ErrorString { get; set; } = "OK";
        //public Calibration(Module_Camellia module)
        //{
        //    m_module = module;
        //    m_DataManager = module.m_DataManager;
        //}

        public string Run(bool bInitialCal,  bool isPM = false, bool bUseThread = true)
        {

            //if (bInitialCal)
            //{
            //    bInitialCal = false;
            //}
            //else
            //{
            //    CalDone = false;
            //}

            if (!isPM)

            {
                (Met.SettingData, Met.Nanoview.ERRORCODE_NANOVIEW) m_SettingDataWithErrorCode = App.m_nanoView.LoadSettingParameters();
                if (m_SettingDataWithErrorCode.Item2 == Met.Nanoview.ERRORCODE_NANOVIEW.SR_NO_ERROR)
                {
                    Met.DataManager.GetInstance().m_SettngData = m_SettingDataWithErrorCode.Item1;

                    Met.DataManager.GetInstance().m_SettngData.nMeasureIntTime_NIR = DataManager.Instance.recipeDM.MeasurementRD.NIRIntegrationTime;

                    Met.DataManager.GetInstance().m_SettngData.nMeasureIntTime_VIS = DataManager.Instance.recipeDM.MeasurementRD.VISIntegrationTime;
                }
                else
                {
                    return "SettingData Load Error";
                }
            }

            if (bUseThread)
            {
                ThreadPool.QueueUserWorkItem(o => RunThreadPool(bInitialCal));
            }
            else
            {
                if (App.m_nanoView.Calibration(bInitialCal) != LibSR_Met.Nanoview.ERRORCODE_NANOVIEW.SR_NO_ERROR)
                {
                    return "Calibration Fail";
                }
            }

            return "OK";
        }

        public void RunThreadPool(bool bInitialCal)
        {
            if(App.m_nanoView.Calibration(bInitialCal) == Met.Nanoview.ERRORCODE_NANOVIEW.SR_NO_ERROR)
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
            else
            {
                 
            }
        }
    }
}
