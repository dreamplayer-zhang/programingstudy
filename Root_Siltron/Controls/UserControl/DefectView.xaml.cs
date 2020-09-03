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

namespace Root_Siltron
{
    /// <summary>
    /// DefectView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class DefectView : UserControl
    {
        public DefectView()
        {
            InitializeComponent();
            AddDefect(gridFront, 0);
            AddDefect(gridFront, 10);
            AddDefect(gridFront, 20);
            AddDefect(gridBack, 7);
            AddDefect(gridBack, -11);
            AddDefect(gridBack, -15);


        }

        public void init(bool useFront, bool useBack, bool useEdge, bool useEBR)
        {
            FrontOption.Visibility = VisibleOption(useFront);
            BackOption.Visibility = VisibleOption(useBack);
            EdgeOption.Visibility = VisibleOption(useEdge);
            EBROption.Visibility = VisibleOption(useEBR);
        }

        public void AddDefect(Grid gridArea, double theta)
        {
            Rectangle defect = new Rectangle();
            defect.Width = 5;
            defect.Height = 5;
            defect.Fill = Brushes.Red;
            defect.Stroke = Brushes.Black;
            defect.StrokeThickness = 0.5;
            defect.VerticalAlignment = VerticalAlignment.Bottom;
            defect.RenderTransformOrigin = new Point(0.5, -50);
            RotateTransform rotate = new RotateTransform(theta);
            TransformGroup transformGroup = new TransformGroup();
            transformGroup.Children.Add(rotate);
            defect.RenderTransform = transformGroup;

            gridArea.Children.Add(defect);             
        }
        public void AddDefectList(List<Defect> listDefect)
        {
            foreach (Defect defect in listDefect)
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
    }
}
