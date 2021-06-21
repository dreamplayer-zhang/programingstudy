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
using System.Windows.Shapes;

namespace RootTools.Memory
{
    /// <summary>
    /// MemoryViewerOption.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MemoryViewerOption : Window
    {
        public bool p_bRemoveOverlap
        {
            get { return (bool)chkRemoveOverlap.IsChecked; }
            set { chkRemoveOverlap.IsChecked = value; }
        }
        public int p_nFov
        {
            get { return Convert.ToInt32(txtFov.Text); }
            set { txtFov.Text = value.ToString(); }
        }
        public int p_nOverlap
        {
            get { return Convert.ToInt32(txtOverlap.Text); }
            set { txtOverlap.Text = value.ToString(); }
        }
        int m_nOffsetFromUpperBit = 0;
        public int p_nOffsetFromUpperBit    // 화면표시에 사용할 비트의 상위비트부터의 Offset
        {
            get { return m_nOffsetFromUpperBit; }
            set
            {
                scrollGVRange.Value = value;

                int nStartBitIdx = (m_nByte - 1) * 8 - value;
                int nEndBitIdx = nStartBitIdx + 7;
                lblRangeValue.Content = string.Format("{0} ~ {1}", nStartBitIdx, nEndBitIdx);

                m_nOffsetFromUpperBit = value;
            }
        }
        int m_nByte = 1;
        public int p_nByte
        {
            get { return m_nByte; }
            set
            {
                if ((value - 1) * 8 < m_nOffsetFromUpperBit)
                {
                    m_nOffsetFromUpperBit = 0;
                }

                lblMaxBit.Content = (value * 8) - 1;
                scrollGVRange.Maximum = (value - 1) * 8;

                m_nByte = value;
            }
        }

        public MemoryViewerOption()
        {
            InitializeComponent();
        }

        public void SetOverlapOption(bool bRemoveOverlap, int nFov, int nOverlap)
        {
            p_bRemoveOverlap = bRemoveOverlap;
            p_nFov = nFov;
            p_nOverlap = nOverlap;
        }

        public void SetGVRangeOption(int nByte, int nOffsetOfUpperbit)
        {
            p_nByte = nByte;
            p_nOffsetFromUpperBit = nOffsetOfUpperbit;
        }

        public void GetOverlapOption(out bool bRemoveOverlap, out int nFov, out int nOverlap)
        {
            bRemoveOverlap = p_bRemoveOverlap;
            nFov = p_nFov;
            nOverlap = p_nOverlap;
        }
        public void GetGVRangeOption(out int nOffsetOfUpperbit)
        {
            nOffsetOfUpperbit = p_nOffsetFromUpperBit;
        }

        private void scrollGVRange_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            p_nOffsetFromUpperBit = (int)scrollGVRange.Value;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}
