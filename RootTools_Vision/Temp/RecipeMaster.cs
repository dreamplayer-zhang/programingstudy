using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RootTools_Vision
{
    public class RecipeMaster
    {
		private int originImageX;
		private int originImageY;

		private int originDieX;
		private int originDieY;

		private int dieSizeX;
		private int dieSizeY;

		public int OriginImageX
		{
			get { return originImageX; }
			set { originImageX = value; }
		}

		public int OriginImageY
		{
			get { return originImageY; }
			set { originImageY = value; }
		}

		public int OriginDieX
		{
			get { return originDieX; }
			set { originDieX = value; }
		}

		public int OriginDieY
		{
			get { return originDieY; }
			set { originDieY = value; }
		}

		public int DieSizeX
		{
			get { return dieSizeX; }
			set { dieSizeX = value; }
		}

		public int DieSizeY
		{
			get { return dieSizeY; }
			set { dieSizeY = value; }
		}
	}
}
