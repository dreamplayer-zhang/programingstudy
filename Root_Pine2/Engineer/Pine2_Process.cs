using RootTools;

namespace Root_Pine2.Engineer
{
    public class Pine2_Process : NotifyProperty
    {
        #region UI Binding
        string _sInfo = "OK";
        public string p_sInfo
        {
            get { return _sInfo; }
            set
            {
                if (_sInfo == value) return;
                _sInfo = value;
                OnPropertyChanged();
            }
        }
        #endregion

        public string p_id { get; set; }
        public Pine2_Handler m_handler;
        Log m_log;
        public Pine2_Process(string id, Pine2_Engineer engineer)
        {
            p_id = id;
            m_handler = (Pine2_Handler)engineer.ClassHandler();
            m_log = LogView.GetLog(id);
        }
    }
}
