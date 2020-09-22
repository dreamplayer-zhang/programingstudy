using RootTools_Vision.UserTypes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RootTools_CLR;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using RootTools_Vision.Temp_Recipe;

namespace RootTools_Vision
{
    public class Position : IWork
    {
        public WORK_TYPE Type => WORK_TYPE.PREPARISON;

        Workplace workplace;

        RecipePosition recipeData;
        ParameterPosition parameterData;

        public void DoWork()
        {
            DoPosition();
        }

        public void SetData(IRecipeData _recipeData, IParameter _parameterData)
        {
            this.recipeData = _recipeData as RecipePosition;
            this.parameterData = _parameterData as ParameterPosition;
        }

        public void SetWorkplace(Workplace _workplace)
        {
            this.workplace = _workplace;
        }

        public bool DoPosition()
        {
            if(this.workplace.MapPositionX == -1 && this.workplace.MapPositionX == -1) // Master
            {
                if (this.recipeData.IndexMaxScoreMasterFeature == -1) // Feature가 셋팅이 안되어 있는 경우 전체 Feature 사용
                {
                    int outX = 0, outY = 0;
                    int maxX = 0, maxY = 0;
                    float score = 0;
                    float maxScore = 0;
                    int i = 0;
                    int maxIndex = 0;

                    foreach(RecipeType_FeatureData feature in this.recipeData.ListMasterFeature)
                    {
                        int startX = (feature.PositionX - this.parameterData.SearchRangeX) < 0 ? 0 : (feature.PositionX - this.parameterData.SearchRangeX);
                        int startY = (feature.PositionY - this.parameterData.SearchRangeY) < 0 ? 0 : (feature.PositionY - this.parameterData.SearchRangeY);
                        int endX = (feature.PositionX + feature.FeatureWidth + this.parameterData.SearchRangeX) >= this.workplace.SharedBufferWidth ? this.workplace.SharedBufferWidth : (feature.PositionX + feature.FeatureWidth + this.parameterData.SearchRangeX);
                        int endY = (feature.PositionY + feature.FeatureHeight + this.parameterData.SearchRangeY) >= this.workplace.SharedBufferHeight ? this.workplace.SharedBufferHeight : (feature.PositionY + feature.FeatureHeight + this.parameterData.SearchRangeY);

                        unsafe
                        {
                            score =  CLR_IP.Cpp_TemplateMatching(
                                (byte*)this.workplace.SharedBuffer.ToPointer(), feature.RawData, &outX, &outY, 
                                this.workplace.SharedBufferWidth, this.workplace.SharedBufferHeight,
                                feature.FeatureWidth, feature.FeatureHeight,
                                startX, startY, endX, endY, 5);
                        }

                        if( score > maxScore)
                        {
                            maxScore = score;
                            maxX = outX;
                            maxY = outY;
                            maxIndex = i;
                        }

                        i++;
                    }


                    if (maxScore < this.parameterData.MinScoreLimit)
                    {
                        return false;
                    }


                    int tplStartX = this.recipeData.ListMasterFeature[maxIndex].PositionX;
                    int tplStartY = this.recipeData.ListMasterFeature[maxIndex].PositionY;
                    int tplW = this.recipeData.ListMasterFeature[maxIndex].FeatureWidth;
                    int tplH = this.recipeData.ListMasterFeature[maxIndex].FeatureHeight;

                    float tplCenterX = (float)this.recipeData.ListMasterFeature[maxIndex].CenterPositionX;
                    float tplCenterY = (float)this.recipeData.ListMasterFeature[maxIndex].CenterPositionY;

                    maxX -= tplStartX;
                    maxY -= tplStartY;


                    // ROI 중심 위치
                    float centerROIX = tplStartX + tplW / 2;
                    float centerROIY = tplStartY + tplH / 2;

                    // Matching 중심 위치
                    float centerMatchingX = (int)(maxX + tplCenterX);
                    float centerMatchingY = (int)(maxY + tplCenterY);

                    int transX = (int)(centerROIX - centerMatchingX);
                    int transY = (int)(centerROIY - centerMatchingY);

                    if (this.workplace.Index == 0)  // Master
                    {
                        this.workplace.SetImagePositionByTrans(transX, transY);
                    }
                }
                else
                {

                }
            }


            //Image img = Bitmap.FromFile(@"D:\test\template images\template3.bmp");
            //Bitmap bitmap = new Bitmap(@"D:\test\template images\template3.bmp");

            //byte[] tplBuffer = new byte[bitmap.Width * bitmap.Height];

            //BitmapData bmpData =
            //    bitmap.LockBits(
            //        new Rectangle(0, 0, bitmap.Width, bitmap.Height), //bitmap 영역
            //        ImageLockMode.ReadOnly,  //읽기 모드
            //        PixelFormat.Format8bppIndexed); //bitmap 형식
            //IntPtr ptr = bmpData.Scan0;  //비트맵의 첫째 픽셀 데이터 주소를 가져오거나 설정합니다.
            //Marshal.Copy(ptr, tplBuffer, 0, bitmap.Width * bitmap.Height);
            //bitmap.UnlockBits(bmpData);


            return true;
        }




    }
}
