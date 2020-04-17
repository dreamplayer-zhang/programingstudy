using RootTools.Trees;
using System.Windows.Controls;

namespace RootTools.RTC5s.LaserBright
{
    /// <summary>
    /// Laser_Bright_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Laser_Bright_UI : UserControl
    {
        public Laser_Bright_UI()
        {
            InitializeComponent();
        }

        Laser_Bright m_laser;
        public void Init(Laser_Bright laser)
        {
            m_laser = laser;
            this.DataContext = laser;
            listDIUI.Init(laser.m_listDI);
            listDOUI.Init(laser.m_listDO);
            treeUI.Init(laser.m_treeRoot);
            laser.RunTree(Tree.eMode.Init);
            RTC5UI.Init(laser.m_RTC5);
        }
    }
}
