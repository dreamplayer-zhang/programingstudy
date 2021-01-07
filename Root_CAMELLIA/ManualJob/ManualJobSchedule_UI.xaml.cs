﻿using RootTools;
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

namespace Root_CAMELLIA.ManualJob
{
    /// <summary>
    /// ManualJobSchedule_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ManualJobSchedule_UI : Window
    {
        static public bool m_bShow = false;
        const int nSlot = 25;
        Button[] m_btnSelect = new Button[nSlot];
        TextBlock[] m_tblockState = new TextBlock[nSlot];
        ComboBox[] m_cbRecipe = new ComboBox[nSlot];
        TextBox[] m_tboxWaferID = new TextBox[nSlot];
        InfoCarrier m_infoCarrier = null;
        public ManualJobSchedule_UI(InfoCarrier infoCarrier)
        {
            InitializeComponent();
            m_infoCarrier = infoCarrier;
            for (int i = 0; i < nSlot; i++)
            {
                m_btnSelect[i] = new Button();
                m_tblockState[i] = new TextBlock();
                m_cbRecipe[i] = new ComboBox();
                m_tboxWaferID[i] = new TextBox();
            }

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
            cd.Width = new GridLength(1.5, GridUnitType.Star);
            gridSlot.ColumnDefinitions.Add(cd);

            cd = new ColumnDefinition();
            cd.Width = new GridLength(1.5, GridUnitType.Star);
            gridSlot.ColumnDefinitions.Add(cd);

            cd = new ColumnDefinition();
            cd.Width = new GridLength(4, GridUnitType.Star);
            gridSlot.ColumnDefinitions.Add(cd);

            cd = new ColumnDefinition();
            cd.Width = new GridLength(3, GridUnitType.Star);
            gridSlot.ColumnDefinitions.Add(cd);

            for (int i = 0; i < nSlot; i++)
            {
                gridSlot.Children.Add(m_btnSelect[i]);
                Grid.SetRow(m_btnSelect[i], i + 1);
                Grid.SetColumn(m_btnSelect[i], 1);

                m_tblockState[i].Text = "none";
                m_tblockState[i].VerticalAlignment = VerticalAlignment.Center;
                m_tblockState[i].TextAlignment = TextAlignment.Center;
                gridSlot.Children.Add(m_tblockState[i]);
                Grid.SetRow(m_tblockState[i], i + 1);
                Grid.SetColumn(m_tblockState[i], 2);

                gridSlot.Children.Add(m_cbRecipe[i]);
                Grid.SetRow(m_cbRecipe[i], i + 1);
                Grid.SetColumn(m_cbRecipe[i], 3);

                m_tboxWaferID[i].Text = string.Format("Wafer{00}", (i + 1).ToString());
                m_tboxWaferID[i].VerticalAlignment = VerticalAlignment.Center;
                m_tboxWaferID[i].TextAlignment = TextAlignment.Center;
                gridSlot.Children.Add(m_tboxWaferID[i]);
                Grid.SetRow(m_tboxWaferID[i], i + 1);
                Grid.SetColumn(m_tboxWaferID[i], 4);
            }

            for(int i=0; i<nSlot; i++)
            {
                if (m_infoCarrier.GetInfoWafer(i) == null)
                {
                    m_btnSelect[i].IsEnabled = false;
                    m_tblockState[i].IsEnabled = false;
                    m_cbRecipe[i].IsEnabled = false;
                    m_tboxWaferID[i].IsEnabled = false;
                }
            }
        }

        void InitSlotRecipeList()
        {
            DirectoryInfo info = new DirectoryInfo("C:\\Recipe\\Camellia");
            FileInfo[] files = info.GetFiles("*.Camellia");
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
            InitRecipeList();
            InitSlotDisplay();
            InitSlotRecipeList();
        }

        void InitRecipeList()
        {
            DirectoryInfo info = new DirectoryInfo("C:\\Recipe\\Camellia");
            FileInfo[] files = info.GetFiles("*.Camellia");
            List<string> asRecipeFile = new List<string>();
            foreach(FileInfo fileInfo in files)
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
            if (sRecipe == null) return;
            m_JobSchedule.p_sRecipe = sRecipe;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
         
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

        }
    }
}
