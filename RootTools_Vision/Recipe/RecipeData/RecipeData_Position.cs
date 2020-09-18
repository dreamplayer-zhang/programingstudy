using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RootTools;
using Emgu.CV;
using Emgu.Util;
using System.Runtime.InteropServices;
using System.Drawing;

namespace RootTools_Vision
{
    public enum Position_Type
    {
        MasterMark = 0,
        ShotMark = 1,
        ChipMark = 2,
    }

    public class RecipeData_Position
    {
        #region [Graphics XML Serialize 변수(레시피)]
        // 무조건 Public 선언되어야 함  
        byte[] btyePositionFeature;
        public const int m_nPositionTypeNum = 3;
        public List<Image_Bundle> m_PositionImageBundle; // 시리얼 라이즈가 왠지 안됨
        #endregion

        public RecipeData_Position()
        {
            //m_PositionImageBundle = new List<Image_Bundle>();
            //foreach (Position_Type type in Enum.GetValues(typeof(Position_Type)))
            //{
            //    Image_Bundle image_Bundle = new Image_Bundle(type);
            //    m_PositionImageBundle.Add(image_Bundle);
            //}
        }

        public RecipeData_Position(int nCount)
        {
            m_PositionImageBundle = new List<Image_Bundle>();
            for (int i = 0; i < nCount; i++)
            {
                Image_Bundle image_Bundle = new Image_Bundle((Position_Type)i);
                m_PositionImageBundle.Add(image_Bundle);
            }
        }

            public void AddImageToBundle(ImageData _ImageData, Position_Type _Type, CPoint _ptCenter, CRect _rtBoundry)
        {
            // 신규 Position Feature 등록

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

            ImageConverter converter = new ImageConverter();
            Image image = (System.Drawing.Image)converter.ConvertFrom(byteFeatureBuffer);

            Image_Bundle bundle = GetImagebundle(_Type);
            bundle.AddImageToBundle(image);

            //Image image = Image.FromFile(@"C:\Wind2\6.bmp");
            //ImageConverter converter = new ImageConverter();
            //btyePositionFeature = (byte[])converter.ConvertTo(image, typeof(byte[]));
        }

        public void AddImageToBundle()
        {
            Image image1 = Image.FromFile(@"C:\Wind2\1.bmp");
            Image image2 = Image.FromFile(@"C:\Wind2\2.bmp");
            Image image3 = Image.FromFile(@"C:\Wind2\3.bmp");
            Image image4 = Image.FromFile(@"C:\Wind2\4.bmp");
            Image image5 = Image.FromFile(@"C:\Wind2\5.bmp");
            Image image6 = Image.FromFile(@"C:\Wind2\6.bmp");
            Image image7 = Image.FromFile(@"C:\Wind2\7.bmp");

            List<Image> images;
            images = new List<Image>();
            images.Add(image1);
            images.Add(image2);
            images.Add(image3);
            images.Add(image4);
            images.Add(image5);
            images.Add(image6);
            images.Add(image7);

            Image_Bundle bundle = GetImagebundle(Position_Type.MasterMark);

            foreach (Image item in images)
            {
                bundle.AddImageToBundle(item);
            }
        }


        public void SaveImageBundle()
        {
            Image_Bundle bundle = GetImagebundle(Position_Type.MasterMark);
            bundle.SaveImageBundle();
        }

            public Image_Bundle GetImagebundle(Position_Type _Type)
        {
            int nIndex = Convert.ToInt32(_Type);
            return m_PositionImageBundle[nIndex];
        }
    }
}
