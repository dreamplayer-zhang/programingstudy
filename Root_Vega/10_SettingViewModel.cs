using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RootTools.Control.Ajin;
using RootTools;

namespace Root_Vega
{
    class _10_SettingViewModel: ObservableObject
    {
        AjinInOutSettingViewModel m_AjinViewModel;
        public AjinInOutSettingViewModel p_AjinViewModel
        {
            get
            {
                return m_AjinViewModel;
            }
            set
            {
                SetProperty(ref m_AjinViewModel,value);
            }
        }

        Optic_MainVisionViewModel m_MainVisionViewModel;
        public Optic_MainVisionViewModel p_MainVisionViewModel
        {
            get
            {
                return m_MainVisionViewModel;
            }
            set
            {
                SetProperty(ref m_MainVisionViewModel, value);
            }
        }
        Optic_SideVisionViewModel m_SideVisionViewModel;
        public Optic_SideVisionViewModel p_SideVisionViewModel
        {
            get
            {
                return m_SideVisionViewModel;
            }
            set
            {
                SetProperty(ref m_SideVisionViewModel, value);
            }
        }

        Setting_IlluminationViewModel m_IlluminationViewModel;
        public Setting_IlluminationViewModel p_IlluminationViewModel
        {
            get
            {
                return m_IlluminationViewModel;
            }
            set
            {
                SetProperty(ref m_IlluminationViewModel, value);
            }
        }
        
        public _10_SettingViewModel(Vega_Engineer engineer, IDialogService service)
        {
            p_AjinViewModel = new AjinInOutSettingViewModel(engineer.m_ajin);
            p_MainVisionViewModel = new Optic_MainVisionViewModel(engineer, service);
            p_SideVisionViewModel = new Optic_SideVisionViewModel(engineer, service);
            p_IlluminationViewModel = new Setting_IlluminationViewModel(engineer);
        }
    }
}
