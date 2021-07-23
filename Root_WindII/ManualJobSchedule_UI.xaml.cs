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
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Root_WindII
{
    /// <summary>
    /// ManualJobSchedule_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ManualJobSchedule_UI : Window
    {
        static public bool m_bShow = false;
        int nSlot = 25;
        Button[] m_btnSlot = new Button[25];
        ToggleButton[] m_togbtnSelect = new ToggleButton[25];
        TextBlock[] m_tblockState = new TextBlock[25];
        ComboBox[] m_cbRecipe = new ComboBox[25];
        TextBox[] m_tboxWaferID = new TextBox[25];
        InfoCarrier m_infoCarrier = null;
        public ManualJobSchedule_UI(InfoCarrier infoCarrier)
        {
            InitializeComponent();
            m_infoCarrier = infoCarrier;
            nSlot = m_infoCarrier.p_lWafer;
            for (int i = 0; i < nSlot; i++)
            {
                m_btnSlot[i] = new Button();
                m_btnSlot[i].Content = i + 1;
                m_togbtnSelect[i] = new ToggleButton();
                m_togbtnSelect[i].Margin = new Thickness(10, 1, 10, 1);
                m_togbtnSelect[i].Tag = i;
                m_togbtnSelect[i].Checked += ManualJobSchedule_UI_Checked;
                m_togbtnSelect[i].Unchecked += ManualJobSchedule_UI_Unchecked;
                m_tblockState[i] = new TextBlock();
                m_tblockState[i].Foreground = Brushes.White;
                m_cbRecipe[i] = new ComboBox();
                m_cbRecipe[i].IsEnabled = false;
                m_tboxWaferID[i] = new TextBox();
                m_tboxWaferID[i].IsEnabled = false;
            }
        }

        private void ManualJobSchedule_UI_Unchecked(object sender, RoutedEventArgs e)
        {
            ToggleButton toggleButton = (ToggleButton)sender;
            int nIndex = Convert.ToInt32(toggleButton.Tag);
            m_tblockState[nIndex].Text = "Exist";
            m_cbRecipe[nIndex].IsEnabled = false;
            m_tboxWaferID[nIndex].IsEnabled = false;
        }

        private void ManualJobSchedule_UI_Checked(object sender, RoutedEventArgs e)
        {
            ToggleButton toggleButton = (ToggleButton)sender;
            int nIndex = Convert.ToInt32(toggleButton.Tag);
            m_tblockState[nIndex].Text = "Select";
            m_cbRecipe[nIndex].IsEnabled = true;
            m_tboxWaferID[nIndex].IsEnabled = true;
        }

        void InitSlotDisplay()
        {
            RowDefinition rd = new RowDefinition();
            rd.Height = GridLength.Auto;
            gridSlot.RowDefinitions.Add(rd);
            for (int i = 0; i < nSlot; i++)
            {
                rd = new RowDefinition();
                rd.Height = new GridLength(1, GridUnitType.Star);
                gridSlot.RowDefinitions.Add(rd);
            }

            ColumnDefinition cd = new ColumnDefinition();
            cd.Width = GridLength.Auto;
            gridSlot.ColumnDefinitions.Add(cd);

            cd = new ColumnDefinition();
            cd.Width = new GridLength(1, GridUnitType.Star);
            gridSlot.ColumnDefinitions.Add(cd);

            cd = new ColumnDefinition();
            cd.Width = new GridLength(1.2, GridUnitType.Star);
            gridSlot.ColumnDefinitions.Add(cd);

            cd = new ColumnDefinition();
            cd.Width = new GridLength(1.3, GridUnitType.Star);
            gridSlot.ColumnDefinitions.Add(cd);

            cd = new ColumnDefinition();
            cd.Width = new GridLength(4, GridUnitType.Star);
            gridSlot.ColumnDefinitions.Add(cd);

            cd = new ColumnDefinition();
            cd.Width = new GridLength(2.5, GridUnitType.Star);
            gridSlot.ColumnDefinitions.Add(cd);

            for (int i = 0; i < nSlot; i++)
            {
                gridSlot.Children.Add(m_btnSlot[i]);
                Grid.SetRow(m_btnSlot[i], nSlot - i);
                Grid.SetColumn(m_btnSlot[i], 1);

                gridSlot.Children.Add(m_togbtnSelect[i]);
                Grid.SetRow(m_togbtnSelect[i], nSlot - i);
                Grid.SetColumn(m_togbtnSelect[i], 2);

                m_tblockState[i].Text = "none";
                m_tblockState[i].VerticalAlignment = VerticalAlignment.Center;
                m_tblockState[i].TextAlignment = TextAlignment.Center;
                gridSlot.Children.Add(m_tblockState[i]);
                Grid.SetRow(m_tblockState[i], nSlot - i);
                Grid.SetColumn(m_tblockState[i], 3);

                gridSlot.Children.Add(m_cbRecipe[i]);
                Grid.SetRow(m_cbRecipe[i], nSlot - i);
                Grid.SetColumn(m_cbRecipe[i], 4);

                m_tboxWaferID[i].Text = string.Format("Wafer{00}", (i + 1).ToString());
                m_tboxWaferID[i].VerticalAlignment = VerticalAlignment.Center;
                m_tboxWaferID[i].TextAlignment = TextAlignment.Center;
                gridSlot.Children.Add(m_tboxWaferID[i]);
                Grid.SetRow(m_tboxWaferID[i], nSlot - i);
                Grid.SetColumn(m_tboxWaferID[i], 5);
            }

            for (int i = 0; i < nSlot; i++)
            {
                if (m_infoCarrier.GetInfoWafer(i) == null)
                {
                    m_togbtnSelect[i].Visibility = Visibility.Hidden;
                    m_tblockState[i].Text = "Empty";
                    m_cbRecipe[i].Visibility = Visibility.Hidden;
                    m_tboxWaferID[i].Visibility = Visibility.Hidden;
                }
                else
                {
                    m_togbtnSelect[i].Visibility = Visibility.Visible;
                    m_tblockState[i].Text = "Exist";
                    m_cbRecipe[i].Visibility = Visibility.Visible;
                    m_tboxWaferID[i].Visibility = Visibility.Visible;
                }
            }
        }

        void InitSlotRecipeList()
        {
            DirectoryInfo info = new DirectoryInfo("C:\\Recipe");
            FileInfo[] files = info.GetFiles("*.WIND2F");
            List<string> asRecipeFile = new List<string>();
            foreach (FileInfo fileInfo in files)
            {
                asRecipeFile.Add(fileInfo.Name);
            }
            for (int i = 0; i < nSlot; i++)
            {
                m_cbRecipe[i].ItemsSource = asRecipeFile;
            }
        }

        ManualJobSchedule m_JobSchedule;

        public void Init(ManualJobSchedule manualJobSchedule, InfoCarrier infoCarrier)
        {
            m_JobSchedule = manualJobSchedule;
            this.DataContext = manualJobSchedule;
            m_infoCarrier = infoCarrier;
            InitInfo();
            InitRecipeList();
            InitSlotDisplay();
            InitSlotRecipeList();
        }

        void InitInfo()
        {
            textboxLocID.Text = m_infoCarrier.p_sLocID;
            textboxLotID.Text = m_infoCarrier.p_sLotID;
            textboxCstID.Text = m_infoCarrier.p_sCarrierID;
        }

        void InitRecipeList()
        {
            DirectoryInfo info = new DirectoryInfo("C:\\Recipe");
            FileInfo[] files = info.GetFiles("*.WIND2F");
            List<string> asRecipeFile = new List<string>();
            foreach (FileInfo fileInfo in files)
            {
                asRecipeFile.Add(fileInfo.Name);
            }
            comboRecipeID.ItemsSource = asRecipeFile;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }

        private void comboRecipeID_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string sRecipe = (string)comboRecipeID.SelectedValue;
            if (sRecipe == null)
                return;
            m_JobSchedule.p_sRecipe = sRecipe;
            int nIndex = comboRecipeID.SelectedIndex;
            ChangeSlotRecipe(nIndex);
        }

        void ChangeSlotRecipe(int nIndex)
        {
            for (int i = 0; i < nSlot; i++)
            {
                if (m_infoCarrier.GetInfoWafer(i) != null)
                {
                    m_cbRecipe[i].SelectedIndex = nIndex;
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (m_infoCarrier == null)
                return;
            m_infoCarrier.p_sLocID = textboxLocID.Text;
            m_infoCarrier.p_sLotID = textboxLotID.Text;
            m_infoCarrier.p_sCarrierID = textboxCstID.Text;


            int nWaferNum = 0;
            int nSlot = 0;
            for (int i = 0; i < nSlot; i++)
            {
                InfoWafer infoWafer = m_infoCarrier.GetInfoWafer(i);
                if (infoWafer.p_eState == GemSlotBase.eState.Select)
                {
                    nWaferNum++;
                    nSlot = i;
                }
            }
            if (nWaferNum == 1)
            {
                InfoWafer infoWafer = m_infoCarrier.GetInfoWafer(nSlot);
                if (infoWafer.p_eState == GemSlotBase.eState.Select)
                {
                    infoWafer.p_eWaferOrder = InfoWafer.eWaferOrder.FirstLastWafer;
                }
            }
            else
            {

                for (int i = 0; i < nSlot; i++)
                {
                    InfoWafer infoWafer = m_infoCarrier.GetInfoWafer(i);
                    if (infoWafer.p_eState == GemSlotBase.eState.Select)
                    {
                        infoWafer.p_eWaferOrder = InfoWafer.eWaferOrder.FirstWafer;
                        break;
                    }
                }

                for (int i = nSlot - 1; i >= 0; i--)
                {
                    InfoWafer infoWafer = m_infoCarrier.GetInfoWafer(i);
                    if (infoWafer.p_eState == GemSlotBase.eState.Select)
                    {
                        infoWafer.p_eWaferOrder = InfoWafer.eWaferOrder.LastWafer;
                        break;
                    }
                }
            }

            for (int i = 0; i < nSlot; i++)
            {
                InfoWafer infoWafer = m_infoCarrier.GetInfoWafer(i);
                if (infoWafer != null)
                {
                    infoWafer.p_eState = (m_tblockState[i].Text == "Select") ? GemSlotBase.eState.Select : GemSlotBase.eState.Empty;
                    infoWafer.p_sWaferID = m_tboxWaferID[i].Text;
                    if (infoWafer.p_eState == GemSlotBase.eState.Select)
                    {
                        infoWafer.RecipeOpen("C:\\Recipe\\" + m_cbRecipe[i].Text);
                        m_infoCarrier.StartProcess(infoWafer.p_id);
                    }
                }
            }
            m_infoCarrier.SetSelectMapData(m_infoCarrier);
            this.DialogResult = true;
            EQ.p_nRnR = Convert.ToInt32(textboxRnR.Text);
            EQ.p_eState = EQ.eState.Run;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            for (int n = 0; n < m_infoCarrier.p_lWafer; n++)
            {
                if (m_infoCarrier.GetInfoWafer(n) != null)
                    m_infoCarrier.SetInfoWafer(n, null);
            }
            this.Close();
        }
    }
}
