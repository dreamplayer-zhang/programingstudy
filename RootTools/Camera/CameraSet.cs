using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace RootTools.Camera
{
    public class CameraSet : ObservableObject, ITool
    {
        public delegate void dgOnChangeTool();
        public event dgOnChangeTool OnChangeTool;

        #region List Camera
        public ObservableCollection<ICamera> m_aCamera = new ObservableCollection<ICamera>(); 
        public ObservableCollection<ICamera> p_aCamera
        {
            get { return m_aCamera; }
            set { SetProperty(ref m_aCamera, value); }
        }
        public List<string> p_asCamera
        {
            get
            {
                List<string> asCamera = new List<string>();
                foreach (ICamera camera in m_aCamera) asCamera.Add(camera.p_id);
                return asCamera; 
            }
        }

        public void Add(ICamera camera)
        {
            ICamera cam = Get(camera.p_id);
            if (cam != null) return;
            m_aCamera.Add(camera);
            m_toolSetCamera.Add(camera); 
            if (OnChangeTool != null) OnChangeTool(); 
        }

        public ICamera Get(string id)
        {
            foreach (ICamera camera in m_aCamera)
            {
                if (camera.p_id.Contains(id)) return camera;
            }
            return null;
        }
        #endregion

        #region UI
        public UserControl p_ui
        {
            get
            {
                CameraSet_UI ui = new CameraSet_UI();
                ui.Init(this);
                return (UserControl)ui;
            }
        }
        #endregion

        public string p_id { get; set; }
        public string m_sModule; 
        ToolSetCamera m_toolSetCamera;
        Log m_log; 
        public CameraSet(ToolSetCamera toolSetCamera, string sModule, Log log)
        {
            p_id = "Camera";
            m_sModule = sModule; 
            m_toolSetCamera = toolSetCamera;
            m_log = log;
        }

        public void ThreadStop()
        {
        }
    }
}
