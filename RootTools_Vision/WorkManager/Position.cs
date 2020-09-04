using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RootTools_Vision
{

    public class Position : IWork
    {
        #region IWork 멤버

        public WORK_TYPE TYPE
        {
            get { return WORK_TYPE.Position; }
        }

        #endregion

        #region [Variables]
        byte[] pImageBuffer = null;
        int nImageWidth = 0;
        int nImageHeight = 0;

        #endregion

        public Position()
        {
            // Image/ROI/
            // Inspection Data
            // 
        }

        public void SetImageBuffer(byte[] _buffer, int _width, int _height)
        {
            this.pImageBuffer = _buffer;
            this.nImageWidth = _width;
            this.nImageHeight = _height;
        }
    }
}
