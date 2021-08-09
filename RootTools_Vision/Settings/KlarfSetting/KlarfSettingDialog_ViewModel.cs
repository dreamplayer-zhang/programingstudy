using RootTools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RootTools_Vision
{
	public class KlarfSettingDialog_ViewModel : ObservableObject, IDialogRequestClose
    {
        private KlarfSettingItem_Edgeside item_Edgeside;

        public KlarfSettingItem_Edgeside Item_Edgeside
        {
            get { return item_Edgeside; }
            set
            {
                SetProperty(ref this.item_Edgeside, value);
            }
        }

        private FinebinSpecificationLimit selectedItem;
        public FinebinSpecificationLimit SelectedItem
		{
            get { return selectedItem; }
            set
			{
                SetProperty(ref this.selectedItem, value);
			}
		}


        #region [Commands]
        public event EventHandler<DialogCloseRequestedEventArgs> CloseRequested;

        public RelayCommand btnOKCommand
        {
            get
            {
                return new RelayCommand(OnButtonClickedOK);
            }
        }
        public RelayCommand btnCancelCommand
        {
            get
            {
                return new RelayCommand(OnButtonClickedCancel);
            }
        }

        public RelayCommand ClickRightCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (SelectedItem != null)
                        Item_Edgeside.SLList.Remove(SelectedItem);
                });
            }
        }        
        #endregion

        #region [Commnads Callback]
        public void OnButtonClickedOK()
        {
            Save();
            CloseRequested(this, new DialogCloseRequestedEventArgs(true));
        }

        public void OnButtonClickedCancel()
        {
            CloseRequested(this, new DialogCloseRequestedEventArgs(false));
        }
        #endregion

        public KlarfSettingDialog_ViewModel(/*KlarfSettingItem item*/ KlarfSettingItem_Edgeside item)
		{
			Item_Edgeside = item;
            Load();
		}

        public void Load()
		{
            //Item_Edgeside = (KlarfSettingItem_Edgeside)Item_Edgeside.Load();
        }

        public void Save()
        {
            Item_Edgeside.Save();
        }
    }
}
