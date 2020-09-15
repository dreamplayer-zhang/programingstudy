﻿using RootTools_Vision.UserTypes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RootTools_Vision
{
    public class Alignment : IWork
    {
        public WORK_TYPE Type => WORK_TYPE.PREPARISON;

        Workplace workplace;

        public void DoWork()
        {
            DoPosition();
        }

        public void DoPosition()
        {
            int tranX = 0;
            int tranY = 0;

            // Position


            if(this.workplace.Index == 0)  // Master
            {
                this.workplace.SetImagePositionByTrans(tranX, tranY);
            }
        }

        public void SetWorkplace(Workplace _workplace)
        {
            this.workplace = _workplace;
        }
    }
}
