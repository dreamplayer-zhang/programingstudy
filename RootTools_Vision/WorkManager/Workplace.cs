using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace RootTools_Vision
{
    public enum WORKPLACE_STATE
    {
        NONE = 0,
        OCCUPIED, // 작업자가 할당됨
        INUSE, // 사용중
        COMPLETED // 작업이 완료됨
    }
    /// <summary>
    /// 작업(Work)를 할 위치
    /// 작업할 메모리(Image) 및 위치, 개수 등..
    /// 칩 하나를 의미
    /// </summary>
    public class Workplace
    {
        private int index;
        private Point subIndex;
        private Point imagePosition;
        private Size workBufferSize;    // 이거는 위치를 바꿔야할 수 도 있음

        private WORKPLACE_STATE state;

        /// <summary>
        /// 일차배열 Index
        /// </summary>
        public int Index
        {
            get { return this.index; }
            set { this.index = value; }
        }

        /// <summary>
        /// 2차배열 인덱스
        /// </summary>
        public Point SubIndex
        {
            get { return subIndex; }
            set { subIndex = value; }
        }

        /// <summary>
        /// 전체 이미지 상에서 좌표
        /// </summary>
        public Point ImagePosition
        {
            get { return this.imagePosition; }
            set { this.imagePosition = value; }
        }

        public Size WorkBufferSize
        {
            get { return this.workBufferSize; }
            set { this.workBufferSize = value; }
        }


        public WORKPLACE_STATE State
        {
            get { return this.state; }
            set { this.state = value; }
        }

        public Workplace()
        {
            index = 0;
            subIndex = new Point(0, 0);
            imagePosition = new Point(0, 0);
            state = WORKPLACE_STATE.NONE;
        }

        public Workplace(int index, Point subIndex, Point pos, Size size)
        {
            this.index = index;
            this.subIndex = subIndex;
            this.imagePosition = pos;
            this.workBufferSize = size;

            state = WORKPLACE_STATE.NONE;
        }


        // Index
        // SubIndex(칩위치) 검사 중 칩번호가 필요한 경우가 있을 수도?
        // 메모리
        // ROI
        // 절대위치
        // ROI는 모든 ROI를 가지고 있고, Inspection에서 ROI를 선택해야함
        // subPos는 Position 뒤에 재배치된 위치로 이값을 다른 검사에서도 포지셔닝 없이 진행하기 위하여 사용할 수 있음
    }
}
