using System.Collections.Generic;

namespace RootTools_Vision
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

            Load();
        }

        private void CreateSettingItems()
        {
            SettingItems.Add(new SettingItem_Setup(new string[] { "Setup" }));
            SettingItems.Add(new SettingItem_SetupFrontside(new string[] { "Setup", "Frontside" }));
            SettingItems.Add(new SettingItem_SetupBackside(new string[] { "Setup", "Backside" }));
            SettingItems.Add(new SettingItem_SetupEdgeside(new string[] { "Setup", "Edgeside" }));
            SettingItems.Add(new SettingItem_SetupCamera(new string[] { "Setup", "Camera" }));
            SettingItems.Add(new SettingItem_SetupEBR(new string[] { "Setup", "EBR" }));
            SettingItems.Add(new SettingItem_Database(new string[] { "DataBase" }));
        }

        public T GetItem<T>()
        {
            foreach(SettingItem item in this.SettingItems)
            {
                if (item.GetType() == typeof(T))
                    return (T)(object)item;
            }
            return default(T);
        }

        public void Load()
        {
            foreach(SettingItem item in this.SettingItems)
            {
                item.Load();
            }
        }
    }
}
