using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.Util;

namespace RootTools_Vision
{
    public enum Position_Type
    { 
        MasterMark = 0,
        ShotMark = 1,
        ChipMark = 2,
    }

    public class Image_Bundle
    {
        // 이미지 번들.
        // 이미지 등록함수
        // 이미지 제거 함수

        Position_Type m_Type;

        public Image_Bundle(Position_Type _Type)
        {
            m_Type = _Type;
        }
        public Position_Type GetPositionType()
        {
            return m_Type;
        }

        public void AddImageToBundle()
        {
            // 이미지 좌표
            // 이미지 주소
            // -> Mat, 좌표저장
        }

        public void SaveImageBundle()
        {
            string sPath = "";
            //foreach(Mat temp in m_ImageList)
            //{
            //    temp.Save(sPath);
            //}
        }

        public void LoadImageBundle()
        {

        }




    }
}
