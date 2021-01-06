using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RootTools_Vision
{
    /// <summary>
    /// 무조건 RGB 채널 표시되도록하고, Gray는 자동으로
    /// </summary>
    public enum INSPECTION_IMAGE_CHANNEL
    {
        R = 0,
        G = 1,
        B = 2
    }

    public interface IColorInspection
    {
        INSPECTION_IMAGE_CHANNEL IndexChannel
        {
            get;
            set;
        }
    }
}
