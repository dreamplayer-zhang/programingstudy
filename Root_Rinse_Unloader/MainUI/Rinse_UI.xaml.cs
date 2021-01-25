using Root_Rinse_Unloader.Module;
using RootTools;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace Root_Rinse_Unloader.MainUI
{
    /// <summary>
    /// Rinse_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Rinse_UI : UserControl
    {
        public Rinse_UI()
        {
            InitializeComponent();
        }

        RinseU m_rinse;
        public void Init(RinseU rinse)
        {
            m_rinse = rinse;
            DataContext = rinse;
            comboBoxMode.ItemsSource = Enum.GetValues(typeof(RinseU.eRunMode));
            textBlockState.DataContext = EQ.m_EQ;
        }

        private void textBoxWidth_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;
            DependencyProperty property = TextBox.TextProperty;
            BindingExpression binding = BindingOperations.GetBindingExpression((TextBox)sender, property);
            if (binding != null) binding.UpdateSource();
        }
    }
}
