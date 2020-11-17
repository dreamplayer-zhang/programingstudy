using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Root_CAMELLIA.Draw
{
    public class TextManager
    {
        public TextBlock Text { get; private set; }
        public TextManager()
        {
            Text = new TextBlock();
        }
        public TextManager(Brush foreground)
        {
            Text = new TextBlock();
            Text.Foreground = foreground;
            Text.FontSize = 9;
            Text.FontWeight = FontWeights.DemiBold;
            Text.FontFamily = new FontFamily("century gothic");
        }
        public void SetBackGround(Brush background)
        {
            Text.Background = background;
        }
        public void SetData(string text, int fontSize = 9, double left = 0, double top = 0, System.Windows.HorizontalAlignment horizontalAlign = System.Windows.HorizontalAlignment.Center,
            System.Windows.VerticalAlignment verticalAlign = System.Windows.VerticalAlignment.Center)
        {
            Text.Background = Brushes.Blue;
            Text.Text = text;
            Text.HorizontalAlignment = horizontalAlign;
            Text.VerticalAlignment = verticalAlign;
            Text.FontSize = fontSize;
            Canvas.SetLeft(Text, left);
            Canvas.SetTop(Text, top);
        }
        public void SetVisibility(bool isVisible = true)
        {
            if (isVisible)
            {
                Text.Visibility = Visibility.Visible;
            }
            else
            {
                Text.Visibility = Visibility.Hidden;
            }
        }
    }
}
