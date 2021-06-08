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

namespace Root_VEGA_P
{
    /// <summary>
    /// FileItem.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class FileItem : UserControl
    {
        string fileName;
        double weight;
        public string FileName
        {
            get => fileName;
            set => fileName = value;
        }
        public double Weight
        {
            get => weight;
            set => weight = value;
        }
        public FileItem(string FileName,double Weight)
        {
            InitializeComponent();
            this.FileName = FileName;
            this.Weight = Weight;
        }
    }
}
