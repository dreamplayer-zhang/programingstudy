using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Root_VEGA_P_Vision
{
    public class ImageViewerBase_ViewModel:ObservableObject
    {
        public ImageViewerBase_Panel Main;
        private UserControl m_CurrentPanel;
        Visibility inspbtnvisible;

        public Visibility InspBtnVisibility
        {
            get => inspbtnvisible;
            set => SetProperty(ref inspbtnvisible, value);
        }
        public UserControl p_SubViewer
        {
            get => m_CurrentPanel;
            set => SetProperty(ref m_CurrentPanel, value);
        }

        public ImageViewerBase_ViewModel()
        {
            Main = new ImageViewerBase_Panel();
            Main.DataContext = this;
        }
    }
}
