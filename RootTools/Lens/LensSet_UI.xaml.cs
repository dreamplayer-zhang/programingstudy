using System.Collections.Generic;
using System.Windows.Controls;

namespace RootTools.Lens
{
    /// <summary>
    /// LensSet_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class LensSet_UI : UserControl
    {
        public LensSet_UI()
        {
            InitializeComponent();
        }

        LensSet m_LensSet;
        public void Init(LensSet LensSet)
        {
            m_LensSet = LensSet;
            this.DataContext = LensSet;
            InitTabControl();
            LensSet.OnChangeTool += LensSet_OnChangeTool;
        }

        private void LensSet_OnChangeTool()
        {
            InitTabControl();
        }

        List<string> m_asLens = new List<string>();
        void InitTabControl()
        {
            tabControl.Items.Clear();
            m_asLens.Clear();
            comboLens.ItemsSource = null;
            foreach (ILens Lens in m_LensSet.m_aLens)
            {
                TabItem tabItem = new TabItem();
                tabItem.Header = Lens.p_id.Replace(m_LensSet.m_sModule + ".", "");
                tabItem.Content = Lens.p_ui;
                tabItem.Height = 0;
                tabControl.Items.Add(tabItem);
                m_asLens.Add(Lens.p_id.Replace(m_LensSet.m_sModule + ".", ""));
            }
            comboLens.ItemsSource = m_asLens;
            comboLens.SelectedIndex = 0;
        }

        private void comboLens_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (comboLens.SelectedIndex < 0) return;
            tabControl.SelectedIndex = comboLens.SelectedIndex;
        }
    }
}
