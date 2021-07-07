using RootTools;
using RootTools.Control;
using RootTools_Vision;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Root_EFEM;

namespace Root_WindII
{
    public class PositionConverter
    {

        public static CPoint ConvertImageToVRS(Vision_Frontside module, CPoint imgPos)
        {
            RecipeFront recipe = GlobalObjects.Instance.Get<RecipeFront>();
            GrabModeFront grabMode = module.GetGrabMode(recipe.CameraInfoIndex);

            AxisXY axisXY = module.AxisXY;
            CPoint cpMemoryOffset = new CPoint(grabMode.m_cpMemoryOffset);

            int nMMPerUM = 1000;

            double dXScale = grabMode.m_dTargetResX_um * 10;

            int nWaferSizeY_px = Convert.ToInt32(grabMode.m_nWaferSize_mm * nMMPerUM / grabMode.m_dTargetResY_um);  // 웨이퍼 영역의 Y픽셀 갯수
            int nTotalTriggerCount = Convert.ToInt32(grabMode.m_dTrigger * nWaferSizeY_px);   // 스캔영역 중 웨이퍼 스캔 구간에서 발생할 Trigger 갯수
            int nScanOffset_pulse = 40000;


            double axisStartX = grabMode.m_rpAxisCenter.X + nWaferSizeY_px * dXScale / 2 - grabMode.m_GD.m_nFovSize / 2 * dXScale;
            double axisStartY = grabMode.m_rpAxisCenter.Y - nTotalTriggerCount / 2 - nScanOffset_pulse;

            double dPosX = axisStartX - imgPos.X * grabMode.m_dRealResX_um * 10;
            double dPosY = axisStartY + imgPos.Y * grabMode.m_dRealResY_um * 10;

            dPosX += grabMode.m_dTDIToVRSOffsetX;
            dPosY += grabMode.m_dTDIToVRSOffsetY;

            if (dPosX < 0 || dPosY < 0)
                return null;

            return new CPoint((int)dPosX, (int)dPosY);
        }
    }
}
