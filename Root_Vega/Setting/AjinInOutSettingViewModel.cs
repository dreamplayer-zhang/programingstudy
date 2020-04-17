using RootTools;
using RootTools.Control.Ajin;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace Root_Vega
{
    public class AjinInOutSettingViewModel : ObservableObject
    {
        Ajin m_Ajin;
        public Ajin p_Ajin
        {
            get
            {
                return m_Ajin;
            }
            set
            {
                SetProperty(ref m_Ajin, value);
            }
        }

        AjinDIO m_Dio;
        public AjinDIO p_DIO
        {
            get
            {
                return m_Dio;
            }
            set
            {
                SetProperty(ref m_Dio, value);
            }
        }

        AJINModule m_SelModule;
        public AJINModule p_SelModule
        {
            get
            {
                return m_SelModule;
            }
            set
            {
                SetProperty(ref m_SelModule, value);
            }
        }
        

        public AjinInOutSettingViewModel(Ajin ajin)
        {
            p_Ajin = ajin;
            p_DIO = ajin.m_dio;
            ReadSettingData();
        }

        public void ReadSettingData()
        {
        }
    }
}
