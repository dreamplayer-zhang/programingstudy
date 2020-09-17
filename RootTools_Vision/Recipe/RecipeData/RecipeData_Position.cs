using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RootTools;
using Emgu.CV;
using Emgu.Util;
using System.Runtime.InteropServices;

namespace RootTools_Vision
{
    public class RecipeData_Position
    {
        // Master
        // Shot
        // Chip
        List<Image_Bundle> m_PositionImageBundle;

        public RecipeData_Position()
        {
            m_PositionImageBundle = new List<Image_Bundle>();
            foreach (Position_Type type in Enum.GetValues(typeof(Position_Type)))
            {
                Image_Bundle image_Bundle = new Image_Bundle(type);
                m_PositionImageBundle.Add(image_Bundle);
            }
        }

        public void AddImageToBundle(ImageData _ImageData, Position_Type _Type, CPoint _ptCenter, CRect _rtBoundry)
        {
            // 좌표와 이미지 처리
            Image_Bundle bundle = GetImagebundle(_Type);

            byte[] byteFeatureBuffer = new byte[_rtBoundry.Width * _rtBoundry.Height];
            int nStride = (int)_ImageData.p_Stride;
            for (int y = 0; y < _rtBoundry.Height; y++)
            {
                Marshal.Copy(
                _ImageData.GetPtr() + _ptCenter.X + (y + _ptCenter.Y) * nStride, // source
                byteFeatureBuffer,
                _rtBoundry.Width * y,
                _rtBoundry.Width
                );
            }

            bundle.AddImageToBundle();
        }

        public Image_Bundle GetImagebundle(Position_Type _Type)
        {
            int nIndex = Convert.ToInt32(_Type);
            return m_PositionImageBundle[nIndex];
        }



    }
}
