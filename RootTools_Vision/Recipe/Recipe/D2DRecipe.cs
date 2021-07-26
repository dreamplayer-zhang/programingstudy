using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace RootTools_Vision
{
    public class D2DRecipe : RecipeItemBase
    {
        #region [Parameter]
        private byte[][] _preGolden = new byte[3][];
        private int _preGoldenW = -1;
        private int _preGoldenH = -1;
        #endregion

        #region [Getter Setter]
        [XmlIgnore]
        public byte[][] PreGolden { get => _preGolden; set => _preGolden = value; }
        public int PreGoldenW { get => _preGoldenW; set => _preGoldenW = value; }
        public int PreGoldenH { get => _preGoldenH; set => _preGoldenH = value; }

        #endregion

        public D2DRecipe()
        {

        }

        public override void Clear()
        {

        }

        public override bool Read(string recipePath)
        {
            string goldenImagePath = recipePath + "preGoldenImage.bmp";
            if (File.Exists(goldenImagePath))
            {
                Bitmap bmp = new Bitmap(goldenImagePath);

                this._preGoldenW = bmp.Width;
                this._preGoldenH = bmp.Height;

                byte[] rawColorData = new byte[this._preGoldenW * this._preGoldenH * 3];

                Tools.LoadBitmapToRawdata(goldenImagePath, rawColorData, this._preGoldenW, this._preGoldenH, 3);

                for(int ch = 0; ch < 3; ch++)
                {
                    _preGolden[ch] = new byte[this._preGoldenW * this._preGoldenH];
                }

                Parallel.For(0, this._preGoldenH, i =>
                {
                    for (int j = 0; j < this._preGoldenW; j++)
                    {
                        _preGolden[0][(Int64)i * this._preGoldenW + j] = rawColorData[(Int64)i * this._preGoldenW * 3 + j * 3 + 0];
                        _preGolden[1][(Int64)i * this._preGoldenW + j] = rawColorData[(Int64)i * this._preGoldenW * 3 + j * 3 + 1];
                        _preGolden[2][(Int64)i * this._preGoldenW + j] = rawColorData[(Int64)i * this._preGoldenW * 3 + j * 3 + 2];
                    }
                });
            }

            return true;
        }

        public override bool Save(string recipePath)
        {
            return true;
        }

        public void SetPreGoldenImage(byte[] preGolden, int W, int H)
        {
            _preGoldenH = H;
            _preGoldenW = W;

            Parallel.For(0, this._preGoldenH, i =>
            {
                for (int j = 0; j < this._preGoldenW; j++)
                {
                    _preGolden[0][(Int64)i * this._preGoldenW + j] = preGolden[(Int64)i * this._preGoldenW * 3 + j * 3 + 0];
                    _preGolden[1][(Int64)i * this._preGoldenW + j] = preGolden[(Int64)i * this._preGoldenW * 3 + j * 3 + 1];
                    _preGolden[2][(Int64)i * this._preGoldenW + j] = preGolden[(Int64)i * this._preGoldenW * 3 + j * 3 + 2];
                }
            });
        }
    }
}