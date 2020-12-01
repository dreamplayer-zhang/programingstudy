using RootTools_Vision;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RootTools_Vision
{
    public class PositionParameter : ParameterBase
    {
        public PositionParameter() : base("Position")
        {

        }

        #region [Parameter]
        private int searchRangeX = 100;
        private int searchRangeY = 100;
        private int minScoreLimit = 60;
        #endregion


        #region [Getter Setter]
        public int SearchRangeX
        {
            get => searchRangeX;
            set
            {
                SetProperty<int>(ref searchRangeX, value);
            }
        }
        public int SearchRangeY
        {
            get => searchRangeY;
            set
            {
                SetProperty<int>(ref searchRangeY, value);
            }
        }
        public int MinScoreLimit
        {
            get => minScoreLimit;
            set
            {
                SetProperty<int>(ref minScoreLimit, value);
            }
        }
        #endregion

        public bool Save()
        {
            throw new NotImplementedException();
        }

        public bool Read()
        {
            throw new NotImplementedException();
        }
    }
}
