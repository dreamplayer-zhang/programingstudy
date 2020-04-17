using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace RootTools.Memory
{
    /// <summary>
    /// MemoryViewer_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MemoryViewer_UI : UserControl
    {
        public MemoryViewer_UI()
        {
            InitializeComponent();
        }

        MemoryViewer m_memoryViewer; 
        public void Init(MemoryViewer memoryViewer)
        {
            m_memoryViewer = memoryViewer;
            this.DataContext = memoryViewer;
        }

        private void menuOpen_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "Image Files(*.bmp;*.jpg)|*.bmp;*.jpg";
            if (dlg.ShowDialog() == true) m_memoryViewer.FileOpen(dlg.FileName); 
        }

        private void memuSave_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.DefaultExt = ".bmp";
            dlg.Filter = "Image Files(*.bmp;*.jpg)|*.bmp;*.jpg";
            if (dlg.ShowDialog() == true) m_memoryViewer.FileSave(dlg.FileName);
        }

        private void gridBitmapSource_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            m_memoryViewer.p_szWindow = new CPoint((int)gridBitmapSource.ActualWidth, (int)gridBitmapSource.ActualHeight); 
        }

        private void gridBitmapSource_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            Point p = e.GetPosition(imageBitmapSource);
            m_memoryViewer.p_cpWindow = new CPoint((int)p.X, (int)p.Y); 
        }

        private void gridBitmapSource_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            Point p = e.GetPosition(imageBitmapSource);
            if (e.Delta > 0) m_memoryViewer.ZoomIn(new CPoint((int)p.X, (int)p.Y)); 
            else m_memoryViewer.ZoomOut(new CPoint((int)p.X, (int)p.Y));
        }

        private void gridBitmapSource_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point p = e.GetPosition(imageBitmapSource);
            m_memoryViewer.m_bLBD = true;
            m_memoryViewer.p_cpLBD = new CPoint((int)p.X, (int)p.Y);
        }

        private void gridBitmapSource_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            m_memoryViewer.m_bLBD = false;
        }

        private void gridBitmapSource_MouseLeave(object sender, MouseEventArgs e)
        {
            m_memoryViewer.m_bLBD = false;
        }
    }
}
