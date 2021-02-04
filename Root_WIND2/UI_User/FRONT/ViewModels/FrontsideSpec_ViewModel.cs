using RootTools;
using RootTools_Vision;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Root_WIND2.UI_User
{
    class FrontsideSpec_ViewModel : ObservableObject, IPage
    {
        private readonly FrontsideSpec_ImageViewer_ViewModel imageViewerVM;
        public FrontsideSpec_ImageViewer_ViewModel ImageViewerVM
        {
            get => this.imageViewerVM;
        }

        public FrontsideSpec_ViewModel()
        {
            this.imageViewerVM = new FrontsideSpec_ImageViewer_ViewModel();
            this.imageViewerVM.init(GlobalObjects.Instance.GetNamed<ImageData>("FrontImage"), GlobalObjects.Instance.Get<DialogService>());

            m_cInspItem = new ObservableCollection<InspectionItem>();
            p_MaskList = new ObservableCollection<InspectionROI>();
            p_selectedMethodItem = null;
        }

        #region [IPage Interfaces]
        public void SetPage()
        {
            this.ImageViewerVM.DisplayBox();

            LoadRecipe();
        }

        public void LoadRecipe()
        {
            // Mask
            this.p_MaskList.Clear();
            MaskRecipe maskRecipe = GlobalObjects.Instance.Get<RecipeFront>().GetItem<MaskRecipe>();
            for (int i = 0; i < maskRecipe.MaskList.Count; i++)
            {
                InspectionROI roi = new InspectionROI();
                roi.p_Index = i;
                roi.p_Size = maskRecipe.MaskList[i].Area;
                roi.p_Data = maskRecipe.MaskList[i].ToPointLineList();
                roi.p_Color = maskRecipe.MaskList[i].ColorIndex;
                this.p_MaskList.Add(roi);
            }


            // Inspectio Item
            p_cInspItem.Clear();
            RecipeFront recipe = GlobalObjects.Instance.Get<RecipeFront>();

            foreach (ParameterBase parameterBase in recipe.ParameterItemList)
            {
                InspectionItem item = new InspectionItem();

                int selectMethod = 0;
                for (int i = 0; i < item.p_cInspMethod.Count; i++)
                {
                    if (item.p_cInspMethod[i].InspectionType.Name == parameterBase.InspectionType.Name)
                    {
                        item.p_cInspMethod[i] = (ParameterBase)parameterBase.Clone();
                        selectMethod = i;
                        break;
                    }
                }

                item.ComboBoxItemChanged_Mask += ComboBoxItemChanged_Mask_Callback;
                item.ComboBoxItemChanged_Method += ComboBoxItemChanged_Method_Callback;
                item.ButtonClicked_Delete += ButtonClicked_Delete_Callback;

                p_cInspItem.Add(item);

                item.p_InspMethod = item.p_cInspMethod[selectMethod];
            }

            if (p_cInspItem.Count > 0)
                p_selectedInspItem = p_cInspItem[0];

            SetParameter();
        }

         #endregion

        #region Property

        private ObservableCollection<InspectionItem> m_cInspItem;
        public ObservableCollection<InspectionItem> p_cInspItem
        {
            get
            {
                return m_cInspItem;
            }
            set
            {
                SetProperty(ref m_cInspItem, value);
            }
        }



        private InspectionROI m_selectedMask;
        public InspectionROI p_SelectedMask
        {
            get => this.m_selectedMask;
            set
            {
                SetProperty<InspectionROI>(ref m_selectedMask, value);
            }
        }

        private ObservableCollection<InspectionROI> m_MaskList;
        public ObservableCollection<InspectionROI> p_MaskList
        {
            get => m_MaskList;
            set
            {
                SetProperty<ObservableCollection<InspectionROI>>(ref m_MaskList, value);
            }
        }

        private InspectionItem m_selectedInspItem;
        public InspectionItem p_selectedInspItem
        {
            get
            {
                return m_selectedInspItem;
            }
            set
            {
                SetProperty(ref m_selectedInspItem, value);
                if (m_selectedInspItem != null)
                    p_selectedMethodItem = m_selectedInspItem.p_InspMethod;
            }
        }

        private ParameterBase m_selectedMethodItem;
        public ParameterBase p_selectedMethodItem
        {
            get
            {
                return m_selectedMethodItem;
            }
            set
            {
                SetProperty(ref m_selectedMethodItem, value);
            }
        }

        #endregion


        public void ComboBoxItemChanged_Mask_Callback(object obj, EventArgs args)
        {
            InspectionROI mask = (InspectionROI)obj;
        }

        public void ComboBoxItemChanged_Method_Callback(object obj, EventArgs args)
        {
            ParameterBase param = obj as ParameterBase;
            p_selectedMethodItem = param;
            SetParameter();
        }

        public void ButtonClicked_Delete_Callback(object obj, EventArgs args)
        {
            InspectionItem item = obj as InspectionItem;

            p_cInspItem.Remove(item);
            for (int i = 0; i < p_cInspItem.Count; i++)
            {
                p_cInspItem[i].p_Index = i;
            }
            SetParameter();
        }

        public void SetParameter()
        {
            List<ParameterBase> paramList = new List<ParameterBase>();
            foreach (InspectionItem item in p_cInspItem)
            {
                if (item.p_InspMethod is IMaskInspection)
                {
                    if (item.p_InspROI != null)
                    {
                        ((IMaskInspection)item.p_InspMethod).MaskIndex = item.p_InspROI.p_Index;
                    }
                }

                if (item.p_InspMethod is IColorInspection)
                {
                    ((IColorInspection)item.p_InspMethod).IndexChannel = item.p_InspChannel;
                }

                paramList.Add(item.p_InspMethod);
            }

            RecipeFront recipe = GlobalObjects.Instance.Get<RecipeFront>();
            recipe.ParameterItemList = paramList;
        }

        

        #region ICommand
        public ICommand btnAddInspItem
        {
            get
            {
                return new RelayCommand(() =>
                {
                    InspectionItem item = new InspectionItem();
                    //item.p_cInspROI = p_ROI_Viewer.p_cInspROI;
                    item.p_Index = p_cInspItem.Count();
                    item.ComboBoxItemChanged_Mask += ComboBoxItemChanged_Mask_Callback;
                    item.ComboBoxItemChanged_Method += ComboBoxItemChanged_Method_Callback;
                    item.ButtonClicked_Delete += ButtonClicked_Delete_Callback;

                    p_cInspItem.Add(item);
                    SetParameter();
                });
            }
        }
        public ICommand ComboBoxSelectionChanged_MethodItem
        {
            get
            {
                return new RelayCommand(() =>
                {
                    m_selectedInspItem.p_InspMethod = p_selectedInspItem.p_InspMethod;
                    SetParameter();
                });
            }
        }
        #endregion

    }
}
