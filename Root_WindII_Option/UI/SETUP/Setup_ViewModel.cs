using RootTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Root_WindII_Option
{
    public class Setup_ViewModel : ObservableObject
    {
        public Setup_ViewModel()
        {
            this.menuPanel = new SetupMenuPanel();
            this.menuPanelVM = new SetupMenuPanel_ViewModel();

            this.p_CurrentPanel = this.menuPanel;
            this.p_CurrentPanel.DataContext = this.menuPanelVM;
        }

        #region [UI]
        private readonly SetupMenuPanel menuPanel;
        private readonly SetupMenuPanel_ViewModel menuPanelVM;
        #endregion

        #region [Propeties]
        private UserControl m_CurrentPanel;
        public UserControl p_CurrentPanel
        {
            get
            {
                return m_CurrentPanel;
            }
            set
            {
                SetProperty(ref m_CurrentPanel, value);
            }
        }
        #endregion
    }
}
