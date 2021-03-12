using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RootTools.Database
{
	public class Data
	{
		public virtual Rect GetRect()
		{
			return new Rect();
		}

		public virtual float GetWidth()
		{
			return 0;
		}

		public virtual float GetHeight()
		{
			return 0;
		}
	}
}
