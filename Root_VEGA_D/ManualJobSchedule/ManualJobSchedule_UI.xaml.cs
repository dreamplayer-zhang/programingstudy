using RootTools;
using RootTools.Gem;
using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Shapes;

namespace Root_VEGA_D
{
    /// <summary>
    /// ManualJobSchedule_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ManualJobSchedule_UI : Window
    {
        static public bool m_bShow = false;
        InfoCarrier m_infoCarrier = null;
        InfoWafer m_infoWafer = null;
        public ManualJobSchedule_UI(InfoCarrier infoCarrier)
        {
            InitializeComponent();
            m_infoCarrier = infoCarrier;
            m_infoWafer = infoCarrier.GetInfoWafer(0);
        }

        ManualJobSchedule m_Manualjob;
        public void Init(ManualJobSchedule manualJob)
        {
            m_Manualjob = manualJob;
            this.DataContext = manualJob;
            InitInfo();
            InitRecipe();
        }

        void InitInfo()
        {
            textboxLocID.Content = m_infoCarrier.p_sLocID;
            textboxLotID.Text = m_infoCarrier.p_sLotID;
            textboxCstID.Text = m_infoCarrier.p_sCarrierID;
        }

        void InitRecipe()
        {
            DirectoryInfo info = new DirectoryInfo("C:\\Recipe\\");
            FileInfo[] files = info.GetFiles("*.VegaD");
            List<string> asRecipeFile = new List<string>();
            foreach(FileInfo fileInfo in files)
            {
                asRecipeFile.Add(fileInfo.Name);
            }
            comboRecipeID.ItemsSource = asRecipeFile;
        }

        private void comboRecipeID_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string sRecipe = (string)comboRecipeID.SelectedValue;
            if (sRecipe == null) return;
            m_infoWafer.p_sRecipe = sRecipe;
        }

        private void checkRnR_Checked(object sender, RoutedEventArgs e)
        {
            textblockRnR.Visibility = Visibility.Visible;
            textboxRnR.Visibility = Visibility.Visible;
        }

        private void checkRnR_Unchecked(object sender, RoutedEventArgs e)
        {
            textblockRnR.Visibility = Visibility.Hidden;
            textboxRnR.Visibility = Visibility.Hidden;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (m_infoCarrier != null)
            {
                //m_infoCarrier.p_sLocID = textboxLocID.Text;
                m_infoCarrier.p_sLotID = textboxLotID.Text;
                m_infoCarrier.p_sCarrierID = textboxCstID.Text;
            }
            else return;

            if (m_infoWafer != null)
            {
                m_infoWafer.p_eState = GemSlotBase.eState.Select;
                m_infoWafer.p_sWaferID = "ReticleID";
                m_infoWafer.RecipeOpen("C:\\Recipe\\" + m_infoWafer.p_sRecipe);
                m_infoCarrier.StartProcess(m_infoWafer.p_id);
            }
            else return;

            EQ.p_nRnR = (bool)checkRnR.IsChecked ? Convert.ToInt32(textboxRnR.Text) : 0;
            this.DialogResult = true;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            m_infoCarrier.SetInfoWafer(0, null);
            this.Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }
    }
}
