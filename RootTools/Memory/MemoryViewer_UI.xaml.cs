using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
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

        MemoryViewer m_viewer;
        MemoryPool m_pool; 
        public void Init(MemoryViewer memoryViewer, bool bShowCombo = true)
        {
            m_viewer = memoryViewer;
            m_pool = memoryViewer.m_memoryPool; 
            this.DataContext = memoryViewer;
            comboBoxGroup.ItemsSource = memoryViewer.m_memoryPool.m_asGroup;
            memoryViewer.OnInvalidDraw += MemoryViewer_OnInvalidDraw;
            if (bShowCombo == false)
            {
                CollepseComboBox(comboBoxGroup);
                CollepseComboBox(comboBoxMemory);
                CollepseComboBox(comboBoxIndex); 
            }
        }

        void CollepseComboBox(ComboBox comboBox)
        {
            comboBox.MinWidth = 0; 
            comboBox.Width = 0; 
            comboBox.Height = 0; 
        }

        private void MemoryViewer_OnInvalidDraw()
        {
            m_viewer.Draw(gridDrawing); 
        }

        #region File
        private void menuOpen_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "Image Files(*.bmp;*.jpg;*.bayer)|*.bmp;*.jpg;*.bayer";
            if (dlg.ShowDialog() == true) m_viewer.FileOpen(dlg.FileName); 
        }

        private void memuSave_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.DefaultExt = ".bmp";
            dlg.FilterIndex = 1;
            dlg.Filter = "8bit Bitmap Files(*.bmp)|*.bmp|16bit Bitmap Files(*.bmp)|*.bmp|JPEG Image Files(*.jpg)|*.jpg";
            if (dlg.ShowDialog() == true)
            {
                int nByte = (dlg.FilterIndex != 2) ? 1 : 2;
                m_viewer.FileSave(dlg.FileName, nByte);
            }
        }
        #endregion

        #region Image View
        private void gridBitmapSource_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (m_viewer == null) return; 
            m_viewer.p_szWindow = new CPoint((int)gridBitmapSource.ActualWidth, (int)gridBitmapSource.ActualHeight);
            m_viewer.Draw(gridDrawing);
        }

        private void gridBitmapSource_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (m_viewer == null) return;
            Point p = e.GetPosition(imageBitmapSource);
            m_viewer.p_cpWindow = new CPoint((int)p.X, (int)p.Y);
            if (m_viewer.p_bLBD) m_viewer.Draw(gridDrawing);
            if (m_viewer.p_bLBD && e.LeftButton == MouseButtonState.Released) m_viewer.p_bLBD = false;
        }

        private void gridBitmapSource_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            Point p = e.GetPosition(imageBitmapSource);
            if (e.Delta > 0) m_viewer.ZoomIn(new CPoint((int)p.X, (int)p.Y)); 
            else m_viewer.ZoomOut(new CPoint((int)p.X, (int)p.Y));
            m_viewer.Draw(gridDrawing);
        }

        private void gridBitmapSource_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point p = e.GetPosition(imageBitmapSource);
            m_viewer.p_cpLBD = new CPoint((int)p.X, (int)p.Y);
            m_viewer.p_bLBD = true;
        }

        private void gridBitmapSource_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            m_viewer.p_bLBD = false;
        }

        private void gridBitmapSource_MouseLeave(object sender, MouseEventArgs e)
        {
            if (m_viewer == null) return;
            m_viewer.p_bLBD = false;
        }
        private void gridBitmapSource_KeyDown(object sender, KeyEventArgs e)
        {
            if (m_viewer == null) return;
        }
        #endregion

        #region Select Memory
        private void comboBoxGroup_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            m_viewer.p_memoryData = null;
            if (comboBoxGroup.SelectedValue == null) return; 
            string sGroup = comboBoxGroup.SelectedValue.ToString();
            MemoryGroup group = m_pool.GetGroup(sGroup);
            if (group == null) return;
            comboBoxMemory.ItemsSource = group.m_asMemory;
            comboBoxMemory.SelectedIndex = -1;
        }

        List<int> m_aMemoryIndex = new List<int>(); 
        private void comboBoxMemory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            m_viewer.p_memoryData = null;
            if (comboBoxGroup.SelectedValue == null) return;
            string sGroup = comboBoxGroup.SelectedValue.ToString();
            MemoryGroup group = m_pool.GetGroup(sGroup);
            if (group == null) return;
            if (comboBoxMemory.SelectedValue == null) return; 
            string sMemory = comboBoxMemory.SelectedValue.ToString();
            m_viewer.p_memoryData = group.GetMemory(sMemory);
            comboBoxIndex.ItemsSource = null;
            m_aMemoryIndex.Clear();
            for (int n = 0; n < m_viewer.p_memoryData.p_nCount; n++) m_aMemoryIndex.Add(n);
            comboBoxIndex.ItemsSource = m_aMemoryIndex;
            comboBoxIndex.SelectedIndex = 0; 
        }
        #endregion

        private void memuClear_Click(object sender, RoutedEventArgs e)
        {
            
            MemoryData tttt = m_viewer.p_memoryData;

            IntPtr point = m_viewer.p_memoryData.GetPtr(m_viewer.p_nMemoryIndex);
            CPoint p_Size = tttt.p_sz;
            byte[] pBuf = new byte[p_Size.X * tttt.p_nByte];

            Parallel.For(0, tttt.p_sz.Y, new ParallelOptions { MaxDegreeOfParallelism = 4 }, (y) =>
            {
                for (int i = 0; i < tttt.p_nCount; i++)
                {
                    Marshal.Copy(pBuf, 0, (IntPtr)((long)tttt.GetPtr(i) + (long)p_Size.X * tttt.p_nByte * y), p_Size.X * tttt.p_nByte);
                }
            });
        }

        private void memuOption_Click(object sender, RoutedEventArgs e)
        {
            MemoryViewerOption optionWindow = new MemoryViewerOption();
            optionWindow.Owner = Window.GetWindow(this);
            optionWindow.SetOverlapOption(m_viewer.p_bRemoveOverlapArea, m_viewer.p_nFov, m_viewer.p_nOverlap);
            optionWindow.SetGVRangeOption(m_viewer.p_memoryData.p_nByte, m_viewer.p_nOffsetFromUpperBit);

            Nullable<bool> result = optionWindow.ShowDialog();
            if (result.HasValue && result.Value)
            {
                // Overlap
                bool bRemoveOverlapArea;
                int nFov, nOverlap;
                optionWindow.GetOverlapOption(out bRemoveOverlapArea, out nFov, out nOverlap);
                m_viewer.p_bRemoveOverlapArea = bRemoveOverlapArea;
                m_viewer.p_nFov = nFov;
                m_viewer.p_nOverlap = nOverlap;

                // Offset from Upper bit
                int nOffsetFromUpperBit;
                optionWindow.GetGVRangeOption(out nOffsetFromUpperBit);
                m_viewer.p_nOffsetFromUpperBit = nOffsetFromUpperBit;

                m_viewer.UpdateBitmapSource();
            }

        }

        private void memuAllClear_Click(object sender, RoutedEventArgs e)
        {

            MemoryData tttt = m_viewer.p_memoryData;

            for (int i = 0; i < m_viewer.p_memoryData.p_nCount; i++)
            {
                IntPtr point = m_viewer.p_memoryData.GetPtr(i);
                CPoint p_Size = tttt.p_sz;
                byte[] pBuf = new byte[p_Size.X * tttt.p_nByte];

                Parallel.For(0, tttt.p_sz.Y, new ParallelOptions { MaxDegreeOfParallelism = 4 }, (y) =>
                {

                    Marshal.Copy(pBuf, 0, (IntPtr)((long)point + (long)p_Size.X * y), p_Size.X * tttt.p_nByte);
                    if (tttt.p_nCount == 3)
                    {
                        Marshal.Copy(pBuf, 0, (IntPtr)((long)tttt.GetPtr(1) + (long)p_Size.X * y), p_Size.X);
                        Marshal.Copy(pBuf, 0, (IntPtr)((long)tttt.GetPtr(2) + (long)p_Size.X * y), p_Size.X);
                    }
                //Marshal.Copy(pBuf, 0, (IntPtr)((long)tttt.GetPtr(0) + (long)p_Size.X * y), p_Size.X * tttt.p_nByte);
                //if (tttt.p_nCount == 3)
                //{
                //    Marshal.Copy(pBuf, 0, (IntPtr)((long)tttt.GetPtr(1) + (long)p_Size.X * y), p_Size.X);
                //    Marshal.Copy(pBuf, 0, (IntPtr)((long)tttt.GetPtr(2) + (long)p_Size.X * y), p_Size.X);
                //}
            });
            }
        }
    }
}
