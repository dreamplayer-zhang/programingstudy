using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RootTools_Vision
{
    public class PreInspection : IWork
    {

        #region IWork 멤버

        public WORK_TYPE TYPE
        {
            get { return WORK_TYPE.PreInspection; }
        }

        public void SetImageBuffer(byte[] _buffer, int _width, int _height)
        {

        }

        #endregion
    }
}
