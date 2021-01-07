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
        public ManualJobSchedule_UI()
        {
            InitializeComponent();
        }

        ManualJobSchedule m_JobSchedule;

        public void Init(ManualJobSchedule manualJobSchedule)
        {
            m_JobSchedule = manualJobSchedule;
            this.DataContext = manualJobSchedule;
            InitRecipeList();
        }

        void InitRecipeList()
        {
            DirectoryInfo info = new DirectoryInfo("C:\\Recipe\\Camellia");
            FileInfo[] files = info.GetFiles("*.Camellia");
            List<string> asRecipeFile = new List<string>();
            foreach(FileInfo fileInfo in files)
            {
                asRecipeFile.Add(fileInfo.FullName);
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
