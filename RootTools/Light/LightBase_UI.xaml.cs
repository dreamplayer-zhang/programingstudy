using System;
using System.Windows.Controls;
using System.Windows.Input;

namespace RootTools.Light
{
    /// <summary>
    /// ILight_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class LightBase_UI : UserControl
    {
        public LightBase_UI()
        {
            InitializeComponent();
        }

        LightBase m_light;
        public void Init(LightBase light)
        {
            m_light = light;
            this.DataContext = light;
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) ((TextBox)sender).MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
        }
    }
}
