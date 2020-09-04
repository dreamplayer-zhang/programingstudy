using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RootTools_Vision
{
    public class SurfaceParameter : IParameter
    {
        private int threshold;
        private int minSize;
        private bool isDark;

        public bool IsDark
        {
            get { return isDark; }
            set { isDark = value; }
        }


        public int Threshold
        {
            get { return threshold; }
            set { threshold = value; }
        }

        public int MinSize
        {
            get { return minSize; }
            set { minSize = value; }
        }


        #region [IParameter]
        public bool Write()
        {
            return true;
        }

        public bool Read()
        {
            return true;
        }

        public object CopyTo()
        {
            return this.MemberwiseClone();
        }
        #endregion
    }
}
