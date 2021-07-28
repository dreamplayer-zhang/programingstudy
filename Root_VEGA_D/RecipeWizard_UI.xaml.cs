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
using Root_VEGA_D.Engineer;
using RootTools;
namespace Root_VEGA_D
{
    /// <summary>
    /// RecipeWizard_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class RecipeWizard_UI : UserControl
    {
        public RecipeWizard_UI()
        {
            InitializeComponent();
        }

        private void CalcSize()
        {
            Int32.TryParse(tb_a.Text, out int a);
            Int32.TryParse(tb_b.Text, out int b);
            Int32.TryParse(tb_c.Text, out int c);
            Int32.TryParse(tb_d.Text, out int d);
            Int32.TryParse(tb_e.Text, out int ee);
            Int32.TryParse(tb_f.Text, out int f);
            Int32.TryParse(tb_g.Text, out int g);
            Int32.TryParse(tb_h.Text, out int h);
            tb_dieSize.Text = (b - a).ToString() + "," + (ee - f).ToString();
            tb_laneSize.Text = (c - b).ToString() + "," + (f - g).ToString();
            tb_shotSize.Text = (c - a).ToString() + "," + (ee - g).ToString();
            tb_shot.Text = (d - a).ToString() + "," + (d - h).ToString();
        }
      
    }
}
