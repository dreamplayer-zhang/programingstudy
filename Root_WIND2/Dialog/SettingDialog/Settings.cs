using RootTools_Vision;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Root_WIND2
{
    public class Settings
    {

        private List<SettingItem> settingItems = new List<SettingItem>();
        public List<SettingItem> SettingItems
        {
            get => this.settingItems;
        }


        public Settings()
        {
            CreateSettingItems();
        }

        private void CreateSettingItems()
        {
            SettingItems.Add(new SettingItem_SetupFrontside(new string[] { "Setup", "Frontside" }));
            SettingItems.Add(new SettingItem_SetupBackside(new string[] { "Setup", "Backside" }));
        }
    }
}
