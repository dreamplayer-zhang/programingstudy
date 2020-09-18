using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RootTools;
using System.Drawing;
using System.Xml.Serialization;

namespace RootTools_Vision
{
  
    public class Image_Bundle
    {
        // 이미지 번들.
        // 이미지 등록함수
        // 이미지 제거 함수

        public Position_Type m_Type;
        public List<byte[]> m_byteImageList;
        List<Image> m_ImageDataList;
        

        public Image_Bundle()
        {
        }
        public Image_Bundle(Position_Type _Type)
        {
            m_ImageDataList = new List<Image>();
            m_byteImageList = new List<byte[]>();
            m_Type = _Type;
        }
        public Position_Type GetPositionType()
        {
            return m_Type;
        }

        public void AddImageToBundle(Image _image)
        {
            m_ImageDataList.Add(_image);

            ImageConverter converter = new ImageConverter();
            byte[] temp = (byte[])converter.ConvertTo(_image, typeof(byte[]));
            m_byteImageList.Add(temp);
        }

        public void SaveImageBundle()
        {
            string sPath = "";
            int nCnt = 1;

            ImageConverter con = new ImageConverter();
            foreach (byte[] byteArray in m_byteImageList)
            {
                System.Drawing.Image img = (System.Drawing.Image)con.ConvertFrom(byteArray);
                sPath = string.Format(@"C:\Wind2\ResultFile_{0}.bmp", nCnt++);
                img.Save(sPath);
            }
        }

        public void LoadImageBundle()
        {
        }




    }
}
