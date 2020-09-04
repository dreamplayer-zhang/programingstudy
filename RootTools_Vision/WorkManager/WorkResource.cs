using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RootTools_Vision
{
    /// <summary>
    /// WorkResource가 하는 일
    /// - 모든 작업에서 공유하는 리소스(이미지 등)
    /// 
    /// </summary>
    public class WorkResource
    {
        #region [Member Variables]

        // 다음 변수들을 readonly로 선언하는 이유는 한번 값이 설정한 뒤에 변경되는 일이 없도록 하기 위해서인다
        // 만약 값이 변경되려면 WorkResource 객체를 다시 생성해야한다.
        readonly IntPtr ptrImageBuffer;
        readonly int nImageSizeX = 0;
        readonly int nImageSizeY = 0;


        public IntPtr PointerImageBuffer
        {
            get { return this.ptrImageBuffer; }
        }

        public int ImageSizeX
        {
            get { return this.nImageSizeX; }
        }

        public int ImageSizeY
        {
            get { return this.nImageSizeY; }
        }


        Parameter parameter;

        #endregion

        /// <summary>
        /// 현재는 WorkResource가 굳이 필요없을 수 도 있음
        /// </summary>

        public WorkResource(IntPtr _ptrImageBuffer, int _ImageSizeX, int _ImageSizeY/*, Parameter _param*/)
        {
            this.ptrImageBuffer = _ptrImageBuffer;
            this.nImageSizeX = _ImageSizeX;
            this.nImageSizeY = _ImageSizeY;

            //this.parameter = (Parameter)_param.Clone();
        }
    }
}
