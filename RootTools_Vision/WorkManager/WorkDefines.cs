using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RootTools_Vision
{
    #region [Enums]
    public enum WORK_TYPE
    {
        NONE                    = 0b0000000,
        SNAP                    = 0b0000010,
        ALIGNMENT               = 0b0000100,
        INSPECTION              = 0b0001000,
        DEFECTPROCESS           = 0b0010000,
        DEFECTPROCESS_ALL       = 0b0100000,
    }

    public enum WORKMANAGER_STATE
    {
        NONE = 0,
        CREATED,
        READY,
        STOP,
        EXIT,
        CHECK,
        ASSIGN,
        DONE,
    }

    #endregion


    #region [Structs]
    public struct SharedBufferInfo
    {
        public IntPtr PtrR_GRAY;
        public IntPtr PtrG;
        public IntPtr PtrB;

        public int Width;
        public int Height;
        public int ByteCnt;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sharedBufferR_GRAY"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="byteCnt"></param>
        /// <param name="sharedBufferG">없을 경우 IntPtr.Zero</param>
        /// <param name="sharedBufferB">없을 경우 IntPtr.Zero</param>
        public SharedBufferInfo(IntPtr sharedBufferR_GRAY, int width, int height, int byteCnt, IntPtr sharedBufferG, IntPtr sharedBufferB)
        {
            this.PtrR_GRAY = sharedBufferR_GRAY;
            this.PtrG = sharedBufferG;
            this.PtrB = sharedBufferB;

            this.Width = width;
            this.Height = height;
            this.ByteCnt = byteCnt;
        }
    }
    #endregion
}
