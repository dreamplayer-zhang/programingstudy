using RootTools;
using RootTools.Light;
using RootTools.ToolBoxs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
            m_lightToolSet =(LightToolSet)m_Engineer.ClassToolBox().GetToolSet("Light");
        }

        #region Command
        public RelayCommandWithParameter ControlRemoveCommand
        {
            get
            {
                return new RelayCommandWithParameter(MinusIlluminationControl);
            }
        }
        public void MinusIlluminationControl(object obj)
        {
            if (obj.GetType() == typeof(ObservableCollection<LightTool_12ch>))
            {
                m_lightToolSet.RemoveContoroller(typeof(LightTool_12ch));
            }
            else if (obj.GetType() == typeof(ObservableCollection<LightTool_4ch>))
            {
                m_lightToolSet.RemoveContoroller(typeof(LightTool_4ch));
            }
            else if (obj.GetType() == typeof(ObservableCollection<LightTool_Kwangwoo>))
            {
                m_lightToolSet.RemoveContoroller(typeof(LightTool_Kwangwoo));
            }
            else if (obj.GetType() == typeof(ObservableCollection<LightTool_LVS>))
            {
                m_lightToolSet.RemoveContoroller(typeof(LightTool_LVS));
            }
        }
        public RelayCommandWithParameter ControlAddCommand
        {
            get
            {
                return new RelayCommandWithParameter(AddIlluminationControl);
            }
        }

        public void AddIlluminationControl(object obj)
        {
            if (obj.GetType() == typeof(ObservableCollection<LightTool_12ch>))
            {
                m_lightToolSet.AddContoroller(typeof(LightTool_12ch));
            }
            else if (obj.GetType() == typeof(ObservableCollection<LightTool_4ch>))
            {
                m_lightToolSet.AddContoroller(typeof(LightTool_4ch));
            }
            else if (obj.GetType() == typeof(ObservableCollection<LightTool_Kwangwoo>))
            {
                m_lightToolSet.AddContoroller(typeof(LightTool_Kwangwoo));
            }
            else if (obj.GetType() == typeof(ObservableCollection<LightTool_LVS>))
            {
                m_lightToolSet.AddContoroller(typeof(LightTool_LVS));
            }
        }
        #endregion

    }
}
