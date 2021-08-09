using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace RootTools_Vision
{
	public class KlarfSetting
	{
        private KlarfSettingItem_Edgeside settingEdgeKlarf;
        public KlarfSettingItem_Edgeside SettingEdgeKlarf
		{
            get => settingEdgeKlarf;
            set
			{
                settingEdgeKlarf = value;
            }
		}

		public KlarfSetting()
		{
			CreateSettingItems();

			Load();
		}

        private void CreateSettingItems()
        {
            settingEdgeKlarf = new KlarfSettingItem_Edgeside();
        }

        public void Load()
        {
            SettingEdgeKlarf = (KlarfSettingItem_Edgeside)SettingEdgeKlarf.Load();
            if (SettingEdgeKlarf == null)
                SettingEdgeKlarf = new KlarfSettingItem_Edgeside();
            //SettingEdgeKlarf.Load();
        }

        public void Save()
		{
            SettingEdgeKlarf.Save();
        }

    }
}
