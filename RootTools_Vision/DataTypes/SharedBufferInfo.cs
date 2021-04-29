using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RootTools_Vision
{
    public struct SharedBufferInfo
    {
        public IntPtr PtrR_GRAY;
        public IntPtr PtrG;
        public IntPtr PtrB;

        public int Width;
        public int Height;
        public int ByteCnt;
        public List<IntPtr> liPtr;

        public MemoryID MemoryID;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sharedBufferR_GRAY"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="byteCnt"></param>
        /// <param name="sharedBufferG">없을 경우 IntPtr.Zero</param>
        /// <param name="sharedBufferB">없을 경우 IntPtr.Zero</param>
        /// <param name="liPtr">없을 경우 null</param>
        
        public SharedBufferInfo(IntPtr sharedBufferR_GRAY, int width, int height, int byteCnt, IntPtr sharedBufferG, IntPtr sharedBufferB, List<IntPtr> liPtr = null)
        {
            this.PtrR_GRAY = sharedBufferR_GRAY;
            this.PtrG = sharedBufferG;
            this.PtrB = sharedBufferB;
            this.liPtr = liPtr;

            this.Width = width;
            this.Height = height;
            this.ByteCnt = byteCnt;

            this.MemoryID = new MemoryID();
        }
        public SharedBufferInfo(IntPtr sharedBufferR_GRAY, int width, int height, int byteCnt, IntPtr sharedBufferG, IntPtr sharedBufferB, MemoryID memoryID, List<IntPtr> liPtr = null)
        {
            this.PtrR_GRAY = sharedBufferR_GRAY;
            this.PtrG = sharedBufferG;
            this.PtrB = sharedBufferB;
            this.liPtr = liPtr;

            this.Width = width;
            this.Height = height;
            this.ByteCnt = byteCnt;

            this.MemoryID = memoryID;
        }
    }
}
