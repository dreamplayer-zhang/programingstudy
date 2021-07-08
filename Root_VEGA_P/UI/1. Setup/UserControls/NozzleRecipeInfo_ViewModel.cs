using Root_VEGA_P.Module;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Root_VEGA_P
{
    public class NozzleRecipeInfo_ViewModel:ObservableObject
    {
        string header;
        string imgsrc;
        NozzleSet nozzleSet;
        public string ImgSrc
        {
            get => imgsrc;
            set => SetProperty(ref imgsrc, value);
        }
        public string Header
        {
            get => header;
            set => SetProperty(ref header, value);
        }
        ObservableCollection<UIElement> nozzleList;
        public ObservableCollection<UIElement> NozzleList
        {
            get => nozzleList;
            set => SetProperty(ref nozzleList, value);
        }
        int nozzleListIdx;
        public int NozzleListIdx
        {
            get => nozzleListIdx;
            set => SetProperty(ref nozzleListIdx, value);
        }
        public NozzleRecipeInfo_Panel Main;
        public NozzleRecipeInfo_ViewModel(string ImgSrc,NozzleSet nozzleSet) 
        {
            Main = new NozzleRecipeInfo_Panel();
            Main.DataContext = this;
            this.ImgSrc = ImgSrc;
            this.Header = nozzleSet.m_sExt;
            this.nozzleSet = nozzleSet;
            nozzleList = new ObservableCollection<UIElement>();
            for(int i=0;i<nozzleSet.p_nNozzle;i++)
            {
                NozzleItem item = new NozzleItem(true, i + 1, 3.3, 60);
                nozzleList.Add(item);
            }
        }
        public ICommand btnOpen
        {
            get => new RelayCommand(() => {
                nozzleSet.FileOpen();
            });
        }
        public ICommand btnSave
        {
            get => new RelayCommand(() => {
                nozzleSet.FileSave();
            });
        }
    }
}
