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

        #region [EVENT]
        public delegate void REFEventHandler();
        public delegate void ROIEventHandler();
        public delegate void VerticalWireSaveHandler();

        public event REFEventHandler RefCollectionChanged;
        public event ROIEventHandler InspCollectionChanged;
        public event VerticalWireSaveHandler RecipeSave;
        #endregion

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
            get
            {
                this.refItemIdx = (this.refItemIdx < 0) ? 0 : this.refItemIdx;
                this.refItemIdx = (this.refItemIdx > RefCoordListItem.Count - 1) ? RefCoordListItem.Count - 1 : refItemIdx;
                return refItemIdx;
            }
            set
            {
                SetProperty<int>(ref this.refItemIdx, value);
            }
        }

        private int roiItemIdx;
        public int ROIItemIdx
        {
            get
            {
                this.roiItemIdx = (this.roiItemIdx < 0) ? 0 : this.roiItemIdx;
                this.roiItemIdx = (this.roiItemIdx > ROIListItem.Count - 1) ? ROIListItem.Count - 1 : this.roiItemIdx;
                return this.roiItemIdx;
            }
            set
            {
                SetProperty<int>(ref this.roiItemIdx, value);
            }
        }

        private int chipNuminOrigin = 1;
        public int ChipNuminOrigin
        {
            get => this.chipNuminOrigin;
            set
            {
                SetProperty<int>(ref this.chipNuminOrigin, value);

                foreach (UIElement roiItem in ROIListItem)
                {
                    VerticalWire_ROIItem roi = roiItem as VerticalWire_ROIItem;
                    VerticalWire_ROIItem_ViewModel roiItemItem_ViewModel = roi.DataContext as VerticalWire_ROIItem_ViewModel;

                    roiItemItem_ViewModel.RefCoordNum = this.chipNuminOrigin;
                }
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
        }

        public void AddRefItem()
        {
            VerticalWire_RefCoordItem_ViewModel item = new VerticalWire_RefCoordItem_ViewModel(this.RefCoordListItem.Count + 1);
            item.CollectionChanged += RefCoordItem_CollectionChanged;
            this.RefCoordListItem.Add(item.Main);

            for (int i = 0; i < ROIListItem.Count; i++)
            {
                VerticalWire_ROIItem roiItem = ROIListItem[i] as VerticalWire_ROIItem;
                VerticalWire_ROIItem_ViewModel roiItemItem_ViewModel = roiItem.DataContext as VerticalWire_ROIItem_ViewModel;
                roiItemItem_ViewModel.RefCoordNum = ChipNuminOrigin;
            }
        }

        public void AddROIItem()
        {
            VerticalWire_ROIItem_ViewModel roiItem = new VerticalWire_ROIItem_ViewModel(this.ROIListItem.Count + 1, ChipNuminOrigin);
            roiItem.CollectionChanged += InspROIItem_CollectionChanged;
            this.ROIListItem.Add(roiItem.Main);            
        
        }

        public void RemoveRefItem()
        {
            RefCoordListItem.RemoveAt(RefCoordListItem.Count - 1);
        }

        public void RemoveAllROIItem() 
        {
            this.ROIListItem.Clear() ;
        }

        public void RemoveAllRefItem()
        {
            this.RefCoordListItem.Clear();
            this.ChipNuminOrigin = 0;  
        }

        public void RemoveROIItem()
        {
            this.ROIListItem.RemoveAt(ROIListItem.Count - 1);
        }
        private void _AddROI()
        {
            AddROIItem();
        }
        private void _DeleteROI()
        {
            RemoveROIItem();
        }

        #region [Command]
        
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
                    if(this.ChipNuminOrigin - 1 >= 1)
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

        public ICommand AddROI
        {
            get
            {
                return new RelayCommand(_AddROI);
            }
        }

        public ICommand RefCoordClickedCommand
        {
            get => new RelayCommand(() =>
            {
                this.SelectedItemIdx = refItemIdx;
            });
        }

        public ICommand ROIClickedCommand
        {
            get => new RelayCommand(() =>
            {
                this.SelectedItemIdx = ROIItemOffset + roiItemIdx;
            });
        }

        public ICommand ROIUp
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if(this.SelectedItemIdx >= ROIItemOffset)
                    {
                        int itemIdx = this.SelectedItemIdx - ROIItemOffset;

                        VerticalWire_ROIItem roiItem = ROIListItem[itemIdx] as VerticalWire_ROIItem;
                        VerticalWire_ROIItem_ViewModel roiItemItem_ViewModel = roiItem.DataContext as VerticalWire_ROIItem_ViewModel;
                        
                        //for(int i = 0; i < roiItemItem_ViewModel.WirePointList.Count; i++)
                        //{
                        //    roiItemItem_ViewModel.WirePointList[i].X = 1;
                        //    roiItemItem_ViewModel.WirePointList[i].X;
                        //}

                        this.InspCollectionChanged?.Invoke();
                    }
                });
            }
        }

        public ICommand ROIDown
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (SelectedItemIdx >= ROIItemOffset)
                    {
                        int itemIdx = SelectedItemIdx - ROIItemOffset;

                        VerticalWire_ROIItem roiItem = ROIListItem[itemIdx] as VerticalWire_ROIItem;
                        VerticalWire_ROIItem_ViewModel roiItemItem_ViewModel = roiItem.DataContext as VerticalWire_ROIItem_ViewModel;

                        //foreach (Point pt in roiItemItem_ViewModel.WirePointList)
                        //{
                        //    rt.MemoryRect.Top += 1;
                        //    rt.MemoryRect.Bottom += 1;
                        //}
                        this.InspCollectionChanged?.Invoke();
                    }
                });
            }
        }

        public ICommand ROILeft
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (SelectedItemIdx >= ROIItemOffset)
                    {
                        int itemIdx = SelectedItemIdx - ROIItemOffset;

                        VerticalWire_ROIItem roiItem = ROIListItem[itemIdx] as VerticalWire_ROIItem;
                        VerticalWire_ROIItem_ViewModel roiItemItem_ViewModel = roiItem.DataContext as VerticalWire_ROIItem_ViewModel;

                        //foreach (Point pt in roiItemItem_ViewModel.WirePointList)
                        //{
                        //    rt.MemoryRect.Left -= 1;
                        //    rt.MemoryRect.Right -= 1;
                        //}
                        this.InspCollectionChanged?.Invoke();
                    }
                });
            }
        }

        public ICommand ROIRight
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (SelectedItemIdx >= ROIItemOffset)
                    {
                        int itemIdx = SelectedItemIdx - ROIItemOffset;

                        VerticalWire_ROIItem roiItem = ROIListItem[itemIdx] as VerticalWire_ROIItem;
                        VerticalWire_ROIItem_ViewModel roiItemItem_ViewModel = roiItem.DataContext as VerticalWire_ROIItem_ViewModel;

                        //foreach (Point pt in roiItemItem_ViewModel.WirePointList)
                        //{
                        //    rt.MemoryRect.Left += 1;
                        //    rt.MemoryRect.Right += 1;
                        //}
                        this.InspCollectionChanged?.Invoke();
                    }
                });
            }
        }

        public ICommand Save
        {
            get => new RelayCommand(() =>
            {
                this.RecipeSave?.Invoke();
            });
        }
        #endregion

        #region [Callback]
        void InspROIItem_CollectionChanged()
        {
            this.InspCollectionChanged?.Invoke();
        }
        void RefCoordItem_CollectionChanged()
        {
            this.RefCollectionChanged?.Invoke();
        }
        #endregion
    }
}
