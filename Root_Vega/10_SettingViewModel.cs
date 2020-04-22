﻿using System;
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

        Setting.Setting_RADSViewModel m_Setting_RADS;
        public Setting.Setting_RADSViewModel p_Setting_RADS
        {
            get
            {
                return m_Setting_RADS;
            }
            set
            {
                SetProperty(ref m_Setting_RADS, value);
            }
        }

        public _10_SettingViewModel(Vega_Engineer engineer)
        {
            p_AjinViewModel = new AjinInOutSettingViewModel(engineer.m_ajin);
            p_Setting_RADS = new Setting.Setting_RADSViewModel(engineer);
        }
    }
}
