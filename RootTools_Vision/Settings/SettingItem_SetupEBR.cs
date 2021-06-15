using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RootTools_Vision
{
	public class SettingItem_SetupEBR : SettingItem
	{
		public SettingItem_SetupEBR(string[] _treeViewPath) : base(_treeViewPath)
		{

		}

		[Category("Klarf")]
		[DisplayName("Use Klarf")]
		public bool UseKlarf
		{
			get => this.useKlarf;
			set => this.useKlarf = value;
		}
		private bool useKlarf = true;

		[Category("Klarf")]
		[DisplayName("Klarf Save Path")]
		public string KlarfSavePath
		{
			get
			{
				return klarfSavePath;
			}
			set
			{
				klarfSavePath = value;
			}
		}
		private string klarfSavePath = "D:\\Klarf";

		[Category("Klarf")]
		[DisplayName("Circle Image Size Width")]
		public int OutputImageSizeWidth
		{
			get
			{
				return outputImageSizeWidth;
			}
			set
			{
				outputImageSizeWidth = value;
			}
		}
		private int outputImageSizeWidth = 1000;


		[Category("Klarf")]
		[DisplayName("Circle Image Size Height")]
		public int OutputImageSizeHeight
		{
			get
			{
				return outputImageSizeHeight;
			}
			set
			{
				outputImageSizeHeight = value;
			}
		}
		private int outputImageSizeHeight = 1000;

		[Category("Klarf")]
		[DisplayName("Circle Thickness")]
		public int Thickness
		{
			get
			{
				return thickness;
			}
			set
			{
				thickness = value;
			}
		}
		private int thickness = 300;

		[Category("Common")]
		[DisplayName("Measurement Image Path")]
		public string MeasureImagePath
		{
			get
			{
				return measureImagePath;
			}
			set
			{
				measureImagePath = value;
			}
		}
		private string measureImagePath = "D:\\MeasurementImage";
	}
}
