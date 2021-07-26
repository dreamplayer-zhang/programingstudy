using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RootTools_Vision
{
    public class SettingItem_Setup : SettingItem
    {
        [Category("Path")]
        [DisplayName("Recipe Root Path")]
        public string RecipeRootPath
        {
            get
            {
                return recipeRootPath;
            }
            set
            {
                recipeRootPath = value;
            }
        }
        private string recipeRootPath = @"C:\Root\Recipe";

        [Category("Path")]
        [DisplayName("Front Recipe Folder Name")]
        public string FrontRecipeFolderName
        {
            get
            {
                return frontRecipeFolderName;
            }
            set
            {
                frontRecipeFolderName = value;
            }
        }
        private string frontRecipeFolderName = @"Front";

        [Category("Path")]
        [DisplayName("Backside Recipe Folder Name")]
        public string BackRecipeFolderName
        {
            get
            {
                return backRecipeFolderName;
            }
            set
            {
                backRecipeFolderName = value;
            }
        }
        private string backRecipeFolderName = @"Back";

        [Category("Path")]
        [DisplayName("Edge Recipe Folder Name")]
        public string EdgeRecipeFolderName
        {
            get
            {
                return edgeRecipeFolderName;
            }
            set
            {
                edgeRecipeFolderName = value;
            }
        }
        private string edgeRecipeFolderName = @"Edge";

        [Category("Path")]
        [DisplayName("EBR Recipe Folder Name")]
        public string EBRRecipeFolderName
        {
            get
            {
                return ebrRecipeFolderName;
            }
            set
            {
                ebrRecipeFolderName = value;
            }
        }
        private string ebrRecipeFolderName = @"Edge";

        public SettingItem_Setup(string[] _treePath) : base(_treePath)
        {

        }
    }
}
