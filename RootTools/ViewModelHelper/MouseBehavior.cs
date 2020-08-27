using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;


namespace mousebehavior
{
    public class KeyBehaviour : Behavior<FrameworkElement>
    {
        public static readonly DependencyProperty KeyArgs = DependencyProperty.Register(
            "KeyEvent", typeof(KeyEventArgs), typeof(KeyBehaviour), new PropertyMetadata(default(KeyEventArgs)));
        
        public KeyEventArgs KeyEvent
        {
            get
            {
                return (KeyEventArgs)GetValue(KeyArgs);
            }
            set
            {
                SetValue(KeyArgs, value);
            }
        }
        protected override void OnAttached()
        {
            AssociatedObject.PreviewKeyDown += AssociatedObjectKeyDown;
        }
        private void AssociatedObjectKeyDown(object sender, KeyEventArgs e)
        {
            KeyEvent = e; 
        }
        protected override void OnDetaching()
        {
            AssociatedObject.PreviewKeyDown -= AssociatedObjectKeyDown;
        }
    }
    public class MouseBehaviour : Behavior<FrameworkElement>
    {
        public static readonly DependencyProperty MouseArgs = DependencyProperty.Register(
            "MouseEvent", typeof(MouseEventArgs), typeof(MouseBehaviour), new PropertyMetadata(default(MouseEventArgs)));

        public MouseEventArgs MouseEvent
        {
            get
            {
                return (MouseEventArgs)GetValue(MouseArgs);
            }
            set
            {
                SetValue(MouseArgs, value);
            }
        }

        public static readonly DependencyProperty MouseYProperty = DependencyProperty.Register(
            "MouseY", typeof(int), typeof(MouseBehaviour), new PropertyMetadata(default(int)));

        public int MouseY
        {
            get
            {
                return (int)GetValue(MouseYProperty);
            }
            set
            {
                SetValue(MouseYProperty, value);
            }
        }

        public static readonly DependencyProperty MouseXProperty = DependencyProperty.Register(
            "MouseX", typeof(int), typeof(MouseBehaviour), new PropertyMetadata(default(int)));

        public int MouseX
        {
            get
            {
                return (int)GetValue(MouseXProperty);
            }
            set
            {
                SetValue(MouseXProperty, value);
            }
        }

        protected override void OnAttached()
        {
            AssociatedObject.MouseMove += AssociatedObjectOnMouseMove;
        }

        private void AssociatedObjectOnMouseMove(object sender, System.Windows.Input.MouseEventArgs mouseEventArgs)
        {
            var pos = mouseEventArgs.GetPosition(AssociatedObject);
            MouseX = Convert.ToInt32(pos.X);
            MouseY = Convert.ToInt32(pos.Y);
            MouseEvent = mouseEventArgs;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.MouseMove -= AssociatedObjectOnMouseMove;
        }
    }
}
