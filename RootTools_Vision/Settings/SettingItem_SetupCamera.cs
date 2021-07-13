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

        [Category("Align Camera")]
        [DisplayName("Illumination Index List")]
        [Description("조명 Index를 설정합니다. 각 Index는 콤마로 구분합니다.\n ex) 0,1,3,4")]
        public string IlluminationIndexList
        {
            get
            {
                return illuminationIndexList;
            }
            set
            {
                illuminationIndexList = value;
            }
        }
        private string illuminationIndexList = "";

        #endregion
    }
}
