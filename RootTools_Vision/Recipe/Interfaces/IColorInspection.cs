﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RootTools_Vision
{
    public interface IColorInspection
    {
        IMAGE_CHANNEL IndexChannel
        {
            get;
            set;
        }
    }
}
