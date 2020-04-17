using Microsoft.Win32;
using RootTools.Trees;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace RootTools.RTC5s
{
    /// <summary>
    /// RTC5Design_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class RTC5Design_UI : UserControl
    {
        public RTC5Design_UI()
        {
            InitializeComponent();
        }

        RTC5Design m_design;
        public void Init(RTC5Design design)
        {
            m_design = design;
            this.DataContext = design;
            comboDesign.ItemsSource = Enum.GetValues(typeof(RTC5Design.eDesign)).Cast<RTC5Design.eDesign>();
            treeRootUI.Init(design.m_treeRoot);
            design.m_treeRoot.UpdateTree += M_treeRoot_UpdateTree;
            design.RunTree(Tree.eMode.Init);
        }

        private void M_treeRoot_UpdateTree()
        {
            Draw();
        }

        private void buttonAdd_Click(object sender, RoutedEventArgs e)
        {
            if (comboDesign.SelectedItem == null) return;
            RTC5Design.eDesign design = (RTC5Design.eDesign)comboDesign.SelectedItem;
            m_design.Add(design);
            comboDraw.ItemsSource = null;
            comboDraw.ItemsSource = m_design.p_asDesign;
            comboDraw.SelectedItem = m_design.p_asDesign[m_design.p_asDesign.Count - 1];
        }

        private void comboDraw_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Draw();
        }

        void Draw()
        {
            gridDrawing.Children.Clear();
            if (comboDraw.SelectedItem == null) return;
            string sDesign = (string)comboDraw.SelectedItem;
            DesignBase design = m_design.Get(sDesign);
            if (design == null) return;
            RPoint szGrid = new RPoint(gridDraw.ActualWidth - 2, gridDraw.ActualHeight - 2);
            RPoint szData = design.m_dataList.m_szData;
            if ((szData.X == 0) && (szData.Y == 0)) return;
            if (szData.X == 0) szData.X = szData.Y;
            if (szData.Y == 0) szData.Y = szData.X;
            double fScale = Math.Min(szGrid.X / szData.X, szGrid.Y / szData.Y);
            gridDrawing.Width = fScale * szData.X;
            gridDrawing.Height = fScale * szData.Y;
            RPoint rpCenter = new RPoint(fScale * szData.X / 2, fScale * szData.Y / 2);
            DrawDataList(design.m_hatching.m_dataList, fScale, rpCenter, Brushes.DarkGreen);
            Brush brush = (design.m_eMark != DesignBase.eMark.Hatching) ? Brushes.Green : Brushes.DarkGray;
            DrawDataList(design.m_dataList, fScale, rpCenter, brush);
        }

        void DrawDataList(DataList dataList, double fScale, RPoint rpCenter, Brush brush)
        {
            if (dataList.m_aData.Count == 0) return;
            Polyline polyline = new Polyline();
            foreach (DataList.Data data in dataList.m_aData)
            {
                switch (data.m_eCmd)
                {
                    case DataList.Data.eCmd.Jump:
                        polyline = new Polyline();
                        polyline.Stroke = brush;
                        polyline.StrokeThickness = 1;
                        polyline.Points.Add(new Point(rpCenter.X + fScale * data.m_x, rpCenter.Y - fScale * data.m_y));
                        gridDrawing.Children.Add(polyline);
                        break;
                    case DataList.Data.eCmd.Mark:
                        polyline.Points.Add(new Point(rpCenter.X + fScale * data.m_x, rpCenter.Y - fScale * data.m_y));
                        break;
                    case DataList.Data.eCmd.Dot:
                        polyline = new Polyline();
                        polyline.Stroke = Brushes.Red;
                        polyline.StrokeThickness = 1;
                        double x = rpCenter.X + fScale * data.m_x;
                        double y = rpCenter.Y - fScale * data.m_y;
                        polyline.Points.Add(new Point(x - 1, y));
                        polyline.Points.Add(new Point(x, y + 1));
                        polyline.Points.Add(new Point(x + 1, y));
                        polyline.Points.Add(new Point(x, y - 1));
                        polyline.Points.Add(new Point(x - 1, y));
                        gridDrawing.Children.Add(polyline);
                        break;
                }
            }
        }

        private void buttonSave_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog();
            string sExt = "." + m_design.m_sExt;
            dlg.DefaultExt = sExt;
            dlg.Filter = "Save RTC5 Design File (" + sExt + ")|*" + sExt;
            if (dlg.ShowDialog() == false) return;
            m_design.FileSave(dlg.FileName);
        }

        private void buttonOpen_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            string sExt = "." + m_design.m_sExt;
            dlg.DefaultExt = sExt;
            dlg.Filter = "Save RTC5 Design File (" + sExt + ")|*" + sExt;
            if (dlg.ShowDialog() == false) return;
            m_design.FileOpen(dlg.FileName);
            comboDraw.ItemsSource = null;
            comboDraw.ItemsSource = m_design.p_asDesign;
            comboDraw.SelectedItem = m_design.p_asDesign[m_design.p_asDesign.Count - 1];
        }

        private void buttonClear_Click(object sender, RoutedEventArgs e)
        {
            m_design.m_aDesign.Clear();
            m_design.RunTree(Tree.eMode.Init);
            comboDraw.ItemsSource = null;
            comboDraw.ItemsSource = m_design.p_asDesign;
            if (m_design.p_asDesign.Count < 1) return;
            comboDraw.SelectedItem = m_design.p_asDesign[m_design.p_asDesign.Count - 1];
        }
    }
}
