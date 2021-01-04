using RootTools;
using RootTools.Inspects;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Root_CAMELLIA
{
    public class MeasurementManager : Singleton<MeasurementManager>, INotifyPropertyChanged
    {
        Thread Thread { get; set; }
        Measurement[] measurementThread { get; set; }
        StopWatch StopWatch { get; set; }
        bool m_bProgress { get; set; }
        public bool IsInitialized { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public void StartMeasurement()
        {
            if (m_bProgress)
            {
                return;
            }
            m_bProgress = false;

        }
    }
}