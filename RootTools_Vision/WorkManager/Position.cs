using RootTools_Vision.UserTypes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RootTools_CLR;

namespace RootTools_Vision
{
    public class Position : IWork
    {
        public WORK_TYPE Type => WORK_TYPE.PREPARISON;

        Workplace workplace;

        public void DoWork()
        {
            DoPosition();
        }

        public void SetData(object _recipeData, object _parameterData)
        {
            
        }

        public bool DoPosition()
        {
            int tranX = 0;
            int tranY = 0;



            int tplW = 100;
            int tplH = 100;

            byte[] tplBuffer = new byte[tplW * tplH];


            int startX = 0;
            int startY = 0;
            int endX = 0;
            int endY = 0;

            int outX = 0;
            int outY = 0;

            float scoreLimit = 60;

            float score = 0;
            // Position
            unsafe
            {
                score = CLR_IP.Cpp_TemplateMatching((byte*)this.workplace.SharedBuffer.ToPointer(), tplBuffer, &outX, &outY,  this.workplace.SharedBufferWidth, this.workplace.SharedBufferHeight, tplW, tplH, startX, startY, endX, endY,  5);
            }


            if(score < scoreLimit)
            {
                return false;
            }

            //tpl 중심 위치
            float centerTplX = (float)tplW / 2;
            float centerTplY = (float)tplH / 2;


            // ROI 중심 위치
            float centerSrcX = (endX - startX) / 2;
            float centerSrcY = (endY - startY) / 2;

            int transX = (int)(outX + centerTplX);
            int transY = (int)(outY + centerTplY);

            transX = (int)(centerSrcX - transX);
            transY = (int)(centerSrcY - transY);


            if (this.workplace.Index == 0)  // Master
            {
                this.workplace.SetImagePositionByTrans(transX, transY);
            }

            return true;
        }

        public void SetWorkplace(Workplace _workplace)
        {
            this.workplace = _workplace;
        }


    }
}
