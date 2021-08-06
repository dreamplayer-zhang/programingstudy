using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Root_WindII.XmlListViewer_ViewModel;

namespace Root_WindII
{
    public class RACCreate_ViewModel : ObservableObject
    {
        public RACCreate_ViewModel()
        {
            XMLListViewer_VM.OnSelectChanged += SelectItemChange;
        }
        #region [PROPERTY]
        XmlListViewer_ViewModel xmlListViewer_VM = new XmlListViewer_ViewModel();
        public XmlListViewer_ViewModel XMLListViewer_VM
        {
            get => xmlListViewer_VM;
            set
            {
                SetProperty(ref xmlListViewer_VM, value);
            }
        }

        RACRecipeCreateViewer_ViewModel racRecipeCreate_VM = new RACRecipeCreateViewer_ViewModel();
        public RACRecipeCreateViewer_ViewModel RACRecipeCreate_VM
        {
            get => racRecipeCreate_VM;
            set
            {
                SetProperty(ref racRecipeCreate_VM, value);
            }
        }
        #endregion

        #region [FUNCTION]
        void SelectItemChange(object obj)
        {
            if (obj == null)
                return;
            string name = ((ListFileInfo)obj).FileName;
            RACRecipeCreate_VM.XMLFileName = name;
            RACRecipeCreate_VM.Refresh();
        }

        #endregion
    }
}
