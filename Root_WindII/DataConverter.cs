using RootTools;
using RootTools_Vision;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Root_WindII
{
    public class DataConverter
    {
        public static CameraInfo GrabModeToCameraInfo(GrabModeBase grabMode)
        {
            CameraInfo camInfo = new CameraInfo();

            if (grabMode == null)
            {
                return camInfo;
            }
            
            camInfo.RealResX = grabMode.m_dRealResX_um;
            camInfo.RealResY = grabMode.m_dRealResY_um;
            camInfo.TargetResX = grabMode.m_dTargetResX_um;
            camInfo.TargetResY = grabMode.m_dTargetResY_um;

            return camInfo;
        }
    }
}
