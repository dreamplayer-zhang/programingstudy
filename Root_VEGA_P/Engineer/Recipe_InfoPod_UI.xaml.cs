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

        bool m_bFlip = false;
        bool m_bRunFlip = false; 

        public bool m_bInitExist = false; 
        bool _bExist = false; 
        public bool p_bExist
        {
            get { return _bExist; }
            set
            {
                if (_bExist == value) return;
                _bExist = value;
                textBlock.Foreground = value ? (m_bFlip ? Brushes.Red : Brushes.Black) : Brushes.LightGray;
            }
        }

        InfoPod.ePod m_ePod = InfoPod.ePod.EOP_Door; 
        VEGA_P_Recipe m_recipe; 
        IRTRChild m_child; 
        public void Init(InfoPod.ePod ePod, bool bExist, bool bRunFlip, VEGA_P_Recipe recipe, IRTRChild child)
        {
            m_ePod = ePod; 
            textBlock.Text = ePod.ToString();
            textBlock.Foreground = Brushes.LightGray;
            m_bInitExist = bExist;
            m_bRunFlip = bRunFlip; 
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
            m_bDrag = (block.Foreground != Brushes.LightGray); 
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
            if (from.m_ePod != m_ePod) return;
            if (from.p_bExist == false) return; 
            if (p_bExist) return;
            m_bFlip = from.m_bFlip;
            if (m_bRunFlip) m_bFlip = !m_bFlip; 
            from.p_bExist = false;
            p_bExist = true;
            m_recipe.AddRunGetPut(from.m_child.p_id, m_child.p_id); 
        }
    }
}
