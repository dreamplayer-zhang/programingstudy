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

namespace Root_VEGA_P_Vision
{
    /// <summary>
    /// Page_RecipeWizard.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Page_RecipeWizard : UserControl
    {
        public Page_RecipeWizard()
        {
            InitializeComponent();
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {

        }
    }
    public class Data
    {
        public string a
        {
            get; set;
        }
        public string b
        {
            get; set;
        }
        public string c
        {
            get; set;
        }
        public string d
        {
            get; set;
        }
        public string e
        {
            get; set;
        }

    }
}
