using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Met = LibSR_Met;

namespace Root_CAMELLIA
{
    public class Calibration
    {
        private Met.Nanoview m_Nanoview;
        public Calibration()
        {
            m_Nanoview = App.m_nanoView;
        }

        public void Run(int nBGIntTime_VIS, int nBGIntTime_NIR, int nAverage_VIS, int nAverage_NIR, bool bInitialCal)
        {
            ThreadPool.QueueUserWorkItem(o => RunThreadPool(nBGIntTime_VIS, nBGIntTime_NIR, nAverage_VIS, nAverage_NIR, bInitialCal));
        }

        public void RunThreadPool(int nBGIntTime_VIS, int nBGIntTime_NIR, int nAverage_VIS, int nAverage_NIR, bool bInitialCal)
        {
            m_Nanoview.Calibration(nBGIntTime_VIS, nBGIntTime_NIR, nAverage_VIS, nAverage_NIR, bInitialCal);
        }
    }
}
