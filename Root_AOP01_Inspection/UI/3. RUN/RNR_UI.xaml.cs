using RootTools;
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
using System.Windows.Threading;

namespace Root_AOP01_Inspection.UI._3._RUN
{
    /// <summary>
    /// RNR_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class RNR_UI : Window
    {
        AOP01_Engineer m_engineer;
        AOP01_Handler m_handler;
        //RNR m_rnr;
        //class RNR : NotifyProperty
        //{
        //    int _nRNRCount = 0;
        //    public int p_nRNRCount
        //    {
        //        get { return _nRNRCount; }
        //        set
        //        {
        //            if (_nRNRCount == value) return;
        //            _nRNRCount = value;
        //            OnPropertyChanged();
        //        }
        //    }
        //}
        
        public RNR_UI()
        {
            InitializeComponent();
            InitTimer();
        }
        public void Init(AOP01_Engineer engineer)
        {
            //RNR rnr = new RNR();
            //m_rnr = rnr;
            m_engineer = engineer;
            m_handler = engineer.m_handler;
            RNRCount.DataContext = m_handler.p_nRnRCount;
        }
        DispatcherTimer m_timer = new DispatcherTimer();
        void InitTimer()
        {
            m_timer.Interval = TimeSpan.FromMilliseconds(20);
            m_timer.Tick += M_timer_Tick;
            m_timer.Start();
        }
        private void M_timer_Tick(object sender, EventArgs e)
        {
            RNRCount.Text =Convert.ToString(m_handler.p_nRnRCount);
        }
    }
}
