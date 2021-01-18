using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RootTools.Light
{
    /// <summary>
    /// LightTool_DAWOO_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class LightTool_DAWOO_UI : UserControl
    {
        public LightTool_DAWOO_UI()
        {
            InitializeComponent();
        }

        LightTool_DAWOO m_lightTool;
        public void Init(LightTool_DAWOO lightTool)
        {
            m_lightTool = lightTool;
            this.DataContext = lightTool;
            rs232UI.Init(lightTool.m_rs232);
            treeRootSetupUI.Init(lightTool.m_treeRootSetup);
            lightTool.RunTreeSetup(Trees.Tree.eMode.Init);
            InitTabControl();
            lightTool.OnChangeTool += LightTool_OnChangeTool;
        }

        private void LightTool_OnChangeTool()
        {
            InitTabControl();
        }

        void InitTabControl()
        {
            tabControl.Items.Clear();
            foreach (LightBase light in m_lightTool.p_aLight)
            {
                TabItem tabItem = new TabItem();
                tabItem.Header = light.p_sID.Replace(m_lightTool.p_id + ".", "");
                tabItem.Content = light.p_ui;
                tabControl.Items.Add(tabItem);
            }
        }
    }
}
