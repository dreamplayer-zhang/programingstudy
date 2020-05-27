using RootTools;
using RootTools.Light;
using RootTools.ToolBoxs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Root_Vega
{
    class Setting_IlluminationViewModel : ObservableObject
    {
        Vega_Engineer m_Engineer;
        
        LightToolSet m_lightToolSet;
        public LightToolSet p_lightToolSet
        {
            get { return m_lightToolSet; }
            set { SetProperty(ref m_lightToolSet, value); }
        }
        
        public Setting_IlluminationViewModel(Vega_Engineer engineer)
        {
            m_Engineer = engineer;
            foreach (KeyValuePair<IToolSet, UserControl> kv in engineer.ClassToolBox().m_aToolSet)
            {
                if (kv.Key.p_id == "Light")
                {
                    p_lightToolSet = (LightToolSet)kv.Key;
                    break;
                }
            }
        }
    }
}
