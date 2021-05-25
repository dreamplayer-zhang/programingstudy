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

        public List<IntPtr> PtrList;

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
        public SharedBufferInfo(IntPtr sharedBufferR_GRAY, int width, int height, int byteCnt)
        {
            this.PtrR_GRAY = sharedBufferR_GRAY;
            this.PtrG = IntPtr.Zero;
            this.PtrB = IntPtr.Zero;

            this.Width = width;
            this.Height = height;
            this.ByteCnt = byteCnt;

            this.MemoryID = new MemoryID();

            PtrList = new List<IntPtr>();
            PtrList.Add(this.PtrR_GRAY);
        }
        public SharedBufferInfo(IntPtr sharedBufferR_GRAY, int width, int height, int byteCnt, IntPtr sharedBufferG, IntPtr sharedBufferB)
        {
            this.PtrR_GRAY = sharedBufferR_GRAY;
            this.PtrG = sharedBufferG;
            this.PtrB = sharedBufferB;

            this.Width = width;
            this.Height = height;
            this.ByteCnt = byteCnt;

            this.MemoryID = new MemoryID();

            PtrList = new List<IntPtr>();
            PtrList.Add(this.PtrR_GRAY);
            PtrList.Add(this.PtrG);
            PtrList.Add(this.PtrB);
        }
        public SharedBufferInfo(IntPtr sharedBufferR_GRAY, int width, int height, int byteCnt, IntPtr sharedBufferG, IntPtr sharedBufferB, MemoryID memoryID)
        {
            this.PtrR_GRAY = sharedBufferR_GRAY;
            this.PtrG = sharedBufferG;
            this.PtrB = sharedBufferB;

            this.Width = width;
            this.Height = height;
            this.ByteCnt = byteCnt;

            this.MemoryID = memoryID;

            PtrList = new List<IntPtr>();
            PtrList.Add(this.PtrR_GRAY);
            PtrList.Add(this.PtrG);
            PtrList.Add(this.PtrB);
        }

        public SharedBufferInfo(int width, int height, int byteCnt, List<IntPtr> ptrList)
        {
            if (ptrList.Count > 0)
                this.PtrR_GRAY = ptrList[0];
            else
                this.PtrR_GRAY = IntPtr.Zero;

            if (ptrList.Count > 1)
                this.PtrG = ptrList[1];
            else
                this.PtrG = IntPtr.Zero;

            if (ptrList.Count > 2)
                this.PtrB = ptrList[2];
            else
                this.PtrB = IntPtr.Zero;

            this.Width = width;
            this.Height = height;
            this.ByteCnt = byteCnt;

            this.MemoryID = new MemoryID();

            PtrList = ptrList;
        }
    }
}
