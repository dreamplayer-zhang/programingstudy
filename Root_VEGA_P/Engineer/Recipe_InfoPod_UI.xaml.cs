using Root_VEGA_P_Vision.Module;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Root_VEGA_P.Engineer
{
    /// <summary>
    /// VEGA_P_Recipe_IRTRChild_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Recipe_InfoPod_UI : UserControl
    {
        public Recipe_InfoPod_UI()
        {
            InitializeComponent();
        }

        public bool m_bInitExist = false; 
        bool _bExist = false; 
        public bool p_bExist
        {
            get { return _bExist; }
            set
            {
                if (_bExist == value) return;
                _bExist = value;
                textBlock.Foreground = value ? Brushes.Black : Brushes.LightGray;
            }
        }

        VEGA_P_Recipe m_recipe; 
        IRTRChild m_child; 
        public void Init(string id, bool bExist, VEGA_P_Recipe recipe, IRTRChild child)
        {
            textBlock.Text = id;
            textBlock.Foreground = Brushes.LightGray;
            m_bInitExist = bExist; 
            p_bExist = bExist;
            m_recipe = recipe; 
            m_child = child; 
            textBlock.PreviewMouseLeftButtonDown += TextBlock_PreviewMouseLeftButtonDown;
            textBlock.PreviewMouseMove += TextBlock_PreviewMouseMove;
            textBlock.Drop += TextBlock_Drop;
        }

        public void ClearRecipe()
        {
            p_bExist = m_bInitExist;
        }

        bool m_bDrag = false; 
        private void TextBlock_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            TextBlock block = sender as TextBlock;
            m_bDrag = (block.Foreground == Brushes.Black); 
        }

        private void TextBlock_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (!m_bDrag || (e.LeftButton != MouseButtonState.Pressed)) return;
            m_bDrag = false;
            DataObject obj = new DataObject("Recipe_InfoPod", this);
            System.Windows.DragDrop.DoDragDrop(textBlock, obj, DragDropEffects.Move); 
        }

        private void TextBlock_Drop(object sender, DragEventArgs e)
        {
            Recipe_InfoPod_UI from = (Recipe_InfoPod_UI)e.Data.GetData("Recipe_InfoPod"); 
            if (from.textBlock.Text != textBlock.Text) return;
            if (from.p_bExist == false) return; 
            if (p_bExist) return; 
            from.p_bExist = false;
            p_bExist = true;
            m_recipe.AddRunGetPut(from.m_child.p_id, m_child.p_id); 
        }
    }
}
