using System.Collections.Generic;

namespace RootTools.Camera
{
    public class ToolSetCamera : IToolSet
    {
        public delegate void dgOnToolChanged();
        public event dgOnToolChanged OnToolChanged;

        #region ICameraTools
        public List<ICamera> m_aCamera = new List<ICamera>();

        public void Add(ICamera camera)
        {
            ICamera cam = GetCamera(camera.p_id);
            if (cam != null) return;
            m_aCamera.Add(camera);
            if (OnToolChanged != null) OnToolChanged(); 
        }

        public ICamera GetCamera(string sCamera)
        {
            foreach (ICamera camera in m_aCamera)
            {
                if (camera.p_id == sCamera) return camera; 
            }
            return null; 
        }
        #endregion

        public string p_id { get; set; }

        public ToolSetCamera(string id)
        {
            p_id = id;
        }

        public void ThreadStop()
        {
            foreach (ICamera camera in m_aCamera) camera.ThreadStop();
        }
    }
}
