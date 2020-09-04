using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RootTools_Vision
{
    abstract public class Inspection : IWork
    {

        #region IWork 멤버

        public WORK_TYPE TYPE
        {
            get { return WORK_TYPE.Inspection; }
        }
        #endregion



        #region [Member Variables]

        protected IntPtr ptrImageBuffer;
        protected Size szImageBuffer;
        protected Size szInspectionBuffer;
        protected byte[] bufInspection;

        private InspectionParameter m_InspParam;

        #endregion


        /// <summary>
        /// 모든 Inspection에서 공통적으로 필요한 요소 Setting
        /// - 전체 이미지 핸들
        /// - 전체 이미지 사이즈
        /// - 
        /// </summary>
        public void SetData(IntPtr _ptrImageBuffer, Size _szImageBuffer, Size _szInspectionBuffer)
        {
            this.ptrImageBuffer = _ptrImageBuffer;
            this.szImageBuffer = _szImageBuffer;
            this.szInspectionBuffer = _szInspectionBuffer;
        }



    }
}
