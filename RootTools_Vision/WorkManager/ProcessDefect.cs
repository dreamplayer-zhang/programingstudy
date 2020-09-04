using RootTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RootTools_Vision
{
    class ProcessDefect : IWork
    {
        /// <summary>
        /// Defect 정보 Temp Table Insert
        /// </summary>
        #region IWork 멤버

        public WORK_TYPE TYPE
        {
            get { return WORK_TYPE.PreInspection; }
        }

        public void SetImageBuffer(byte[] _buffer, int _width, int _height)
        {

        }

        #endregion

        void AddDefect(int nThreadID)
        {
            //DatabaseManager.Instance.InsertDefect(nThreadID);
        }
    }
}
