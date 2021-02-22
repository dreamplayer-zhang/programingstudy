using System.Collections.Generic;
using System.Windows.Controls;

namespace RootTools.Lens
{
    /// <summary>
    /// ToolSetLens_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ToolSetLens_UI : UserControl
    {
        public ToolSetLens_UI()
        {
            InitializeComponent();
        }

        ToolSetLens m_toolSetLens;
        public void Init(ToolSetLens toolSetLens)
        {
            m_toolSetLens = toolSetLens;
            this.DataContext = toolSetLens;
            toolSetLens.OnToolChanged += LightToolSet_OnToolChanged;
            InitTabControl();
        }

        private void LightToolSet_OnToolChanged()
        {
            InitTabControl();
        }

        List<string> m_asLens = new List<string>();
        void InitTabControl()
        {
            tabControl.Items.Clear();
            m_asLens.Clear();
            comboLens.ItemsSource = null;
            foreach (ILens Lens in m_toolSetLens.m_aLens)
            {
                TabItem tabItem = new TabItem();
                tabItem.Header = Lens.p_id;
                tabItem.Height = 0;
                tabItem.Content = Lens.p_ui;
                tabControl.Items.Add(tabItem);
                m_asLens.Add(Lens.p_id);
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
