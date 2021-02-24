namespace RootTools_Vision
{
    public class SettingItem_SetupBackside : SettingItem
    {
        public SettingItem_SetupBackside(string[] _treeViewPath) : base(_treeViewPath)
        {
            
        }

        private bool boolTest;

        public bool BoolTest
        {
            get { return boolTest; }
            set { boolTest = value; }
        }
    }
}
