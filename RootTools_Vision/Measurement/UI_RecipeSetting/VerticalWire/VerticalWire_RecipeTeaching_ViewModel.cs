using RootTools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace RootTools_Vision
{
    public class VerticalWire_RecipeTeaching_ViewModel : ObservableObject
    {
        public int ROIItemOffset = 1000;
        
        #region [Properties]
        private int selectedItemIdx;

        public int SelectedItemIdx
        {
            get => this.selectedItemIdx;
            set
            {
                SetProperty<int>(ref this.selectedItemIdx, value);
            }
        }

        private int refItemIdx;
        public int RefItemIdx
        {
            get => this.refItemIdx;
            set
            {
                SetProperty<int>(ref this.refItemIdx, value);
            }
        }

        private int roiItemIdx;
        public int ROIItemIdx
        {
            get => this.roiItemIdx;
            set
            {
                SetProperty<int>(ref this.roiItemIdx, value);
            }
        }

        private int chipNuminOrigin = 0;
        public int ChipNuminOrigin
        {
            get => this.chipNuminOrigin;
            set
            {
                SetProperty<int>(ref this.chipNuminOrigin, value);
            }
        }

        private ObservableCollection<UIElement> refCoordListItem = new ObservableCollection<UIElement>();
        public ObservableCollection<UIElement> RefCoordListItem
        {
            get
            {
                return refCoordListItem;
            }
            set
            {
                SetProperty(ref refCoordListItem, value);
            }
        }

        private ObservableCollection<UIElement> roiListItem = new ObservableCollection<UIElement>();
        public ObservableCollection<UIElement> ROIListItem
        {
            get
            {
                return roiListItem;
            }
            set
            {
                SetProperty(ref roiListItem, value);
            }
        }
        #endregion

        public VerticalWire_RecipeTeaching_ViewModel()
        {
            this.ChipNuminOrigin = 1;
            VerticalWire_RefCoordItem_ViewModel refItem = new VerticalWire_RefCoordItem_ViewModel(this.ChipNuminOrigin);
            RefCoordListItem.Add(refItem.Main);

            VerticalWire_ROIItem_ViewModel roiItem = new VerticalWire_ROIItem_ViewModel(this.ChipNuminOrigin);
            ROIListItem.Add(roiItem.Main);
        }

        public void AddRefItem()
        {
            VerticalWire_RefCoordItem_ViewModel item = new VerticalWire_RefCoordItem_ViewModel(this.ChipNuminOrigin);
            RefCoordListItem.Add(item.Main);

            VerticalWire_ROIItem_ViewModel roiItem = new VerticalWire_ROIItem_ViewModel(this.ChipNuminOrigin);
            for (int i = 0; i < ROIListItem.Count; i++)
                roiItem.RefCoordNum = chipNuminOrigin;
        }

        public void AddROIItem()
        {
            VerticalWire_ROIItem_ViewModel roiItem = new VerticalWire_ROIItem_ViewModel(this.ChipNuminOrigin);
            ROIListItem.Add(roiItem.Main);            
        
        }

        public void RemoveRefItem()
        {
            RefCoordListItem.RemoveAt(RefCoordListItem.Count - 1);

            VerticalWire_ROIItem_ViewModel roiItem = new VerticalWire_ROIItem_ViewModel(this.ChipNuminOrigin);
            for (int i = 0; i < ROIListItem.Count; i++)
                roiItem.RefCoordNum = chipNuminOrigin;
        }

        public void RemoveROIItem() 
        {
            ROIListItem.RemoveAt(ROIListItem.Count - 1);
        }

        private void _CreateROI()
        {
            AddROIItem();
        }
        private void _DeleteROI()
        {
            RemoveROIItem();
        }

        #region [Command]

        public ICommand LoadedCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                });
            }
        }
        
        public ICommand btnChipNumPlusCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if(this.ChipNuminOrigin + 1 <= 5)
                    {
                        this.ChipNuminOrigin = this.ChipNuminOrigin + 1;
                        AddRefItem();
                    }
                });
            }
        }

        public ICommand btnChipNumMinusCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if(this.ChipNuminOrigin - 1 >= 0)
                    {
                        this.ChipNuminOrigin = this.ChipNuminOrigin - 1;
                        RemoveRefItem();
                    }
                });
            }
        }

        public ICommand DeleteROI
        {
            get
            {
                return new RelayCommand(_DeleteROI);
            }
        }

        public ICommand CreateROI
        {
            get
            {
                return new RelayCommand(_CreateROI);
            }
        }

        public ICommand RefCoordClickedCommand
        {
            get => new RelayCommand(() =>
            {
                SelectedItemIdx = refItemIdx;
            });
        }

        public ICommand ROIClickedCommand
        {
            get => new RelayCommand(() =>
            {
                SelectedItemIdx = ROIItemOffset + roiItemIdx;
            });
        }
        #endregion
    }
}
