using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RootTools_Vision
{
    public class SettingItem_SetupCamera : SettingItem
    {
        public SettingItem_SetupCamera(string[] _treeViewPath) : base(_treeViewPath)
        {

        }

        #region [Items]

        [Category("Align Camera")]
        [DisplayName("Feature Image Path")]
        public string FeatureImagePath
        {
            get
            {
                return featureImagePath;
            }
            set
            {
                featureImagePath = value;
            }
        }
        private string featureImagePath = @"C:\Root\Setup\AlignFeatureImages";

        #endregion
    }
}
