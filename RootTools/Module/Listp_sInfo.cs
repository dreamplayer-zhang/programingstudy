using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace RootTools.Module
{
    /// <summary> Listp_sInfo : p_sInfo에 값을 쓸 때 마다 List에 저장하여 보여 준다 </summary>
    public class Listp_sInfo : NotifyProperty
    {
        #region Info Data
        public class Info
        {
            public string p_sDate { get; set; }
            public string p_sInfo { get; set; }

            public Info(string sInfo)
            {
                p_sDate = DateTime.Now.ToString();
                p_sInfo = sInfo;
            }
        }
        public ObservableCollection<Info> p_aInfo { get; set; }
        #endregion

        #region Timer
        DispatcherTimer m_timer = new DispatcherTimer();
        private void M_timer_Tick(object sender, EventArgs e)
        {
            try
            {
                while (m_qInfo.Count > 0) p_aInfo.Add(m_qInfo.Dequeue());
                while (p_aInfo.Count > 100) p_aInfo.RemoveAt(0);
                OnPropertyChanged("p_aInfo");
            }
            catch { }
        }

        Queue<Info> m_qInfo = new Queue<Info>();
        #endregion

        #region Add Info
        public void Add(string sInfo)
        {
            Info data = new Info(sInfo);
            m_qInfo.Enqueue(data);
        }
        #endregion

        public Listp_sInfo()
        {
            p_aInfo = new ObservableCollection<Info>(); 
            m_timer.Interval = TimeSpan.FromMilliseconds(100);
            m_timer.Tick += M_timer_Tick; 
            m_timer.Start();
        }
    }
}
