using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RootTools_Vision
{
    public class EdgePosition : IPosition
    {
        public POSITION_TYPE TYPE => POSITION_TYPE.Feature;

        public void DoPosition()
        {
            throw new NotImplementedException();
        }
    }
}
