using Root_CAMELLIA.ManualJob;
using RootTools;
using RootTools.Gem;
using RootTools.Module;
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

namespace Root_CAMELLIA
{
    /// <summary>
    /// WIND2_Hander_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Camellia_Hander_UI : UserControl
    {
        CAMELLIA_Handler m_handler;
        public Camellia_Hander_UI()
        {
            InitializeComponent();
        }

        public void Init(CAMELLIA_Handler handler)
        {
            m_handler = handler;
            DataContext = handler;
            moduleListUI.Init(handler.m_moduleList);
            recipeUI.Init(handler.m_recipe);
            processUI.Init(handler.m_process);
            gafUI.Init(handler.m_gaf);
            InitTabControl();
        }

        void InitTabControl()
        {
            foreach (KeyValuePair<ModuleBase, UserControl> kv in m_handler.m_moduleList.m_aModule)
            {
                TabItem tabItem = new TabItem();
                tabItem.Header = kv.Key.p_id;
                tabItem.Content = kv.Value;
                tabItem.Background = m_handler.p_brushModule;
                tabModule.Items.Add(tabItem);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            InfoCarrier infoCarrier = m_handler.m_aLoadport[0].p_infoCarrier;
            List<GemSlotBase.eState> aSlot = new List<GemSlotBase.eState>();
            string sMap = "1100110011111100000011010";
            foreach (char ch in sMap)
            {
                switch (ch)
                {
                    case '0': aSlot.Add(GemSlotBase.eState.Empty); break;
                    case '1': aSlot.Add(GemSlotBase.eState.Exist); break;
                    case 'D': aSlot.Add(GemSlotBase.eState.Double); break;
                    case 'C': aSlot.Add(GemSlotBase.eState.Cross); break;
                    default:
                        aSlot.Add(GemSlotBase.eState.Undefined);
                        break;
                }
            }
            infoCarrier.SetMapData(aSlot);
            ManualJobSchedule jobSchedule = new ManualJobSchedule(infoCarrier);
            jobSchedule.ShowPopup();
        }
    }
}
