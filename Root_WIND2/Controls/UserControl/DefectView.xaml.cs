using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Root_WIND2
{
    /// <summary>
    /// DefectView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class DefectView : UserControl
    {
        public DefectView()
        {
            InitializeComponent();
        }

        public void init(bool useFront, bool useBack, bool useEdge, bool useEBR)
        {
            FrontOption.Visibility = VisibleOption(useFront);
            BackOption.Visibility = VisibleOption(useBack);
            EdgeOption.Visibility = VisibleOption(useEdge);
            EBROption.Visibility = VisibleOption(useEBR);
        }

        public void AddDefectFront(double theta)
        {
            AddDefect(gridFront, theta);
        }
    
        public void AddDefect(Grid gridArea, double theta)
        {
            Rectangle defect = new Rectangle();
            defect.Width = 10;
            defect.Height = 10;
            if(gridArea == gridFront)
                defect.Fill = Brushes.Red;
            if (gridArea == gridBack)
                defect.Fill = Brushes.Blue;
            if (gridArea == gridEdge)
                defect.Fill = Brushes.Green;

            defect.Stroke = Brushes.Black;
            defect.StrokeThickness = 0.5;
            defect.VerticalAlignment = VerticalAlignment.Bottom;
            defect.RenderTransformOrigin = new Point(0.5, -49);
            RotateTransform rotate = new RotateTransform(theta);
            TransformGroup transformGroup = new TransformGroup();
            transformGroup.Children.Add(rotate);
            defect.RenderTransform = transformGroup;

            gridArea.Children.Add(defect);             
        }
        public void AddDefectList(List<EdgeDefect> listDefect)
        {
            foreach (EdgeDefect defect in listDefect)
            {
                switch (defect.m_eDirection)
                {
                    case eDirection.Front:
                        {
                            AddDefect(gridFront, defect.m_dTheta);
                            break;
                        }
                    case eDirection.Back:
                        {
                            AddDefect(gridBack, defect.m_dTheta);
                            break;
                        }
                    case eDirection.Side:
                        {
                            AddDefect(gridEdge, defect.m_dTheta);
                            break;
                        }
                }
            }
        }


        private Visibility VisibleOption(bool use)
        {
            if (use)
                return Visibility.Visible;
            else
                return Visibility.Collapsed;
            
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            gridFront.Children.Clear();
            gridBack.Children.Clear();
            gridEdge.Children.Clear();
            Random random = new Random(1);
            Random random2 = new Random(3);
            Random random3 = new Random(7);
            for (int i = 0; i < 150; i++)
            {

                int r1 = random.Next(-360, 360);
                int r2 = random2.Next(-360, 360);
                int r3 = random3.Next(-360, 360);
                AddDefect(gridFront, r1);
                AddDefect(gridBack, r2);
                AddDefect(gridEdge, r3);

            }
        }
    }
}
