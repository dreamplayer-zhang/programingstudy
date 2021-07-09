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
using Root_VEGA_D.Engineer;
using Root_EFEM.Module;
using System.Globalization;
using RootTools.Module;

namespace Root_VEGA_D
{
    /// <summary>
    /// ManualJobSchedule_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ManualJobSchedule_UI : Window
    {
        static public bool bParallel = false;
        static public bool m_bShow = false;
        InfoCarrier m_infoCarrier = null;
        InfoWafer m_infoWafer = null;
        VEGA_D_Engineer m_engineer;
        VEGA_D_Handler m_handler;
        public ManualJobSchedule_UI(InfoCarrier infoCarrier)
        {
            InitializeComponent();
            m_infoCarrier = infoCarrier;
            m_infoWafer = infoCarrier.GetInfoWafer(0);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            m_bShow = false;
        }
        ManualJobSchedule m_Manualjob;
        Loadport_Cymechs m_loadport;
        public void Init(ManualJobSchedule manualJob, IEngineer engineer, Loadport_Cymechs loadport)
        {
            m_Manualjob = manualJob;
            this.DataContext = manualJob;
            m_engineer = (VEGA_D_Engineer)engineer;
            m_handler = m_engineer.m_handler;
            btnRun.DataContext = loadport;
            m_loadport = loadport;
            //InitInfo();
            textboxLocID.Content = m_infoCarrier.p_sLocID;
            InitRecipe();
        }

        //void InitInfo()
        //{
        //    textboxLocID.Content = m_infoCarrier.p_sLocID;
        //    textboxLotID.Text = m_infoCarrier.p_sLotID;
        //    textboxCstID.Text = m_infoCarrier.p_sCarrierID;
        //}

        void InitRecipe()
        {
            DirectoryInfo info = new DirectoryInfo("C:\\Recipe\\VEGA_D");
            FileInfo[] files = info.GetFiles("*.Vega_D");
            List<string> asRecipeFile = new List<string>();
            foreach(FileInfo fileInfo in files)
            {
                asRecipeFile.Add(fileInfo.Name);
            }
            comboRecipeID.ItemsSource = asRecipeFile;
            comboRecipeID.SelectedIndex = 0;
        }

        private void comboRecipeID_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string sRecipe = (string)comboRecipeID.SelectedValue;
            if (sRecipe == null) return;
            m_infoWafer.p_sRecipe = sRecipe;
        }

        private void checkRnR_Checked(object sender, RoutedEventArgs e)
        {
            m_Manualjob.p_bRnR = true;
            bParallel = true;
            textblockRnR.Visibility = Visibility.Visible;
            textboxRnR.Visibility = Visibility.Visible;
        }

        private void checkRnR_Unchecked(object sender, RoutedEventArgs e)
        {
            m_Manualjob.p_bRnR = false;
            bParallel = false;
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
            InfoWafer infoWafer = m_infoCarrier.GetInfoWafer(0);
            if (infoWafer != null)
            {
                infoWafer.RecipeOpen("C:\\Recipe\\VEGA_D\\" + m_infoWafer.p_sRecipe);
                //infoWafer.RecipeOpen("C:\\Recipe\\VEGA_D\\" + "OnlyOne.Vega_D");
                //m_handler.m_RNRinfoWafer = infoWafer;
                m_handler.AddSequence(infoWafer);
                m_handler.CalcSequence();
            }
            EQ.p_nRnR = (bool)checkRnR.IsChecked ? Convert.ToInt32(textboxRnR.Text) : 0;
            m_handler.m_bIsRNR = (bool)checkRnR.IsChecked ? true : false;
            //if (m_infoWafer != null)
            //{
            //m_infoWafer.p_eState = GemSlotBase.eState.Select;
            //m_infoWafer.p_sWaferID = "ReticleID";
            //m_infoWafer.RecipeOpen("C:\\Recipe\\" + m_infoWafer.p_sRecipe);
            //m_infoWafer.RecipeOpen("C:\\Recipe\\VEGA_D\\" + "OnlyOne.Vega_D");//단일 레시피 적용
            //m_infoCarrier.StartProcess(m_infoWafer.p_id);
            //}
            this.DialogResult = true;
            //this.Close();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            //m_infoCarrier.SetInfoWafer(0, null);
            ModuleRunBase UnDocking = m_loadport.m_runUndocking.Clone();
            m_loadport.StartRun(UnDocking);
            this.Close();
        }
    }
    class BooltoVisibleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            {
                if ((bool)value == true)
                    return Visibility.Visible;
                else
                    return Visibility.Collapsed;
            }

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
