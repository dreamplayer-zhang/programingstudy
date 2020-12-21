﻿using RootTools.Module;
using System.Collections.Generic;
using System.Windows.Controls;

namespace Root_EFEM
{
    /// <summary>
    /// EFEM_Handler_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class EFEM_Handler_UI : UserControl
    {
        public EFEM_Handler_UI()
        {
            InitializeComponent();
        }

        EFEM_Handler m_handler;
        public void Init(EFEM_Handler handler)
        {
            m_handler = handler;
            DataContext = handler;
            moduleListUI.Init(handler.p_moduleList);
            recipeUI.Init(handler.m_recipe);
            processUI.Init(handler.m_process);
            gafUI.Init(handler.m_gaf);
            InitTabControl();
        }

        void InitTabControl()
        {
            foreach (KeyValuePair<ModuleBase, UserControl> kv in m_handler.p_moduleList.m_aModule)
            {
                TabItem tabItem = new TabItem();
                tabItem.Header = kv.Key.p_id;
                tabItem.Content = kv.Value;
                tabItem.Background = m_handler.p_brushModule;
                tabModule.Items.Add(tabItem);
            }
        }
    }
}
