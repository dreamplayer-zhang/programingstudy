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

        public RootViewer_ViewModel viewerVM;
        public VEGA_D_Engineer m_engineer;
        public bool init(VEGA_D_Engineer engineer)
        {
            m_engineer = engineer;
            viewerVM = new RootViewer_ViewModel();
            viewerVM.init(new ImageData(m_engineer.ClassMemoryTool().GetMemory("Vision.Memory", "Vision", "Main")));
            Viewer_UI.DataContext = viewerVM;

            HelpCollapsed();
            return true;
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
        private void HelpCollapsed()
        {
            Arrow_a.Visibility = Visibility.Collapsed;
            Arrow_b.Visibility = Visibility.Collapsed;
            Arrow_c.Visibility = Visibility.Collapsed;
            Arrow_d.Visibility = Visibility.Collapsed;
            Arrow_e.Visibility = Visibility.Collapsed;
            Arrow_f.Visibility = Visibility.Collapsed;
            Arrow_g.Visibility = Visibility.Collapsed;
            Arrow_h.Visibility = Visibility.Collapsed;
        }

        private void Viewer_UI_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (rb_AlignPt1.IsChecked == true)
            {
                tb_AlignPt1.Text = viewerVM.p_MouseMemX.ToString() + "," + viewerVM.p_MouseMemY.ToString();
                return;
            }
            if (rb_AlignPt2.IsChecked == true)
            {
                tb_AlignPt2.Text = viewerVM.p_MouseMemX.ToString() + "," + viewerVM.p_MouseMemY.ToString();
                return;
            }
            if (rb_a.IsChecked == true)
            {
                tb_a.Text = viewerVM.p_MouseMemX.ToString();
                return;
            }
            if (rb_b.IsChecked == true)
            {
                tb_b.Text = viewerVM.p_MouseMemX.ToString();
                return;
            }
            if (rb_c.IsChecked == true)
            {
                tb_c.Text = viewerVM.p_MouseMemX.ToString();
                return;
            }
            if (rb_c.IsChecked == true)
            {
                tb_c.Text = viewerVM.p_MouseMemX.ToString();
                return;
            }
            if (rb_d.IsChecked == true)
            {
                tb_d.Text = viewerVM.p_MouseMemX.ToString();
                return;
            }
            if (rb_e.IsChecked == true)
            {
                tb_e.Text = viewerVM.p_MouseMemY.ToString();
                return;
            }
            if (rb_f.IsChecked == true)
            {
                tb_f.Text = viewerVM.p_MouseMemY.ToString();
                return;
            }
            if (rb_g.IsChecked == true)
            {
                tb_g.Text = viewerVM.p_MouseMemY.ToString();
                return;
            }
            if (rb_h.IsChecked == true)
            {
                tb_h.Text = viewerVM.p_MouseMemY.ToString();
                CalcSize();
                return;
            }

        }
        private void GroupBox_GotFocus(object sender, RoutedEventArgs e)
        {
            rb_AlignPt1.IsChecked = false;
            rb_AlignPt2.IsChecked = false;
        }
        private void GroupBox_GotFocus_1(object sender, RoutedEventArgs e)
        {
            HelpCollapsed();
            rb_a.IsChecked = false;
            rb_b.IsChecked = false;
            rb_c.IsChecked = false;
            rb_d.IsChecked = false;
            rb_e.IsChecked = false;
            rb_f.IsChecked = false;
            rb_g.IsChecked = false;
            rb_h.IsChecked = false;
        }
        private void Coordinate_Checked(object sender, RoutedEventArgs e)
        {
            HelpCollapsed();
            switch (((RadioButton)sender).Tag)
            {
                case "a":                   
                    Arrow_a.Visibility = Visibility.Visible;
                    break;
                case "b":
                    Arrow_b.Visibility = Visibility.Visible;
                    break;
                case "c":
                    Arrow_c.Visibility = Visibility.Visible;
                    break;
                case "d":
                    Arrow_d.Visibility = Visibility.Visible;
                    break;
                case "e":
                    Arrow_e.Visibility = Visibility.Visible;
                    break;
                case "f":
                    Arrow_f.Visibility = Visibility.Visible;
                    break;
                case "g":
                    Arrow_g.Visibility = Visibility.Visible;
                    break;
                case "h":
                    Arrow_h.Visibility = Visibility.Visible;
                    break;
            }
        }
    }
}
