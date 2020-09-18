using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RootTools;

namespace RootTools_Vision
{
    public class LineData
    {
        CPoint ptStart;
        int nLength;
        public LineData(CPoint _ptStart, int _nLength)
        {
            ptStart = _ptStart;
            nLength = _nLength;
        }
    }
}
